using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysic
{
    public class CustomPhysicEngine : MonoBehaviour
    {
        private List<CustomCollider> colliders = new List<CustomCollider>();
        [SerializeField] private DAABBTree dynamicAABBTree;
        [SerializeField] bool applyRotationResponse;

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
            if(applyRotationResponse)
            {
                CollisionResponseWithRotation(collisions);
            }
            else
            {
                CollisionResponseWithoutRotation(collisions);
            }
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
                    if (!collisionPair.colliderA.Moved && !collisionPair.colliderB.Moved)
                        continue;

                }

                CollisionInfo collisionInfo = CustomCollider.CheckCollision(collisionPair.colliderA, collisionPair.colliderB);
                if (collisionInfo != null)
                {
                    Debug.Log("Colision detected (" + collisionPair.colliderA.gameObject.name + ", " + collisionPair.colliderB.gameObject.name 
                                    + ") Penetration : " + collisionInfo.penetration
                                    + " Normal: " + collisionInfo.normal);
                                
                    collisions.Add(collisionInfo);
                }
            }

            return collisions;
        }

        private void CollisionResponseWithRotation(List<CollisionInfo> collisions)
        {
            foreach(CollisionInfo collision in collisions)
            {
                CustomPhysic.CustomRigidbody RB_A = collision.objectA.RB;
                CustomPhysic.CustomRigidbody RB_B = collision.objectB.RB;
                if(collision.objectA.bIsTrigger || collision.objectB.bIsTrigger)
                {
                    continue;
                }

                float invMass_A = RB_A != null ? 1f / RB_A.Mass : 0f;
                float invMass_B = RB_B != null ? 1f / RB_B.Mass : 0f;
                Vector3 rA = collision.contactA - collision.objectA.transform.position;
                Vector3 rB = collision.contactB - collision.objectB.transform.position;
                Vector3 vAi = RB_A != null ? RB_A.Velocity + Vector3.Cross(RB_A.AngVelocity, rA) : Vector3.zero;
                Vector3 vBi = RB_B != null ? RB_B.Velocity + Vector3.Cross(RB_B.AngVelocity, rB) : Vector3.zero;

                float relativeVelocity = Vector3.Dot(vAi - vBi, collision.normal);
                if(relativeVelocity > 0)
                {
                    continue;
                }

                //Rotation
                float weightRotA = 0f, weightRotB = 0f;
                Vector3 momentumA = Vector3.zero, momentumB = Vector3.zero;
                if (RB_A != null)
                {
                    momentumA = RB_A.GetInvWorldInertiaTensor().MultiplyPoint3x4(Vector3.Cross(rA, collision.normal));
                    weightRotA = Vector3.Dot(Vector3.Cross(momentumA, rA), collision.normal);
                }
                if(RB_B != null)
                {
                    momentumB = RB_B.GetInvWorldInertiaTensor().MultiplyPoint3x4(Vector3.Cross(rB, collision.normal));
                    weightRotB = Vector3.Dot(Vector3.Cross(momentumB, rB), collision.normal);
                }

                //Position Correction
                float damping = 0.25f;
                float correction = (collision.penetration * damping) / (invMass_A + invMass_B);
                if (RB_A != null)
                {
                    RB_A.transform.position += correction * invMass_A * collision.normal;
                }
                if (RB_B != null)
                {
                    RB_B.transform.position -= correction * invMass_B * collision.normal;
                }

                // Impulse + Torques
                if (RB_A != null)
                {
                    float JA = (-(1 + collision.objectA.PM.bouciness) * relativeVelocity) / (invMass_A + invMass_B + weightRotA + weightRotB);
                    RB_A.AddForce(JA * invMass_A * collision.normal, CustomRigidbody.ForceType.FT_VelocityChange);
                    RB_A.AddTorques(JA * momentumA, CustomRigidbody.ForceType.FT_VelocityChange);
                }
                if (RB_B != null)
                {
                    float JB = (-(1 + collision.objectB.PM.bouciness) * relativeVelocity) / (invMass_A + invMass_B + weightRotA + weightRotB);
                    RB_B.AddForce(-JB * invMass_B * collision.normal, CustomRigidbody.ForceType.FT_VelocityChange);
                    RB_B.AddTorques(-JB * momentumB, CustomRigidbody.ForceType.FT_VelocityChange);
                }
            }
        }


        private void CollisionResponseWithoutRotation(List<CollisionInfo> collisions)
        {
            foreach (CollisionInfo collision in collisions)
            {
                CustomPhysic.CustomRigidbody RB_A = collision.objectA.RB;
                CustomPhysic.CustomRigidbody RB_B = collision.objectB.RB;
                if (collision.objectA.bIsTrigger || collision.objectB.bIsTrigger)
                {
                    continue;
                }

                // Not sure if we should do an average ?
                float invMass_A = RB_A != null ? 1f / RB_A.Mass : 0f;
                float invMass_B = RB_B != null ? 1f / RB_B.Mass : 0f;


                Vector3 rB = collision.contactB - collision.objectB.transform.position;
                Vector3 vAi = RB_A != null ? RB_A.Velocity : Vector3.zero;
                Vector3 vBi = RB_B != null ? RB_B.Velocity : Vector3.zero;

                float relativeVelocity = Vector3.Dot(vAi - vBi, collision.normal);
                if (relativeVelocity > 0)
                {
                    continue;
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

                // Impulse
                if (RB_A != null)
                {
                    float JA = (-(1 + collision.objectA.PM.bouciness) * relativeVelocity) / (invMass_A + invMass_B);

                    RB_A.Velocity += JA * invMass_A * collision.normal;
                }
                if (RB_B != null)
                {
                    float JB = (-(1 + collision.objectB.PM.bouciness) * relativeVelocity) / (invMass_A + invMass_B);
                    RB_B.Velocity -= JB * invMass_B * collision.normal;
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

            foreach (CollisionInfo collision in collisions)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(collision.contactA, 0.1f);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(collision.contactB, 0.1f);
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
