using System.Collections.Generic;
using UnityEngine;

public class CustomPhysicEngine : MonoBehaviour
{
    private List<CustomCollider> colliders = new List<CustomCollider>();
    [SerializeField] private DAABBTree dynamicAABBTree;

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
    //void Start()
    //{
    //    colliders = new List<CustomCollider>(FindObjectsByType<CustomCollider>(FindObjectsSortMode.None));
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    foreach (CustomCollider testingCollider in colliders) 
    //    {
    //        foreach (CustomCollider otherCollider in colliders)
    //        {
    //            if (otherCollider == testingCollider)
    //                continue;

    //            if (CustomCollider.CheckCollision(testingCollider, otherCollider))
    //                Debug.Log("Collisioooooooon !!!");
    //        }
    //    }
    //}

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
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(Vector3.zero, 0.2f);

        //if (!Application.isPlaying)
        //    return;


        //Gizmos.color = Color.blue;
        //List<Vector3> diff = CustomCollider.MinkowskiDifference(colliders[0], colliders[1]);

        //foreach (Vector3 v in diff)
        //{
        //    Gizmos.DrawSphere(v, 0.05f);
        //}
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
