using UnityEngine;

public struct AABB 
{
    public Vector3 lowerBound;
    public Vector3 upperBound;
    public Vector3 center;
    public Vector3 extend;
    public CustomRigidbody body;

    public AABB(Vector3 lowerBound, Vector3 upperBound)
    {
        this.lowerBound = lowerBound;
        this.upperBound = upperBound;
        this.center = ((upperBound - lowerBound) / 2f) + lowerBound;
        this.extend = (upperBound - lowerBound);
        body = null;
    }

    public static AABB Merge(AABB A, AABB B)
    {
        AABB result = new AABB();
        result.lowerBound = Vector3.Min(A.lowerBound, B.lowerBound);
        result.upperBound = Vector3.Max(A.upperBound, B.upperBound);
        result.center = ((result.upperBound - result.lowerBound) / 2f) + result.lowerBound;
        result.extend = (result.upperBound - result.lowerBound);
        return result;
    }

    public static float GetAreaUnion(AABB A, AABB B)
    {
        AABB MergedAABB = Merge(A, B);
        return MergedAABB.GetArea();
    }

    public float GetArea()
    {
        Vector3 diagonal = upperBound - lowerBound;
        return 2.0f * (diagonal.x * diagonal.y + diagonal.y * diagonal.z + diagonal.z * diagonal.x);
    }
}
