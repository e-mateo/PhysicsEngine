using UnityEngine;
using CustomPhysic;

public class CustomSphereCollider : CustomCollider
{
    [SerializeField] float radius = 1.0f;

    protected override void SetInvInteriaTensor()
    {
        if (customRigidbody != null)
        {
            float M = customRigidbody.Mass;
            float R = radius;

            InvInertiaTensor = new Matrix4x4(
            new Vector4((2f / 5f) * M * R * R, 0f, 0f, 0f),
            new Vector4(0f, (2f / 5f) * M * R * R, 0f, 0f),
            new Vector4(0f, 0f, (2f / 5f) * M * R * R, 0f),
            new Vector4(0f, 0f, 0f, 1.0f)
            );

            InvInertiaTensor = InvInertiaTensor.inverse;
        }
        else
        {
            base.SetInvInteriaTensor();
        }
    }

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
