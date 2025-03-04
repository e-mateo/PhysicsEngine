using CustomPhysic;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShowroomSituation : MonoBehaviour
{
    // Start is called before the first frame update
    List<CustomRigidbody> rigidbodies = new List<CustomRigidbody>();
    public bool HasStarted { get; private set; }

    private void Awake()
    {
        rigidbodies = GetComponentsInChildren<CustomRigidbody>().ToList();
    }

    public void StartSimulation()
    {
        foreach (CustomRigidbody body in rigidbodies)
        {
            body.IsKinematic = false;
        }

        HasStarted = true;
    }
}
