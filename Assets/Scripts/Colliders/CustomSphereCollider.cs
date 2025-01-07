using UnityEngine;

public class CustomSphereCollider : CustomCollider
{
    [SerializeField] float radius = 1.0f;

    protected override Vector3 Support(Vector3 dir)
    {
        return transform.position + (radius * dir.normalized);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(193f / 255f, 63f / 255f, 240f / 255f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
