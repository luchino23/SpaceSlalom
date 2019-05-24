using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LagSimulator : MonoBehaviour
{
    // speed value for simulating server movements
    public float speed;
    // number of infos per second
    public float lag;

    // current server position
    public float xPosition;
    // previous server position
    public float xPreviousPosition;

    // time value for simulating lag
    public float timer;

    // gradient for interpolation/extrapolation
    public float lagGradient;

    // prediction of the client lag (distance covered in time)
    public float prediction;

    // Start is called before the first frame update
    void Start()
    {
        // start with a valid timer, so we simulate initial lag
        timer = 1.0f / lag;
    }

    void GetNextUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            // reset gradient as we have a new value from the server
            lagGradient = 0;

            // store previous server position
            xPreviousPosition = xPosition;
            // compute new position (will be the new server position in a true game)
            xPosition += speed / lag;
            // predict distance covered in the next step
            prediction = xPosition - xPreviousPosition;
            // next lag simulation
            timer = (1.0f / lag) + timer;
        }
    }


    // Update is called once per frame
    void Update()
    {
        GetNextUpdate();
        // previous server position + prediction
        Vector3 prevPosition = transform.position;
        prevPosition.x = xPreviousPosition + prediction;

        // latest server position + prediction
        Vector3 nextPosition = transform.position;
        nextPosition.x = xPosition + prediction;

        transform.position = Vector3.Lerp(prevPosition, nextPosition, lagGradient);
        // recompute gradient (fix the lag multiplication here, taking into account the client ping)
        lagGradient += Time.deltaTime * lag;
    }
}

