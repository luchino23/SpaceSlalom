using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colorChange : MonoBehaviour
{
    Renderer rend;
    Color color = Color.red;
    public float maxalpha;

    public float time_tochange;
    // Start is called before the first frame update
    void Start()
    {
        color.r = 1.0f;
        color.g = 0.0f;
        color.b = 0.0f;
        color.a = maxalpha;

        rend = GetComponent<Renderer>();

        rend.material.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        time_tochange -= Time.deltaTime;

        if (time_tochange <= 0)
        {
            color.r += 0.01f;
            color.g += 0.01f;
            color.b += 0.01f;
            color.a = maxalpha;

            rend.material.color = color;

        }
    }
}
