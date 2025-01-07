using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

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
        protected Mesh mesh;
        protected Renderer renderer;

        public Bounds bounds { get { return mesh.bounds; } }
        public Bounds worldBounds { get { return renderer.bounds; } }


        private void Awake()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            renderer = GetComponent<Renderer>();
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
                Vector3 support = A.Support(dir) - B.Support(-dir);
                tetrahedron[3] = support;

                if (tetrahedron[3] == tetrahedron[2])
                    return null;

                if (PointInTetrahedron(lastTetrahedron, tetrahedron[3]))
                    return null;

                if (PointInTetrahedron(tetrahedron, Vector3.zero))
                {
                    CustomPhysicEngine.collidingTethraedron = tetrahedron;

                    return collisionInfo;
                }
            }

            return null;
        }
        #endregion
    }
}
