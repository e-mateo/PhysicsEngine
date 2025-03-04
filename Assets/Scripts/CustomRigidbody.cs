using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace CustomPhysic
{
    [Serializable]
    public class RigidbodyDebug
    {
        [SerializeField] public bool debugAcceleration;
        [SerializeField] public bool normalizeAcc;
        [SerializeField] public float accLenght = 1;
        [SerializeField] public Color accColor = Color.yellow;

        [SerializeField] public bool debugImpulse;
        [SerializeField] public bool normalizeImp;
        [SerializeField] public float impLenght = 1;
        [SerializeField] public Color impColor = Color.cyan;

        [SerializeField] public bool debugVelocity;
        [SerializeField] public bool normalizeVel;
        [SerializeField] public float velLenght = 1;
        [SerializeField] public Color velColor = Color.green;
    }

    public class CustomRigidbody : MonoBehaviour
    {
        public enum ForceType
        {
            FT_Force,
            FT_Acceleration,
            FT_Impulse,
            FT_VelocityChange,
        }

        [SerializeField] float mass = 1.0f;
        [SerializeField] bool useGravity = true;
        [SerializeField] bool isKinematic;
        [SerializeField] float linearDrag = 0f;
        [SerializeField] float angularDrag = 0.05f;

        [SerializeField] RigidbodyDebug debug;

        private Vector3 acceleration;
        private Vector3 angAcceleration;
        private Vector3 velocity;
        private Vector3 angVelocity;

        private Vector3 Forces;
        private Vector3 Impulses;

        private Vector3 Torques;
        private Vector3 TorquesImpulses;

        Matrix4x4 invLocalTensor;

        public Vector3 Velocity {  get { return velocity; } set {  velocity = value; } }
        public Vector3 AngVelocity { get { return angVelocity; } set { angVelocity = value; } }
        public bool IsKinematic { get { return isKinematic; } set { isKinematic = value; } }

        public float Mass { get { return mass; } set { mass = value; } }

        public void AddForce(Vector3 force, ForceType type)
        {
            switch (type)
            {
                case ForceType.FT_Force:
                    Forces += (force / mass); break;

                case ForceType.FT_Acceleration:
                    Forces += (force); break;

                case ForceType.FT_Impulse:
                    Impulses += (force / mass); break;

                case ForceType.FT_VelocityChange:
                    Impulses += (force); break;

                default:
                    break;
            }
        }

        //Torques in radians
        public void AddTorques(Vector3 torque, ForceType type)
        {
            switch (type)
            {
                case ForceType.FT_Force:
                    Torques += (torque / mass); break;

                case ForceType.FT_Acceleration:
                    Torques += (torque); break;

                case ForceType.FT_Impulse:
                    TorquesImpulses += (torque / mass); break;

                case ForceType.FT_VelocityChange:
                    TorquesImpulses += (torque); break;

                default:
                    break;
            }
        }

        #region Monobehaviour
        private void Start()
        {
            InitVariable();
        }

        private void InitVariable()
        {
            velocity = new Vector3(0, 0, 0);
            angVelocity = new Vector3(0, 0, 0);
            acceleration = new Vector3(0, 0, 0);
        }

        private void FixedUpdate()
        {
            if(isKinematic) return;

            if (useGravity)
                AddForce(GlobalParameters.instance.Gravity, ForceType.FT_Acceleration);

            Integrate();
        }

        private void Integrate()
        {
            acceleration = Forces;
            angAcceleration = Torques;

            velocity = velocity + (acceleration * Time.fixedDeltaTime) + Impulses;
            angVelocity = angVelocity + (angAcceleration * Time.fixedDeltaTime) + TorquesImpulses;

            //Euler Semi implicite
            transform.position = transform.position + velocity * Time.fixedDeltaTime;
            transform.rotation = transform.rotation * Quaternion.Euler(angVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime);

            //Drag
            velocity *= 1.0f / (1.0f + (Time.fixedDeltaTime * linearDrag));
            angVelocity *= 1.0f / (1.0f + (Time.fixedDeltaTime * angularDrag));

            Forces = Vector3.zero; Impulses = Vector3.zero;
            Torques = Vector3.zero; TorquesImpulses = Vector3.zero;
        }

        public void SetLocalInertiaTensor(Matrix4x4 localTensor)
        {
            invLocalTensor = localTensor;
        }

        public Matrix4x4 GetInvWorldInertiaTensor()
        {
            Matrix4x4 rotMatrix = Matrix4x4.Rotate(transform.rotation);
            return rotMatrix * invLocalTensor * rotMatrix.inverse;
        }

        private void OnDrawGizmos()
        {
            if (debug.debugAcceleration)
            {
                Gizmos.color = debug.accColor;
                Gizmos.DrawLine(transform.position, debug.normalizeAcc ? transform.position + acceleration.normalized * debug.accLenght : transform.position + acceleration * debug.accLenght);
            }

            if (debug.debugImpulse)
            {
                Gizmos.color = debug.impColor;
                Gizmos.DrawLine(transform.position, debug.normalizeImp ? transform.position + Impulses.normalized * debug.impLenght : transform.position + Impulses * debug.impLenght);
            }

            if (debug.debugVelocity)
            {
                Gizmos.color = debug.velColor;
                Gizmos.DrawLine(transform.position, debug.normalizeVel ? transform.position + velocity.normalized * debug.velLenght : transform.position + velocity * debug.velLenght);
            }
        }
        #endregion
    }
}
