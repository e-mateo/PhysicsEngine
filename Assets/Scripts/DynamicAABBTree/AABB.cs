using UnityEngine;

public struct AABB 
{
    public Vector3 LowerBound { get; private set; }
    public Vector3 UpperBound { get; private set; }
    public Vector3 Center { get; private set; }
    public Vector3 Extend { get; private set; }
    public CustomRigidbody Body { get; private set; }
    

    public AABB(Vector3 lowerBound, Vector3 upperBound)
    {
        LowerBound = lowerBound;
        UpperBound = upperBound;
        Center = (upperBound - lowerBound) / 2f + lowerBound;
        Extend = upperBound - lowerBound;
        Body = null;
    }

    public static AABB Merge(AABB a, AABB b)
    {
        Vector3 lowerBound = Vector3.Min(a.LowerBound, b.LowerBound);
        Vector3 upperBound = Vector3.Max(a.UpperBound, b.UpperBound);
        return new AABB(lowerBound, upperBound);
    }

    public static float GetAreaUnion(AABB a, AABB b)
    {
        AABB mergedAABB = Merge(a, b);
        return mergedAABB.GetArea();
    }

    public float GetArea()
    {
        Vector3 diagonal = UpperBound - LowerBound;
        return 2.0f * (diagonal.x * diagonal.y + diagonal.y * diagonal.z + diagonal.z * diagonal.x);
    }

    public void BindRigidbody(CustomRigidbody rigidbody)
    {
        Body = rigidbody;
    }
}
