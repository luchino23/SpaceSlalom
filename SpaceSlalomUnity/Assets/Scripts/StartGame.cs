using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    public Canvas startGame;
    float start = 0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        start = start + Time.deltaTime;
        if (start > 2.3f)
        {
            //startGame.GetComponent<Canvas>().enabled = false;
            startGame.enabled = false;
        }
    }
}
