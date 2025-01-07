using UnityEngine;

public struct AABB
{
    static float ENCAPSULATED_ADDED_EXTEND = 1.0f;
    public Vector3 Center { get; private set; }
    public Vector3 LowerBound { get; private set; }
    public Vector3 UpperBound { get; private set; }
    public Vector3 Extend { get; private set; }

    public Vector3 CenterEnlargedAABB { get; private set; }
    public Vector3 LowerBoundEnlargedAABB { get; private set; }
    public Vector3 UpperBoundEnlargedAABB { get; private set; }
    public Vector3 ExtendEnlargedAABB { get; private set; }

    public AABB(Vector3 center, Vector3 extend)
    {
        Center = center;
        Extend = extend;
        LowerBound = center - extend;
        UpperBound = center + extend;

        CenterEnlargedAABB = Center;
        ExtendEnlargedAABB = Extend + new Vector3(ENCAPSULATED_ADDED_EXTEND, ENCAPSULATED_ADDED_EXTEND, ENCAPSULATED_ADDED_EXTEND);
        LowerBoundEnlargedAABB = CenterEnlargedAABB - ExtendEnlargedAABB;
        UpperBoundEnlargedAABB = CenterEnlargedAABB + ExtendEnlargedAABB;
    }

    public void UpdateAABB(Vector3 center)
    {
        Center = center;
        LowerBound = center - Extend;
        UpperBound = center + Extend;
    }

    public void UpdateEnlargedAABB()
    {
        CenterEnlargedAABB = Center;
        ExtendEnlargedAABB = Extend + new Vector3(ENCAPSULATED_ADDED_EXTEND, ENCAPSULATED_ADDED_EXTEND, ENCAPSULATED_ADDED_EXTEND);
        LowerBoundEnlargedAABB = CenterEnlargedAABB - ExtendEnlargedAABB;
        UpperBoundEnlargedAABB = CenterEnlargedAABB + ExtendEnlargedAABB;
    }

    public bool HasExitEnlargedAABB()
    {
        if (LowerBound.x < LowerBoundEnlargedAABB.x || LowerBound.y < LowerBoundEnlargedAABB.y || LowerBound.z < LowerBoundEnlargedAABB.z
         || UpperBound.x > UpperBoundEnlargedAABB.x || UpperBound.y > UpperBoundEnlargedAABB.y || UpperBound.z > UpperBoundEnlargedAABB.z)
        {
            return true;
        }

        return false;
    }

    public static AABB Merge(AABB a, AABB b)
    {
        Vector3 lowerBound = Vector3.Min(a.LowerBound, b.LowerBound);
        Vector3 upperBound = Vector3.Max(a.UpperBound, b.UpperBound);
        return new AABB((upperBound - lowerBound) * 0.5f + lowerBound, (upperBound - lowerBound) * 0.5f);
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

    public static bool IsColliding(AABB a, AABB b)
    {
         return a.LowerBound.x <= b.UpperBound.x && a.UpperBound.x >= b.LowerBound.x
         && a.LowerBound.y <= b.UpperBound.y && a.UpperBound.y >= b.LowerBound.y
         && a.LowerBound.z <= b.UpperBound.z && a.UpperBound.z >= b.LowerBound.z;
    }
}
