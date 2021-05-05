using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class quit : MonoBehaviour
{

    public void wantToQuit()
    {
        Debug.Log("quitting game");
        Application.Quit();
    }
}
