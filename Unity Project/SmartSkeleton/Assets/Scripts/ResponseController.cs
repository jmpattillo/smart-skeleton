using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ResponseController : MonoBehaviour {

	public Text text;

    /*private string[] messages = new string[] {

	"Ready",
	"Correct!  Return to anatomical position and press n", //put parsed response + return to anatomical and press n
	"Incorrect! Try Again" }; */

    //private int state;

    public static ResponseController Instance;

    void Awake()
    {
        Instance = this;   
    }




    // Update is called once per frame
    void Update () {
		//state = QuestionController.Instance.questionState;
	//	text.text = messages[state];
       // if (state == 0) 
       
	}
}
