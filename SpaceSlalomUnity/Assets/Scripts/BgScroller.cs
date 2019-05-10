using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgScroller : MonoBehaviour
{
    public float speed = 0.1f;
    float x;
    Vector2 offSet;
    Renderer mat;
    
    void Awake()
    {
        mat = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        x = Mathf.Repeat(Time.time * speed, 1);
        offSet = new Vector2(x, 0f);
        mat.sharedMaterial.SetTextureOffset("_MainTex", offSet);
    }
}
