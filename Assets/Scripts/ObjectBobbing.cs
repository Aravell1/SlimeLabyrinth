using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBobbing : MonoBehaviour
{
    float pos = 0;
    float offset = 0;

    // Start is called before the first frame update
    void Start()
    {
        pos = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        offset += Time.deltaTime * 2;
        float height = pos + Mathf.Sin(offset) / 2;
        transform.position = new(transform.position.x, height, transform.position.z);
    }
}
