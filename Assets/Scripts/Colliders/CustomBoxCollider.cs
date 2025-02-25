using UnityEngine;
using CustomPhysic;
using System.Collections.Generic;

public class CustomBoxCollider : CustomCollider
{
    [SerializeField] Vector3 extend = new Vector3(0.5f, 0.5f, 0.5f);

    public override Vector3 GetAABBExtends()
    {
        float extendX = 0f;
        float extendY = 0f;
        float extendZ = 0f;

        foreach (Vector3 corner in GetCubeCorner())
        {
            Vector3 local = corner - transform.position;
            if(Mathf.Abs(local.x) > extendX)
            {
                extendX = Mathf.Abs(local.x);
            }
            if (Mathf.Abs(local.y) > extendY)
            {
                extendY = Mathf.Abs(local.y);
            }
            if (Mathf.Abs(local.z) > extendZ)
            {
                extendZ = Mathf.Abs(local.z);
            }
        }

        return new Vector3(extendX, extendY, extendZ);
    }

    protected override Vector3 Support(Vector3 dir)
    {
        Vector3 result = new Vector3();
        float MaxProj = float.MinValue;

        foreach(Vector3 corner in GetCubeCorner())
        {
            float proj = Vector3.Dot(corner, dir.normalized);

            if (proj > MaxProj)
            {
                MaxProj = proj;
                result = corner;
            }
        }

        return result;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new UnityEngine.Color(193f / 255f, 63f / 255f, 240f / 255f);

        Matrix4x4 tempMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(Vector3.zero, extend * 2f);
        Gizmos.matrix = tempMatrix;

        if (bShowAABBBox)
        {
            Gizmos.color = UnityEngine.Color.red;
            Gizmos.DrawWireCube(transform.position, GetAABBExtends() * 2f);
        }
    }

    private List<Vector3> GetCubeCorner()
    {
        List<Vector3> corners = new List<Vector3>();

        for (int x = 0; x <= 1; x++)
        {
            for (int y = 0; y <= 1; y++)
            {
                for (int z = 0; z <= 1; z++)
                {
                    Vector3 localPoint = new Vector3(
                        x == 0 ? extend.x : -extend.x,
                        y == 0 ? extend.y : -extend.y,
                        z == 0 ? extend.z : -extend.z);

                    corners.Add(transform.TransformPoint(localPoint));
                }
            }
        }

        return corners;
    }
}
