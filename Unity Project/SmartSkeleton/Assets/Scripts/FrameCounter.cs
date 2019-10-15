using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameCounter : MonoBehaviour {

public float avgFrameRate;

public void Update()
{
    avgFrameRate = Time.frameCount / Time.time;
}
}


