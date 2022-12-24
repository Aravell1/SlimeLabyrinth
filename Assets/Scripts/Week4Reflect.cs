using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Week4Reflect : MonoBehaviour
{
    public float maxLength;
    public int maxReflections = 5;

    private LineRenderer lr;
    private Ray ray;
    private RaycastHit hit;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        ray = new Ray(transform.position, transform.forward);

        lr.positionCount = 1;
        lr.SetPosition(0, transform.position);

        for (int i = 0; i < maxReflections; i++)
        {
            if (Physics.Raycast(ray, out hit, maxLength))
            {
                lr.positionCount++;
                lr.SetPosition(lr.positionCount - 1, hit.point);
                ray = new Ray(hit.point, CustomReflect(ray.direction, hit.normal));
            }
            else
            {
                lr.positionCount++;
                lr.SetPosition(lr.positionCount - 1, ray.origin + ray.direction * maxLength);
            }
        }
    }

    public Vector3 CustomReflect(Vector3 inDirection, Vector3 inNormal)
    {
        Vector3 resultant = -2 * Vector3.Dot(inNormal, inDirection) * inNormal + inDirection;
        return resultant;
    }
}
