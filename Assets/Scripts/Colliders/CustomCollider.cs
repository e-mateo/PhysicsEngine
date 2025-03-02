using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.TextCore.Text;

namespace CustomPhysic
{
    public class CollisionInfo
    {
        public CustomCollider objectA, objectB;
        public Vector3 contactA;
        public Vector3 contactB;
        public Vector3 normal;
        public float penetration;
    }

    abstract public class CustomCollider : MonoBehaviour
    {
        [SerializeField] protected CustomPhysicMaterial physicsMaterial;

        protected Mesh mesh;
        protected Renderer renderer;
        protected CustomRigidbody customRigidbody;
        protected Vector3 lastPosition;

        static protected float minDistanceToMove = 0.05f;
        static protected int maxEPAIteration = 30;

        protected bool moved;
        public bool Moved { get { return moved; } }
        public Bounds bounds { get { return mesh.bounds; } }
        public Bounds worldBounds { get { return renderer.bounds; } }
        public CustomRigidbody RB { get { return customRigidbody; } }
        public CustomPhysicMaterial PM { get { return physicsMaterial; } }

        [SerializeField] public bool bIsTrigger;

        [SerializeField] protected bool bShowAABBBox;

        protected Matrix4x4 InvInertiaTensor;

        protected virtual void SetInvInteriaTensor()
        {
            InvInertiaTensor = Matrix4x4.identity;
            InvInertiaTensor = InvInertiaTensor.inverse;
        }

        private void FixedUpdate()
        {
            if (Vector3.Distance(lastPosition, transform.position) > minDistanceToMove)
                moved = true;
            else 
                moved = false;


            lastPosition.Set(transform.position.x, transform.position.y, transform.position.z);
        }

        private void Awake()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            renderer = GetComponent<Renderer>();
            customRigidbody = GetComponent<CustomRigidbody>();
            lastPosition = transform.position;
            SetInvInteriaTensor();
            if(customRigidbody != null)
            {
                customRigidbody.SetLocalInertiaTensor(InvInertiaTensor);
            }
        }

        private void OnEnable()
        {
            if (CustomPhysicEngine.Instance)
            {
                CustomPhysicEngine.Instance.OnColliderEnable(this);
            }
        }

        private void OnDisable()
        {
            if (CustomPhysicEngine.Instance)
            {
                CustomPhysicEngine.Instance.OnColliderDisbale(this);
            }
        }

        public virtual Vector3 GetAABBExtends()
        {
            return worldBounds.extents;
        }

        #region Statics
        public static List<Vector3> MinkowskiDifference(CustomCollider A, CustomCollider B)
        {
            List<Vector3> vertices = new List<Vector3>();

            foreach (Vector3 vA in A.mesh.vertices)
            {
                foreach (Vector3 vB in B.mesh.vertices)
                {
                    vertices.Add((vA + A.transform.position) - (vB + B.transform.position));
                }
            }

            return vertices;
        }

        public static float precision = 0.001f;
        public static int maxIteration = 10;

        public static List<Vector3> supportA = new List<Vector3>();
        public static List<Vector3> supportB = new List<Vector3>();

        abstract protected Vector3 Support(Vector3 dir);

        private static bool IsOrigin(Vector3 point)
        {
            return point.magnitude < precision;
        }

        private static bool IsPointSameSideOfPlane(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 point)
        {
            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1);
            Vector3 center = new Vector3((v1.x + v2.x + v3.x) / 3,
                                            (v1.y + v2.y + v3.y) / 3,
                                            (v1.z + v2.z + v3.z) / 3);

            float dotv4 = Vector3.Dot(normal, v4 - center);

            // v4 is on the same plane
            if (dotv4 == 0)
                return false;

            float dotP = Vector3.Dot(normal, point - center);
            return Mathf.Sign(dotv4) == Mathf.Sign(dotP);
        }

        private static bool PointInTetrahedron(Vector3[] tetrahedron, Vector3 point)
        {
            return IsPointSameSideOfPlane(tetrahedron[0], tetrahedron[1], tetrahedron[2], tetrahedron[3], point) &&
                IsPointSameSideOfPlane(tetrahedron[1], tetrahedron[2], tetrahedron[3], tetrahedron[0], point) &&
                IsPointSameSideOfPlane(tetrahedron[2], tetrahedron[3], tetrahedron[0], tetrahedron[1], point) &&
                IsPointSameSideOfPlane(tetrahedron[3], tetrahedron[0], tetrahedron[1], tetrahedron[2], point);
        }

        public static Vector3[] GenerateTetrahedron(CustomCollider A, CustomCollider B)
        {
            Vector3[] tetrahedron = new Vector3[4];
            Vector3 SupA;
            Vector3 SupB;

            // First point creation
            Vector3 dir = Vector3.forward;
            SupA = A.Support(dir);
            SupB = B.Support(-dir);
            supportA.Add(SupA);
            supportB.Add(SupB);
            Vector3 support = SupA - SupB;
            tetrahedron[0] = support;

            // Second point creation
            dir = -support.normalized;
            SupA = A.Support(dir);
            SupB = B.Support(-dir);
            supportA.Add(SupA);
            supportB.Add(SupB);
            support = SupA - SupB;
            tetrahedron[1] = support;

            // Third point creation
            Vector3 line = tetrahedron[1] - tetrahedron[0];
            Vector3 projection = Vector3.ProjectOnPlane(Vector3.zero, line);
            dir = -projection.normalized;
            SupA = A.Support(dir);
            SupB = B.Support(-dir);
            supportA.Add(SupA);
            supportB.Add(SupB);
            support = SupA - SupB;
            tetrahedron[2] = support;

            // Last point creation
            dir = Vector3.Cross(tetrahedron[1] - tetrahedron[0], tetrahedron[2] - tetrahedron[0]);
            Vector3 toZero = Vector3.zero - tetrahedron[2];
            if (Vector3.Dot(dir, toZero) < 0)
                dir = -dir;
            SupA = A.Support(dir);
            SupB = B.Support(-dir);
            supportA.Add(SupA);
            supportB.Add(SupB);
            support = SupA - SupB;
            tetrahedron[3] = support;

            return tetrahedron;
        }

        // GJK Iteration
        public static CollisionInfo CheckCollision(CustomCollider A, CustomCollider B)
        {
            supportA.Clear();
            supportB.Clear();

            Vector3[] tetrahedron = GenerateTetrahedron(A, B);
            CollisionInfo collisionInfo = new CollisionInfo();
            collisionInfo.objectA = A;
            collisionInfo.objectB = B;

            foreach (Vector3 point in tetrahedron)
            {
                if (IsOrigin(point))
                {
                    CustomPhysicEngine.collidingTethraedron = tetrahedron;
                    if (A.RB != null || B.RB != null)
                    {
                        EPA(A, B, tetrahedron, ref collisionInfo);
                    }

                    return collisionInfo;
                }
            }

            if (PointInTetrahedron(tetrahedron, Vector3.zero))
            {
                CustomPhysicEngine.collidingTethraedron = tetrahedron;
                if (A.RB != null || B.RB != null)
                {
                    EPA(A, B, tetrahedron, ref collisionInfo);
                }
                return collisionInfo;
            }

            // Start loop over simplex
            Vector3[] lastTetrahedron = new Vector3[4]; ;
            for (int i = 0; i < maxIteration; i++)
            {
                tetrahedron.CopyTo(lastTetrahedron, 0);
                tetrahedron = new Vector3[4];
                tetrahedron[0] = lastTetrahedron[1];
                tetrahedron[1] = lastTetrahedron[2];
                tetrahedron[2] = lastTetrahedron[3];

                // Calc the last point again
                Vector3 dir = Vector3.Cross(tetrahedron[1] - tetrahedron[0], tetrahedron[2] - tetrahedron[0]);
                Vector3 toZero = Vector3.zero - tetrahedron[0];
                if (Vector3.Dot(dir, toZero) < 0)
                    dir = -dir;

                Vector3 supA = A.Support(dir);
                Vector3 supB = B.Support(-dir);
                supportA.Add(supA);
                supportB.Add(supB);
                supportA.RemoveAt(0);
                supportB.RemoveAt(0);
                Vector3 support = supA - supB;
                tetrahedron[3] = support;

                if (tetrahedron[3] == tetrahedron[2])
                    return null;

                if (PointInTetrahedron(lastTetrahedron, tetrahedron[3]))
                    return null;

                if (PointInTetrahedron(tetrahedron, Vector3.zero))
                {
                    CustomPhysicEngine.collidingTethraedron = tetrahedron;

                    if(A.RB != null || B.RB != null)
                    {
                        EPA(A, B, tetrahedron, ref collisionInfo);
                    }

                    return collisionInfo;
                }
            }

            return null;
        }

        public static void EPA(CustomCollider A, CustomCollider B, Vector3[] Tetrahedron, ref CollisionInfo collisionInfo)
        {
            List<int> faces = new List<int>()
            {
                0, 1, 2,
                0, 3, 1,
                0, 2, 3,
                1, 3, 2,
            };

            List<Vector3> polytope = new List<Vector3>(Tetrahedron.ToList());
            List<Vector4> normals = new List<Vector4>();
            int minFace;
            (normals, minFace) = GetFaceNormals(polytope, faces);

            Vector3 minNormal = Vector3.zero;
            float minDistance = float.MaxValue;
            int iteration = 0;

            while(minDistance == float.MaxValue && iteration < maxEPAIteration && faces.Count > 0)
            {
                iteration++;
                minNormal = new Vector3(normals[minFace].x, normals[minFace].y, normals[minFace].z);
                minDistance = normals[minFace].w;

                Vector3 supA = A.Support(minNormal);
                Vector3 supB = B.Support(-minNormal);

                Vector3 support = A.Support(minNormal) - B.Support(-minNormal);
                float sDistance = Vector3.Dot(minNormal, support);

                if (Mathf.Abs(sDistance - minDistance) > 0.001f)
                {
                    minDistance = float.MaxValue;

                    List<Tuple<int,int>> uniqueEdges = new List<Tuple<int,int>>();
                    for(int i = 0; i < normals.Count; i++)
                    {
                        if (SameDirection(new Vector3(normals[i].x, normals[i].y, normals[i].z), support))
                        {
                            int f = i * 3;
                            AddIfUniqueEdge(ref uniqueEdges, ref faces, f, f + 1);
                            AddIfUniqueEdge(ref uniqueEdges, ref faces, f + 1, f + 2);
                            AddIfUniqueEdge(ref uniqueEdges, ref faces, f + 2, f);

                            faces[f + 2] = faces[faces.Count - 1]; faces.RemoveAt(faces.Count - 1);
                            faces[f + 1] = faces[faces.Count - 1]; faces.RemoveAt(faces.Count - 1);
                            faces[f] = faces[faces.Count - 1]; faces.RemoveAt(faces.Count - 1);

                            normals[i] = normals[normals.Count - 1]; normals.RemoveAt(normals.Count - 1);

                            i--;
                        }
                    }

                    List<int> newFaces = new List<int>();
                    foreach(Tuple<int, int> uniqueEdge in uniqueEdges)
                    {
                        newFaces.Add(uniqueEdge.Item1);
                        newFaces.Add(uniqueEdge.Item2);
                        newFaces.Add(polytope.Count);
                    }
                    polytope.Add(support);
                    supportA.Add(supA);
                    supportB.Add(supB);
                    List<Vector4> newNormals = new List<Vector4>();
                    int newMinFace;
                    (newNormals, newMinFace) = GetFaceNormals(polytope, newFaces);


                    float oldMinDistance = float.MaxValue;
                    for(int i = 0; i < normals.Count; i++)
                    {
                        if (normals[i].w < oldMinDistance)
                        {
                            oldMinDistance = normals[i].w;
                            minFace = i;
                        }
                    }

                    if (newNormals.Count > newMinFace && newNormals[newMinFace].w < oldMinDistance)
                    {
                        minFace = newMinFace + normals.Count;
                    }

                    faces.InsertRange(faces.Count, newFaces);
                    normals.InsertRange(normals.Count, newNormals);
                }
            }

            if (iteration >= maxEPAIteration || faces.Count == 0)
            {
                Debug.Log("MaxIteration");
                collisionInfo.normal = Vector3.zero;
                collisionInfo.penetration = 0;
                collisionInfo.contactA = A.transform.position;
                collisionInfo.contactB = B.transform.position;
                return;
            }

            (collisionInfo.contactA, collisionInfo.contactB) = GetContactPoints(polytope, faces, minFace, minNormal);
            collisionInfo.normal = -minNormal.normalized;
            collisionInfo.penetration = minDistance + 0.001f;
        }

        public static bool IsPointInsideTriangle(Vector3 P, Vector3 A, Vector3 B, Vector3 C)
        {
            float Area = TriangleArea(A, B, C);
            float subArea1 = TriangleArea(A, B, P);
            float subArea2 = TriangleArea(A, C, P);
            float subArea3 = TriangleArea(B, C, P);
            float total = subArea1 + subArea2 + subArea3;

            if (total == Area)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static float TriangleArea(Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 AB = B - A;
            Vector3 AC = C - A;
            return 0.5f * Mathf.Sqrt(Mathf.Pow((AB.y * AC.z) - (AB.z * AC.y), 2f) + Mathf.Pow((AB.z * AC.x) - (AB.x * AC.z), 2f) + Mathf.Pow((AB.x * AC.y) - (AB.y * AC.x), 2f));
        }

        public static (Vector3, Vector3) GetContactPoints(List<Vector3> polytope, List<int> faces, int minFace, Vector3 minNormal)
        {
            int index0 = faces[minFace * 3];
            int index1 = faces[minFace * 3 + 1];
            int index2 = faces[minFace * 3 + 2];

            Vector3 tA = polytope[index0];
            Vector3 tB = polytope[index1];
            Vector3 tC = polytope[index2];
            Plane plane = new Plane(minNormal.normalized, tA);
            Vector3 Cp = plane.ClosestPointOnPlane(Vector3.zero);
            if (!IsPointInsideTriangle(Cp, tA, tB, tC))
            {
                List<Vector3> projectedPoint = new List<Vector3>();
                projectedPoint.Add(Vector3.Project(Cp, tB - tA));
                projectedPoint.Add(Vector3.Project(Cp, tC - tA));
                projectedPoint.Add(Vector3.Project(Cp, tB - tC));

                Vector3 nearest = projectedPoint[0];
                float minDist = float.MaxValue;

                for (int i = 0; i < projectedPoint.Count; i++)
                {
                    float dist = Vector3.Distance(Cp, projectedPoint[i]);
                    if(dist < minDist)
                    {
                        minDist = dist;
                        nearest = projectedPoint[i];
                    }
                }

                Cp = nearest;
            }

            float x = 0f, y = 0f, z = 0f;
            Barycentric(Cp, tA, tB, tC, ref x, ref y, ref z);
            Vector3 Ap = x * supportA[index0] + y * supportA[index1] + z * supportA[index2];
            Vector3 Bp = x * supportB[index0] + y * supportB[index1] + z * supportB[index2];
            return (Ap, Bp);
        }


        public static void Barycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c, ref float u, ref float v, ref float w)
        {
            u = TriangleArea(b, c, p) / TriangleArea(a, b, c);
            v = TriangleArea(c, a, p) / TriangleArea(a, b, c);
            w = TriangleArea(a, b, p) / TriangleArea(a, b, c);
        }


        public static (List<Vector4>, int) GetFaceNormals(List<Vector3> polytope, List<int> faces)
        {
            List<Vector4> normals = new List<Vector4>();
            int minTriangle = 0;
            float minDistance = float.MaxValue;

            for(int i = 0; i < faces.Count; i+= 3)
            {
                Vector3 a = polytope[faces[i]];
                Vector3 b = polytope[faces[i + 1]];
                Vector3 c = polytope[faces[i + 2]];

                Vector3 normal = Vector3.Cross((b - a), (c - a)).normalized;
                float distance = Vector3.Dot(normal, a);

                if(distance < 0)
                {
                    normal *= -1;
                    distance *= -1;
                }

                normals.Add(new Vector4(normal.x, normal.y, normal.z, distance));

                if(distance < minDistance)
                {
                    minTriangle = i / 3;
                    minDistance = distance;
                }
            }

            return (normals, minTriangle);
        }

        public static void AddIfUniqueEdge(ref List<Tuple<int, int>> edges, ref List<int> faces, int a, int b)
        {
            int reverse = -1;
            for(int i = 0; i < edges.Count; i++)
            {
                if (edges[i].Item1 == faces[b] && edges[i].Item2 == faces[a])
                {
                    reverse = i;
                    break;
                }
            }

            if (reverse >= 0)
            {
                edges.RemoveAt(reverse);
            }
            else
            {
                edges.Add(Tuple.Create(faces[a], faces[b]));
            }
        }

        public static bool SameDirection(Vector3 a, Vector3 b)
        {
            return Vector3.Dot(a.normalized, b.normalized) >= 0;
        }
    }
        #endregion


   
}
