using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Energia : MonoBehaviour
{
    float vita;
    public float MaxEnergy;
    public float timer;
    public Scrollbar loadingbar;

    // Start is called before the first frame update
    void Start()
    {
        loadingbar.size = vita;
        

        vita = MaxEnergy;
    }
    // Update is called once per frame
    void Update()
    {
        vita -= Time.deltaTime;
      
        loadingbar.size = vita / timer;

    }
}
