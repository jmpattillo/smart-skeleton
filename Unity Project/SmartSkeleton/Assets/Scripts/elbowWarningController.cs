using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class elbowWarningController : MonoBehaviour {

    public Text text;


    private string[] messages = new string[] {
        "Press E to ignore elbow",
        "Press E to include elbow"

    };

    private bool ignored;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        ignored = QuestionController.Instance.ignoreElbow;
        int state = 0;
        if (ignored)
        {
            state = 1;
            text.color = Color.grey;
        }
        else
        {
            state = 0;
            text.color = Color.white;

        }

        text.text = messages[state];


    }
}

