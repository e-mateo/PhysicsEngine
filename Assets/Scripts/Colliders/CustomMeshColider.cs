using UnityEngine;

[RequireComponent (typeof(Mesh))]
public class CustomMeshColider : CustomCollider
{
    Mesh mesh;

    private void Awake()
    {
        mesh = GetComponent<Mesh>();
    }

    protected override Vector3 Support(Vector3 dir) 
    {
        Vector3 result = new Vector3();
        float MaxProj = float.MinValue;

        foreach (Vector3 point in mesh.vertices) 
        {
            float proj = Vector3.Dot(point, dir);

            if (proj > MaxProj) 
            {
                MaxProj = proj;
                result = point;
            }
        }

        return result;
    }
}
