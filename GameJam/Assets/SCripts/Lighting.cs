using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighting : MonoBehaviour
{
    public Light lighting;
   
    Color color;
    Color startColor;
   
    public float maxalpha;
    public float maxRange;


    float maxColor = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        lighting.gameObject.GetComponent<SphereCollider>().radius = lighting.range;
        color = this.GetComponent<Renderer>().material.color;
        startColor = color;
    }

    // Update is called once per frame
    void Update()
    {
        lighting.gameObject.GetComponent<SphereCollider>().radius = lighting.range;
        lighting.range += Time.deltaTime;
       
    }

    private void OnTriggerEnter(Collider other)
    {
        color = startColor;
    }

    private void OnTriggerStay(Collider other)
    {
        color.r += 0.003f;
        if (color.r >= maxColor)
            color.r = maxColor;
        color.g += 0.003f;
        if (color.g >= maxColor)
            color.g = maxColor;

        color.b += 0.003f;
        if (color.b >= maxColor)
            color.b = maxColor;

        color.a = maxalpha;

        this.GetComponent<Renderer>().material.color = color;
        
       
    }

    void OnTriggerExit(Collider other)
    {
        this.GetComponent<Renderer>().material.color = startColor;

        Debug.Log("Uscito");

    }

    
}
