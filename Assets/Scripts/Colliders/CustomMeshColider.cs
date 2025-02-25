using System.Collections.Generic;
using UnityEngine;


namespace CustomPhysic
{
    [RequireComponent(typeof(Mesh))]
    public class CustomMeshColider : CustomCollider
    {
        Vector3 SupportSelected;

        public override Vector3 GetAABBExtends()
        {
            return worldBounds.extents;
        }

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

            SupportSelected = result;
            return result;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(SupportSelected, 0.1f);

            if (bShowAABBBox)
            {
                Gizmos.color = UnityEngine.Color.red;
                Gizmos.DrawWireCube(transform.position, GetAABBExtends() * 2f);
            }
        }
    }

}
