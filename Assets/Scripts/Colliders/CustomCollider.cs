using UnityEngine;

abstract public class CustomCollider : MonoBehaviour
{
    public static float precision = 0.001f;

    abstract protected Vector3 Support(Vector3 dir);

    private static bool PointIsOrigin(Vector3 point)
    {
        return point.magnitude < precision;
    }

    private static bool PointIsInTriangle(Vector3[] triangle, Vector3 point) 
    {
        return false;
    }

    // GJK Iteration
    public static bool CheckCollision(CustomCollider A, CustomCollider B)
    {
        Vector3[] simplex = new Vector3[4];

        // First point creation
        Vector3 dir = Vector3.forward;
        Vector3 support = A.Support(dir) - B.Support(-dir);
        simplex[0] = support;

        // Second point creation
        dir = support.normalized;
        support = A.Support(dir) - B.Support(-dir);
        simplex[1] = support;

        // Third point creation

        // Last point creation

        // Start loop over simplex

        return false;
    }
}
