using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoulderColor : MonoBehaviour {

	private const float transp = 0.4f;
    private UDPClient udpClient;

	private Color redColor = new Color(1f,0f,0f,transp);
	private Color greenColor = new Color(0f,1f,0f,transp);
	private Color blueColor = new Color(0f,0f,1f,transp);
	private Color yellowColor = new Color(1f,1f,0f,transp);
	private Color magentaColor = new Color(1f,0f,1f,transp);
	private Color invisible = new Color(1f,1f,1f,0f);

    private int currentState;
    private int previousState;


	// Use this for initialization
	void Start () {

        udpClient = UDPClient.Instance;
        currentState = 0;
        previousState = 0;
	    GetComponent<Renderer> ().material.color = invisible;
		
	}
	
	// Update is called once per frame
	void Update ()
	{
        currentState = QuestionController.Instance.shoulderState;

        if (currentState != previousState)
        {
            if (QuestionController.Instance.shoulderState == 0)
            {
                GetComponent<Renderer>().material.color = invisible;
                udpClient.SendValue("XmX"); //turn shoulder LED off

            }
            else if (QuestionController.Instance.shoulderState == 1)
            {
                GetComponent<Renderer>().material.color = blueColor;
                udpClient.SendValue("XiX"); //turn shoulder LED blue
            }
            else if (QuestionController.Instance.shoulderState == 2)
            {
                GetComponent<Renderer>().material.color = yellowColor;
                udpClient.SendValue("XjX"); //turn shoulder LED yellow
            }
            else if (QuestionController.Instance.shoulderState == 3)
            {
                GetComponent<Renderer>().material.color = magentaColor;
                udpClient.SendValue("XkX");
            }
            else if (QuestionController.Instance.shoulderState == 4)
            {
                GetComponent<Renderer>().material.color = greenColor;

            }
            else if (QuestionController.Instance.shoulderState == 5)
            {
                GetComponent<Renderer>().material.color = redColor;
            }

            previousState = currentState;
        }

		
	}
}



