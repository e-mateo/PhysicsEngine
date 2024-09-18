using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class RigidBody : MonoBehaviour
{
    public enum ForceType
    {
        FT_Acceleration, 
        FT_Impulse
    }

    [SerializeField] bool UseGravity;

    private Vector3 velocity;

    List<Vector3> Accelerations;
    List<Vector3> Impulse;

    public void AddForce(Vector3 force, ForceType type)
    {
        switch (type) 
        { 
            case ForceType.FT_Acceleration:
                Accelerations.Add(force); break;

            case ForceType.FT_Impulse:
                Impulse.Add(force); break;

            default:
                break;
        }
    }

    private void InitVariable()
    {
        velocity = new Vector3(0, 0, 0);
        Accelerations = new List<Vector3>();
        Impulse = new List<Vector3>();
    }

    private void CalcVelocity()
    {
        if (UseGravity)
            velocity += GlobalParameters.instance.Gravity * Time.fixedDeltaTime;

        foreach (Vector3 vec in Accelerations)
            velocity += vec * Time.fixedDeltaTime;

        foreach (Vector3 vec in Impulse)
            velocity += vec;

        Accelerations.Clear();
        Impulse.Clear();
    }

    #region Monobehaviour
    private void Start()
    {
        InitVariable();
    }

    private void FixedUpdate()
    {
        CalcVelocity();

        transform.position = transform.position + velocity * Time.fixedDeltaTime;
    }
    #endregion
}
