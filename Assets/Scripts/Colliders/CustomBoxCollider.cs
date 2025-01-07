using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomPhysic;
public class CustomBoxCollider : CustomCollider
{
    [SerializeField] Vector3 origin = Vector3.zero;
    [SerializeField] Vector3 extend = Vector3.one;

    protected override Vector3 Support(Vector3 dir)
    {
        //return transform.position + (radius * dir.normalized);
        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(193f / 255f, 63f / 255f, 240f / 255f);
        Gizmos.DrawWireCube(transform.position + origin, extend);
    }
}
