using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

abstract public class CustomCollider : MonoBehaviour
{
    public static float precision = 0.001f;
    public static int maxIteration = 20;

    abstract protected Vector3 Support(Vector3 dir);

    private static bool IsOrigin(Vector3 point)
    {
        return point.magnitude < precision;
    }

    private static bool IsPointSameSideOfPlane(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 point)
    {
        Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1);
        float dotv4 = Vector3.Dot(normal, v4 - v1);
        float dotP = Vector3.Dot(normal, point - v1);
        return Mathf.Sign(dotv4) == Mathf.Sign(dotP);
    }

    private static bool PointInTetrahedron(Vector3[] tetrahedron, Vector3 point) 
    {
        return IsPointSameSideOfPlane(tetrahedron[0], tetrahedron[1], tetrahedron[2], tetrahedron[3], point) &&
            IsPointSameSideOfPlane(tetrahedron[1], tetrahedron[2], tetrahedron[3], tetrahedron[0], point) &&
            IsPointSameSideOfPlane(tetrahedron[2], tetrahedron[3], tetrahedron[0], tetrahedron[1], point) &&
            IsPointSameSideOfPlane(tetrahedron[3], tetrahedron[0], tetrahedron[1], tetrahedron[2], point);
    }

    private static Vector3[] GenerateTetrahedron(CustomCollider A, CustomCollider B)
    {
        Vector3[] tetrahedron = new Vector3[4];

        // First point creation
        Vector3 dir = Vector3.forward;
        Vector3 support = A.Support(dir) - B.Support(-dir);
        tetrahedron[0] = support;

        // Second point creation
        dir = support.normalized;
        support = A.Support(dir) - B.Support(-dir);
        tetrahedron[1] = support;

        // Third point creation
        Vector3 line = tetrahedron[1] - tetrahedron[0];
        Vector3 projection = Vector3.ProjectOnPlane(Vector3.zero, line);
        dir = -projection.normalized;
        support = A.Support(dir) - B.Support(-dir);
        tetrahedron[2] = support;

        // Last point creation
        dir = Vector3.Cross(dir, line);
        Vector3 toZero = Vector3.zero - tetrahedron[2];
        if (Vector3.Dot(dir, toZero) < 0)
            dir = -dir;
        support = A.Support(dir) - B.Support(-dir);
        tetrahedron[3] = support;

        return tetrahedron;
    }

    // GJK Iteration
    public static bool CheckCollision(CustomCollider A, CustomCollider B)
    {
        Vector3[] tetrahedron = GenerateTetrahedron(A, B);

        foreach (Vector3 point in tetrahedron)
        {
            if (IsOrigin(point))
                return true;
        }

        if (PointInTetrahedron(tetrahedron, Vector3.zero))
            return true;

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
            Vector3 toZero = Vector3.zero - tetrahedron[2];
            if (Vector3.Dot(dir, toZero) < 0)
                dir = -dir;
            Vector3 support = A.Support(dir) - B.Support(-dir);
            tetrahedron[3] = support;

            if (PointInTetrahedron(tetrahedron, Vector3.zero))
                return true;
            else if (PointInTetrahedron(lastTetrahedron, tetrahedron[3]))
                return false;
        }

        return false;
    }
}
