using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CustomPhysicEngine : MonoBehaviour
{
    List<CustomCollider> colliders;

    // Start is called before the first frame update
    void Start()
    {
        colliders = new List<CustomCollider>(FindObjectsByType<CustomCollider>(FindObjectsSortMode.None));
    }

    // Update is called once per frame
    void Update()
    {
        foreach (CustomCollider testingCollider in colliders) 
        {
            foreach (CustomCollider otherCollider in colliders)
            {
                if (otherCollider == testingCollider)
                    continue;

                if (CustomCollider.CheckCollision(testingCollider, otherCollider))
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


        Gizmos.color = Color.blue;
        List<Vector3> diff = CustomCollider.MinkowskiDifference(colliders[0], colliders[1]);

        foreach (Vector3 v in diff)
        {
            Gizmos.DrawSphere(v, 0.05f);
        }

    }
}
