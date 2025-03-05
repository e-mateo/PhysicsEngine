using CustomPhysic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CustomBoxCollider))]
public class ScoringWone : MonoBehaviour
{
    [SerializeField] int value;
    [SerializeField] Scoring scoring;
    private void OnEnable()
    {
        GetComponent<CustomBoxCollider>().collideCallback += Collide;
    }

    private void OnDisable()
    {
        GetComponent<CustomBoxCollider>().collideCallback -= Collide;
    }

    private void Collide(CustomCollider other, CollisionInfo info)
    {
        scoring.score += value;
        Destroy(other.gameObject);
    }
}
