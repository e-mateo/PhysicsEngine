using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysic
{
    public class CustomPhysicEngine : MonoBehaviour
    {
        private List<CustomCollider> colliders = new List<CustomCollider>();
        [SerializeField] private DAABBTree dynamicAABBTree;
        List<CollisionInfo> collisionsa;

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
            collisionsa = CollisionDetection();
            CollisionResponse(collisionsa);
        }

        private List<CollisionInfo> CollisionDetection()
        {
            List<CollisionInfo> collisions = new List<CollisionInfo>();
            dynamicAABBTree.UpdateTreeAndCollisionPairs();
            List<CollisionPair> collisionPairs = dynamicAABBTree.GetCollisionPairs();

            foreach (CollisionPair collisionPair in collisionPairs)
            {
                CollisionInfo collisionInfo = CustomCollider.CheckCollision(collisionPair.colliderA, collisionPair.colliderB);
                if (collisionInfo != null)
                {
                    Debug.Log("Colision detected (" + collisionPair.colliderA.gameObject.name + ", " + collisionPair.colliderB.gameObject.name + ") \n Penetration : " + collisionInfo.penetration);
                    collisions.Add(collisionInfo);
                }
            }

            return collisions;
        }

        private void CollisionResponse(List<CollisionInfo> collisions)
        {

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

            Gizmos.color = Color.green;
            foreach (CollisionInfo collision in collisionsa)
            {
                Gizmos.DrawSphere(collision.contact, 0.1f);
            }
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
