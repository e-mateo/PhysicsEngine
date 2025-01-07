using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysic
{
    public class CustomPhysicEngine : MonoBehaviour
    {
        private List<CustomCollider> colliders = new List<CustomCollider>();
        [SerializeField] private DAABBTree dynamicAABBTree;

        public static Vector3[] collidingTethraedron;

        // Singleton access
        static CustomPhysicEngine instance = null;
        static public CustomPhysicEngine Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<CustomPhysicEngine>();
                }
                return instance;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            collidingTethraedron = new Vector3[4];
        }

        private void FixedUpdate()
        {
            dynamicAABBTree.UpdateTreeAndCollisionPairs();
            List<CollisionPair> collisionPairs = dynamicAABBTree.GetCollisionPairs();
            Debug.Log(collisionPairs.Count);
            foreach (CollisionPair collisionPair in collisionPairs)
            {
                CustomPhysic.CollisionInfo collisionInfo = CustomCollider.CheckCollision(collisionPair.colliderA, collisionPair.colliderB);
    
                if (collisionInfo != null)
                {
                    Debug.Log("Collisioooooooon !!!");

                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(Vector3.zero, 0.2f);

            if (!Application.isPlaying)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(collidingTethraedron[0], collidingTethraedron[1]);
            Gizmos.DrawLine(collidingTethraedron[0], collidingTethraedron[2]);
            Gizmos.DrawLine(collidingTethraedron[0], collidingTethraedron[3]);
            Gizmos.DrawLine(collidingTethraedron[1], collidingTethraedron[2]);
            Gizmos.DrawLine(collidingTethraedron[1], collidingTethraedron[3]);
            Gizmos.DrawLine(collidingTethraedron[2], collidingTethraedron[3]);
        }

        public void OnColliderEnable(CustomCollider collider)
        {
            if (!colliders.Contains(collider))
            {
                colliders.Add(collider);
                dynamicAABBTree.AddColliderToTree(collider);
            }
        }

        public void OnColliderDisbale(CustomCollider collider)
        {
            if (colliders.Contains(collider))
            {
                colliders.Remove(collider);
                dynamicAABBTree.RemoveColliderFromTree(collider);
            }
        }
    }
}
