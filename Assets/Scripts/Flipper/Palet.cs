using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PaletType
{
    PT_Left,
    PT_Right
}

public class Palet : MonoBehaviour
{
    [SerializeField] PaletType type;
    [SerializeField] float angleMax;
    [SerializeField] float degPSecond = 20;
    private float currentAppliedAngle = 0;

    // Update is called once per frame
    void Update()
    {
        // L Palet logic
        if (type == PaletType.PT_Left)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                if (currentAppliedAngle < angleMax)
                {
                    transform.Rotate(Vector3.forward, degPSecond * Time.deltaTime);
                    currentAppliedAngle += degPSecond * Time.deltaTime;
                }
            }
            else
            {
                if (currentAppliedAngle > 0)
                {
                    transform.Rotate(Vector3.forward, -degPSecond * Time.deltaTime);
                    currentAppliedAngle -= degPSecond * Time.deltaTime;
                }
            }
        }

        // R Palet logic
        if (type == PaletType.PT_Right)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                if (currentAppliedAngle < angleMax)
                {
                    transform.Rotate(Vector3.forward, -degPSecond * Time.deltaTime);
                    currentAppliedAngle += degPSecond * Time.deltaTime;
                }
            }
            else
            {
                if (currentAppliedAngle > 0)
                {
                    transform.Rotate(Vector3.forward, degPSecond * Time.deltaTime);
                    currentAppliedAngle -= degPSecond * Time.deltaTime;
                }
            }
        }
    }
}
