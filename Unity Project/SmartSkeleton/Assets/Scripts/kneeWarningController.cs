using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class kneeWarningController : MonoBehaviour {

    public Text text;


    private string[] messages = new string[] {
        "Press K to ignore knee",
        "Press K to include knee"

    };

    private bool ignored;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        ignored = QuestionController.Instance.ignoreKnee;
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
