using UnityEngine;

[RequireComponent (typeof(CustomRigidbody))]
public class RigidbodyTest : MonoBehaviour
{
    private CustomRigidbody body;

    void Start()
    {
        body = GetComponent<CustomRigidbody>();
        
        body.AddForce(new Vector3 (3, 5, 0), CustomRigidbody.ForceType.FT_Impulse);
    }
}
