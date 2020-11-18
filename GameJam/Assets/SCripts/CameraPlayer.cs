using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraPlayer : MonoBehaviour
{
    public Transform target;
    Vector3 distance, offset;
    float camera_z;
    private void Start()
    {
        offset = new Vector3(-50f, 50f, -50f);

    }
    void Update()
    {
        distance = target.position - transform.position;
        if (distance.x > Math.Abs(2) )
        {
            transform.position = Vector3.Lerp(transform.position, target.position + offset, 0.5f * Time.deltaTime);

        }
        if (distance.z > Math.Abs(2))
        {
            transform.position = Vector3.Lerp(transform.position, target.position + offset, 0.5f * Time.deltaTime);

        }
    }
}
