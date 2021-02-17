using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kneeColor : MonoBehaviour {

    private const float transp = 0.4f;

    private Color redColor = new Color(1f, 0f, 0f, transp);
    private Color greenColor = new Color(0f, 1f, 0f, transp);
    private Color blueColor = new Color(0f, 0f, 1f, transp);
    private Color yellowColor = new Color(1f, 1f, 0f, transp);
    private Color magentaColor = new Color(1f, 0f, 1f, transp);
    private Color invisible = new Color(1f, 1f, 1f, 0f);

    private UDPClient udpClient;

    private int currentState;
    private int previousState;

    void Start()
    {
        udpClient = UDPClient.Instance;
        currentState = 0;
        previousState = 0;
        GetComponent<Renderer>().material.color = invisible;

    }

	
	// Update is called once per frame
    void Update()
    {
        currentState = QuestionController.Instance.kneeState;
        if (currentState != previousState)
        {
            if (QuestionController.Instance.ignoreKnee == false)
            {
                if (QuestionController.Instance.kneeState == 0)
                {
                    GetComponent<Renderer>().material.color = invisible;
                    udpClient.SendValue("XtX"); //turn knee LED off
                }
                else if (QuestionController.Instance.kneeState == 1)
                {
                    GetComponent<Renderer>().material.color = blueColor;
                    udpClient.SendValue("XrX"); //turn knee LED blue
                }
                else if (QuestionController.Instance.kneeState == 2)
                {
                    GetComponent<Renderer>().material.color = yellowColor; //this should never happen
                }
                else if (QuestionController.Instance.kneeState == 3)
                {
                    GetComponent<Renderer>().material.color = magentaColor; //this should never happen
                }
                else if (QuestionController.Instance.kneeState == 4)
                {
                    GetComponent<Renderer>().material.color = greenColor;
                }
                else if (QuestionController.Instance.kneeState == 5)
                {
                    GetComponent<Renderer>().material.color = redColor;
                }

                previousState = currentState;
            }
            else { GetComponent<Renderer>().material.color = invisible; }
        }
    }
}
