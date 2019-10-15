using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoulderJoint : MonoBehaviour {

	public static ShoulderJoint Instance;
	public float flex_extend;
	public float rotations;
	public float abduct_adduct;
    //public Vector3 shoulderPosition;
    //Quaternion locatRot = Quaternion.Euler(0f, 0f, -90f);

	void Awake ()
	{
		Instance = this;
	}
	

	// Use this for initialization
	void Start () {

       // shoulderPosition = transform.TransformPoint(0f, 0f, 0f);
		
	}
	
	// Update is called once per frame
	void Update ()
	{


        transform.localRotation = Rotator.Instance.arm;
		
		float angle = transform.localEulerAngles.y;
		flex_extend = (angle > 180) ? angle - 360 : angle; //converts from 0->360 to -180->+180
		
		angle = transform.localEulerAngles.x;
		rotations = (angle > 180) ? angle - 360 : angle;

		angle = transform.localEulerAngles.z;
		abduct_adduct = (angle > 180) ? angle -360 : angle;
		
		
		
	}
}
