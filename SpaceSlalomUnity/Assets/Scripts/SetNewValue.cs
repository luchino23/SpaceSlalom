using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNewValue : MonoBehaviour
{
    public AudioObj audioobj;
    private AudioSource audioSrc;
    // Start is called before the first frame update
    void Start()
    {
        audioSrc = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        audioSrc.volume = audioobj.volValue;
    }
}
