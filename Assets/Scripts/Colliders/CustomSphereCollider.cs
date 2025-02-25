using UnityEngine;
using CustomPhysic;

public class CustomSphereCollider : CustomCollider
{
    [SerializeField] float radius = 1.0f;

    public override Vector3 GetAABBExtends()
    {
        return new Vector3(radius, radius, radius);
    }
    protected override Vector3 Support(Vector3 dir)
    {
        return transform.position + (radius * dir.normalized);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Matrix4x4 tempMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireSphere(Vector3.zero, radius);
        Gizmos.matrix = tempMatrix;

        if (bShowAABBBox)
        {
            Gizmos.color = UnityEngine.Color.red;
            Gizmos.DrawWireCube(transform.position, GetAABBExtends() * 2f);
        }
    }
}
