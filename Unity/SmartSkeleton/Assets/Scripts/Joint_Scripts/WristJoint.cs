using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WristJoint : MonoBehaviour {

	// Use this for initialization

	public static WristJoint Instance;
	public float flex_extend;
	public float abduct_adduct;
   // public Vector3 wristPosition;

    public Quaternion relativeWrist = new Quaternion();

	void Awake ()
	{
		Instance = this;
	}


	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        //set orientation of hand

       // relativeWrist = (Rotator.Instance.hand) * (Quaternion.Inverse(UlnaJoint.Instance.relativeElbow));
       // relativeWrist = (Rotator.Instance.hand) * (Quaternion.Inverse(Rotator.Instance.arm));
       // relativeWrist = relativeWrist * (Quaternion.Inverse(Rotator.Instance.forearm));
        relativeWrist = (Rotator.Instance.hand) * (Quaternion.Inverse(Rotator.Instance.forearm));
        // transform.localRotation = Rotator.Instance.hand;
        transform.localRotation = relativeWrist;
        // transform.rotation = Rotator.Instance.hand;

        // wristPosition = transform.TransformPoint(0f, 0f, 0f);

        //calculate motion angles
        float angle = transform.localEulerAngles.y;
        flex_extend = (angle > 180) ? angle - 360 : angle; //converts from 0->360 to -180->+180

        angle = transform.localEulerAngles.z;
        abduct_adduct = (angle > 180) ? angle - 360 : angle;
		



        //set orientation of hand
       // transform.localRotation = Rotator.Instance.hand;

       // wristPosition = transform.TransformPoint(0f, 0f, 0f);
		
		//calculate motion angles
		//float angle = transform.localEulerAngles.y;
		//flex_extend = (angle > 180) ? angle - 360 : angle; //converts from 0->360 to -180->+180

		//angle = transform.localEulerAngles.z;
		//abduct_adduct = (angle > 180) ? angle - 360 : angle;

		

		
	}
}
