using System;
using System.Collections.Generic;
using System.Linq;
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
        public Vector3 contact;
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
        static protected int maxEPAIteration = 5;

        protected bool moved;
        public bool Moved { get { return moved; } }
        public Bounds bounds { get { return mesh.bounds; } }
        public Bounds worldBounds { get { return renderer.bounds; } }
        public CustomRigidbody RB { get { return customRigidbody; } }
        public CustomPhysicMaterial PM { get { return physicsMaterial; } }

        [SerializeField] protected bool bShowAABBBox;

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
        public static int maxIteration = 5;

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

            // First point creation
            Vector3 dir = Vector3.forward;
            Vector3 support = A.Support(dir) - B.Support(-dir);
            tetrahedron[0] = support;

            // Second point creation
            dir = -support.normalized;
            support = A.Support(dir) - B.Support(-dir);
            tetrahedron[1] = support;

            // Third point creation
            Vector3 line = tetrahedron[1] - tetrahedron[0];
            Vector3 projection = Vector3.ProjectOnPlane(Vector3.zero, line);
            dir = -projection.normalized;
            support = A.Support(dir) - B.Support(-dir);
            tetrahedron[2] = support;

            // Last point creation
            dir = Vector3.Cross(tetrahedron[1] - tetrahedron[0], tetrahedron[2] - tetrahedron[0]);
            Vector3 toZero = Vector3.zero - tetrahedron[2];
            if (Vector3.Dot(dir, toZero) < 0)
                dir = -dir;
            support = A.Support(dir) - B.Support(-dir);
            tetrahedron[3] = support;

            return tetrahedron;
        }

        // GJK Iteration
        public static CollisionInfo CheckCollision(CustomCollider A, CustomCollider B)
        {
            Vector3[] tetrahedron = GenerateTetrahedron(A, B);
            CollisionInfo collisionInfo = new CollisionInfo();
            collisionInfo.objectA = A;
            collisionInfo.objectB = B;

            foreach (Vector3 point in tetrahedron)
            {
                if (IsOrigin(point))
                    return collisionInfo;
            }

            if (PointInTetrahedron(tetrahedron, Vector3.zero))
            {
                CustomPhysicEngine.collidingTethraedron = tetrahedron;
                collisionInfo.contact = A.Support(B.gameObject.transform.position - A.gameObject.transform.position);

                Vector3 contactOther = B.Support(A.gameObject.transform.position - B.gameObject.transform.position);
                collisionInfo.penetration = Vector3.Distance(collisionInfo.contact, contactOther);
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

                Vector3 supportA = A.Support(dir);
                Vector3 supportB = B.Support(-dir);
                Vector3 support = supportA - supportB;
                tetrahedron[3] = support;

                if (tetrahedron[3] == tetrahedron[2])
                    return null;

                if (PointInTetrahedron(lastTetrahedron, tetrahedron[3]))
                    return null;

                if (PointInTetrahedron(tetrahedron, Vector3.zero))
                {
                    CustomPhysicEngine.collidingTethraedron = tetrahedron;
                    collisionInfo.contact = A.Support(B.gameObject.transform.position - A.gameObject.transform.position);

                    if(A.RB != null || B.RB != null)
                    {
                        EPA(A, B, tetrahedron, ref collisionInfo);
                    }
                    //Vector3 contactOther = B.Support(A.gameObject.transform.position - B.gameObject.transform.position);
                    //collisionInfo.penetration = Vector3.Distance(collisionInfo.contact, contactOther);

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

            while(minDistance == float.MaxValue && iteration < maxEPAIteration)
            {
                iteration++;
                minNormal = new Vector3(normals[minFace].x, normals[minFace].y, normals[minFace].z);
                minDistance = normals[minFace].w;

                Vector3 support = A.Support(minNormal) - B.Support(-minNormal);
                float sDistance = Vector3.Dot(minNormal, support);

                if (Mathf.Abs(sDistance - minDistance) > 0.1f)
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

                    if (newNormals[newMinFace].w < oldMinDistance)
                    {
                        minFace = newMinFace + normals.Count;
                    }

                    faces.InsertRange(faces.Count, newFaces);
                    normals.InsertRange(normals.Count, newNormals);
                }
            }

            collisionInfo.normal = -minNormal.normalized;
            collisionInfo.penetration = minDistance + 0.001f;

            if (iteration >= maxEPAIteration)
            {

                collisionInfo.normal = Vector3.zero;
                collisionInfo.penetration = 0;
            }
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
