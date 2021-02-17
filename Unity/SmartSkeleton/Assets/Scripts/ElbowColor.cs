using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElbowColor : MonoBehaviour {

	private const float transp = 0.4f;

	private Color redColor = new Color(1f,0f,0f,transp);
	private Color greenColor = new Color(0f,1f,0f,transp);
	private Color blueColor = new Color(0f,0f,1f,transp);
	private Color yellowColor = new Color(1f,1f,0f,transp);
	private Color magentaColor = new Color(1f,0f,1f,transp);
	private Color invisible = new Color(1f,1f,1f,0f);

    private UDPClient udpClient;

    private int currentState;
    private int previousState;

	// Use this for initialization
	void Start () {

        udpClient = UDPClient.Instance;
        currentState = 0;
        previousState = 0;
        GetComponent<Renderer>().material.color = invisible;
		
	}

    // Update is called once per frame
    void Update()
    {
        currentState = QuestionController.Instance.elbowState;
        if (currentState != previousState)
        {
            if (QuestionController.Instance.ignoreElbow == false)
            {
                if (QuestionController.Instance.elbowState == 0)
                {
                    GetComponent<Renderer>().material.color = invisible;
                    udpClient.SendValue("XhX"); //turn elbow LED off

                }
                else if (QuestionController.Instance.elbowState == 1)
                {
                    GetComponent<Renderer>().material.color = blueColor;
                    udpClient.SendValue("XeX"); //turn elbow LED blue
                }
                else if (QuestionController.Instance.elbowState == 2)
                {
                    GetComponent<Renderer>().material.color = yellowColor;
                    udpClient.SendValue("XfX"); //turn elbow LED yellow
                }
                else if (QuestionController.Instance.elbowState == 3)
                {
                    GetComponent<Renderer>().material.color = magentaColor; //this should never happen

                }
                else if (QuestionController.Instance.elbowState == 4)
                {
                    GetComponent<Renderer>().material.color = greenColor;

                }
                else if (QuestionController.Instance.elbowState == 5)
                {
                    GetComponent<Renderer>().material.color = redColor;
                }

                previousState = currentState;
            }
            else {GetComponent<Renderer>().material.color = invisible;}
        }
    }


	/*	if (QuestionController.Instance.elbowState == 0) {
			GetComponent<Renderer> ().material.color = invisible;
		} else if (QuestionController.Instance.elbowState == 1) {
			GetComponent<Renderer> ().material.color = blueColor;
		} else if (QuestionController.Instance.elbowState == 2) {
			GetComponent<Renderer> ().material.color = yellowColor;
		} else if (QuestionController.Instance.elbowState == 3) {
			GetComponent<Renderer> ().material.color = magentaColor;
		} else if (QuestionController.Instance.elbowState == 4) {
			GetComponent<Renderer> ().material.color = greenColor;		
		} else if (QuestionController.Instance.elbowState == 5) {
			GetComponent<Renderer> ().material.color = redColor;
		} */



		
	
}
