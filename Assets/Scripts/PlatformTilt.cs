using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformTilt : MonoBehaviour
{
    GameObject axle;
    public float rotationRate = 7.5f;
    float lerpTime = 0;
    Quaternion targetRotation;
    Quaternion fromAngle;
    bool resetRotation = false;
    bool resetTime = false;
    float resetTimer = 2f;

    void Start()
    {
        axle = transform.parent.gameObject;
        targetRotation = axle.transform.rotation;
    }

    void Update()
    {
        if (resetTime)
        {
            resetTimer += Time.deltaTime;
            if (resetTimer > 2)
            {
                resetTime = false;
                resetTimer = 0;
                resetRotation = true;
            }
        }
        if (resetRotation)
        {
            lerpTime += Time.deltaTime;
            axle.transform.rotation = Quaternion.Lerp(fromAngle, targetRotation, lerpTime);
            if (axle.transform.rotation == targetRotation)
            {
                resetRotation = false;
                lerpTime = 0;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            resetTime = false;
            resetTimer = 0;
            resetRotation = false;
            if ((other.transform.position.x > transform.position.x && axle.transform.rotation.y == 0) 
                || (other.transform.position.z < transform.position.z && axle.transform.rotation.y != 0))
            {
                if (axle.transform.localEulerAngles.z > -60 && axle.transform.localEulerAngles.z < 60)
                {
                    axle.transform.Rotate(0, 0, -1 * rotationRate * Vector3.Distance(other.transform.position, transform.position) * Time.deltaTime);
                }
            }
            else if ((other.transform.position.x < transform.position.x && axle.transform.rotation.y == 0)
                || (other.transform.position.z > transform.position.z && axle.transform.rotation.y != 0))
            {
                if (axle.transform.localEulerAngles.z > -60 && axle.transform.localEulerAngles.z < 60)
                {
                    axle.transform.Rotate(0, 0, rotationRate * Vector3.Distance(other.transform.position, transform.position) * Time.deltaTime);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            resetTimer = 0;
            resetTime = true;
            lerpTime = 0;
            fromAngle = axle.transform.rotation;
        }
    }

}
