using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ScoringWone : MonoBehaviour
{
    [SerializeField] int value;
    [SerializeField] Scoring scoring;

    private void OnTriggerEnter(Collider other)
    {
        scoring.score += value;
        Debug.Log("Score " +  value);
    }
}
