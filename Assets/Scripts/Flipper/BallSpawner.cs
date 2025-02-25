using CustomPhysic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private float launchSpeed = 10;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            GameObject ball = Instantiate(ballPrefab);
            ball.GetComponent<CustomRigidbody>().AddForce(transform.up * launchSpeed, CustomRigidbody.ForceType.FT_Impulse);
        }
    }
}
