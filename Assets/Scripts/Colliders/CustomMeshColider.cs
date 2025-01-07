using UnityEngine;

[RequireComponent (typeof(Mesh))]
public class CustomMeshColider : CustomCollider
{
    protected override Vector3 Support(Vector3 dir) 
    {
        Vector3 result = new Vector3();
        float MaxProj = float.MinValue;

        foreach (Vector3 point in mesh.vertices) 
        {
            float proj = Vector3.Dot(transform.TransformPoint(point), dir.normalized);

            if (proj > MaxProj) 
            {
                MaxProj = proj;
                result = transform.TransformPoint(point);
            }
        }

        return (result + bounds.center + transform.position);
    }


    public static Vector3 StaticSupport(Vector3[] vertices, Vector3 dir)
    {
        Vector3 result = new Vector3();
        float MaxProj = float.MinValue;

        foreach (Vector3 point in vertices)
        {
            float proj = Vector3.Dot(point, dir.normalized);

            if (proj > MaxProj)
            {
                MaxProj = proj;
                result = point;
            }
        }

        return result;
    }
}
