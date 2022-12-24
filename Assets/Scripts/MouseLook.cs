using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    //MouseLook rotates the transform based on the mouse delta
    //Min and max values can be used to constrain possible rotation

    public enum RotationAxis { MouseX, MouseY }
    public RotationAxis axes = RotationAxis.MouseX;

    public float senX = 30f;
    public float senY = 15f;

    public float minX = -360f;
    public float maxX = 360f;

    public float minY = -60f;
    public float maxY = 60f;

    float rotX = 0f;
    float rotY = 0f;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (axes == RotationAxis.MouseX)
        {
            rotX += Input.GetAxis("Mouse X") * senX;

            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, rotX, transform.localEulerAngles.z);
        }
        else
        {
            rotY += Input.GetAxis("Mouse Y") * senY;
            rotY = Mathf.Clamp(rotY, minY, maxY);

            transform.localEulerAngles = new Vector3(-rotY, transform.localEulerAngles.y, 0);
        }
    }

}
