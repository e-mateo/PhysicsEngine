using System.Collections.Generic;
using UnityEngine;

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
        colliders = new List<CustomCollider>(FindObjectsByType<CustomCollider>(FindObjectsSortMode.None));
        collidingTethraedron = new Vector3[4];
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
                {
                    Debug.Log("Collisioooooooon !!!");

                }
            }
        }
    }

    /*
    private void FixedUpdate()
    {
        dynamicAABBTree.UpdateTreeAndCollisionPairs();
        List<CollisionPair> collisionPairs = dynamicAABBTree.GetCollisionPairs();
        Debug.Log(collisionPairs.Count);
        foreach (CollisionPair collisionPair in collisionPairs)
        {
            if (CustomCollider.CheckCollision(collisionPair.colliderA, collisionPair.colliderB))
            {
                Debug.Log("Collisioooooooon !!!");
            }
        }
    }*/

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(Vector3.zero, 0.2f);

        if (!Application.isPlaying)
            return;

        Vector3[] tet = CustomCollider.GenerateTetrahedron(colliders[0], colliders[1]);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(tet[0], tet[1]);
        Gizmos.DrawLine(tet[0], tet[2]);
        Gizmos.DrawLine(tet[0], tet[3]);
        Gizmos.DrawLine(tet[1], tet[2]);
        Gizmos.DrawLine(tet[1], tet[3]);
        Gizmos.DrawLine(tet[2], tet[3]);

        tet = CustomCollider.GenerateTetrahedron(colliders[1], colliders[0]);
        Gizmos.DrawLine(tet[0], tet[1]);
        Gizmos.DrawLine(tet[0], tet[2]);
        Gizmos.DrawLine(tet[0], tet[3]);
        Gizmos.DrawLine(tet[1], tet[2]);
        Gizmos.DrawLine(tet[1], tet[3]);
        Gizmos.DrawLine(tet[2], tet[3]);

        Gizmos.color = Color.green;
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
