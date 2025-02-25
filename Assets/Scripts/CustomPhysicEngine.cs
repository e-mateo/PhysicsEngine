using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysic
{
    public class CustomPhysicEngine : MonoBehaviour
    {
        private List<CustomCollider> colliders = new List<CustomCollider>();
        [SerializeField] private DAABBTree dynamicAABBTree;
        List<CollisionInfo> collisions = new List<CollisionInfo>();

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
            collisions = CollisionDetection();
            CollisionResponse(collisions);
        }

        private List<CollisionInfo> CollisionDetection()
        {
            List<CollisionInfo> collisions = new List<CollisionInfo>();
            dynamicAABBTree.UpdateTreeAndCollisionPairs();
            List<CollisionPair> collisionPairs = dynamicAABBTree.GetCollisionPairs();

            foreach (CollisionPair collisionPair in collisionPairs)
            {
                if(collisionPair.colliderA.RB == null && collisionPair.colliderB.RB == null)
                {
                    if (!collisionPair.colliderA.Moved && !collisionPair.colliderA.Moved)
                        continue;

                }

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
            foreach(CollisionInfo collision in collisions)
            {
                CustomPhysic.CustomRigidbody RB_A = collision.objectA.RB;
                CustomPhysic.CustomRigidbody RB_B = collision.objectB.RB;
                if(RB_A == null && RB_B == null)
                {
                    return;
                }
                Vector3 vel_A = RB_A != null ? RB_A.Velocity : Vector3.zero;
                Vector3 vel_B = RB_B != null ? RB_B.Velocity : Vector3.zero;

                float relativeVelocity = Vector3.Dot(vel_A - vel_B, collision.normal);
                if(relativeVelocity > 0)
                {
                    continue;
                }

                // Not sure if we should do an average ?
                float restitution = (collision.objectA.PM.bouciness + collision.objectB.PM.bouciness) * 0.5f;

                float invMass_A = RB_A != null ? 1f / RB_A.Mass : 0f;
                float invMass_B = RB_B != null ? 1f / RB_B.Mass : 0f;

                // Impulse
                float J = (-(1 + restitution) * relativeVelocity) / (invMass_A + invMass_B);
                if(RB_A != null)
                {
                    RB_A.Velocity += J * invMass_A * collision.normal;
                }
                if (RB_B != null)
                {
                    RB_B.Velocity -= J * invMass_B * collision.normal;
                }


                //Position Correction
                float damping = 0.2f;
                float correction = (collision.penetration * damping) / (invMass_A + invMass_B);
                if (RB_A != null)
                {
                    RB_A.transform.position += correction * invMass_A * collision.normal;
                }
                if (RB_B != null)
                {
                    RB_B.transform.position -= correction * invMass_B * collision.normal;
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

            Gizmos.color = Color.green;
            foreach (CollisionInfo collision in collisions)
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
