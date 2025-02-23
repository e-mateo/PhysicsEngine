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
            FT_Acceleration,
            FT_Impulse
        }

        [SerializeField] float mass;
        [SerializeField] bool useGravity;
        [SerializeField] bool isKinematic;
        
        [SerializeField] RigidbodyDebug debug;

        private Vector3 acceleration;
        private Vector3 impulse;
        private Vector3 velocity;

        List<Vector3> Accelerations = new List<Vector3>();
        List<Vector3> Impulses = new List<Vector3>();

        public Vector3 Velocity {  get { return velocity; } set {  velocity = value; } }
        public float Mass { get { return mass; } set { mass = value; } }

        public void AddForce(Vector3 force, ForceType type)
        {
            switch (type)
            {
                case ForceType.FT_Acceleration:
                    Accelerations.Add(force); break;

                case ForceType.FT_Impulse:
                    Impulses.Add(force); break;

                default:
                    break;
            }
        }

        private void InitVariable()
        {
            velocity = new Vector3(0, 0, 0);
        }

        private void CalcVelocity()
        {
            acceleration.Set(0, 0, 0);
            impulse.Set(0, 0, 0);

            if (useGravity)
                AddForce(GlobalParameters.instance.Gravity, ForceType.FT_Acceleration);

            foreach (Vector3 vec in Accelerations)
            {
                acceleration += vec;
                velocity += vec * Time.fixedDeltaTime;
            }

            foreach (Vector3 vec in Impulses)
            {
                impulse += vec;
                velocity += vec;
            }

            Accelerations.Clear();
            Impulses.Clear();
        }

        #region Monobehaviour
        private void Start()
        {
            InitVariable();
        }

        private void FixedUpdate()
        {
            if(isKinematic) return;

            CalcVelocity();

            transform.position = transform.position + velocity * Time.fixedDeltaTime;
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
                Gizmos.DrawLine(transform.position, debug.normalizeImp ? transform.position + impulse.normalized * debug.impLenght : transform.position + impulse * debug.impLenght);
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
