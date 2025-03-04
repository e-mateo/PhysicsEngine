using CustomPhysic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private float minLaunchSpeed = 5;
    [SerializeField] private float maxLaunchSpeed = 10;
    [SerializeField] private float speedIncrease = 2.5f;

    private float effectiveLaunchSpeed = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            effectiveLaunchSpeed += speedIncrease * Time.deltaTime;
            if (effectiveLaunchSpeed > maxLaunchSpeed) 
                effectiveLaunchSpeed = maxLaunchSpeed;

            if (effectiveLaunchSpeed < minLaunchSpeed)
                effectiveLaunchSpeed = minLaunchSpeed;
        }

        if (Input.GetKeyUp(KeyCode.UpArrow)) 
        { 
            GameObject ball = Instantiate(ballPrefab,transform.position, transform.rotation);
            ball.GetComponent<CustomRigidbody>().AddForce(transform.up * effectiveLaunchSpeed, CustomRigidbody.ForceType.FT_Impulse);
            effectiveLaunchSpeed = 0;
        }

        Debug.Log(effectiveLaunchSpeed);
    }
}
