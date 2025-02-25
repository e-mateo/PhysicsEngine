using UnityEngine;
using CustomPhysic;
using System.Drawing;
using System.Collections.Generic;
using static UnityEngine.UI.GridLayoutGroup;
using static UnityEngine.UI.Image;

public class CustomBoxCollider : CustomCollider
{
    [SerializeField] Vector3 extend = Vector3.one;


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
        Gizmos.DrawWireCube(Vector3.zero, extend);
        Gizmos.matrix = tempMatrix;
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
                    Vector3 localPoint = 0.5f * new Vector3(
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
