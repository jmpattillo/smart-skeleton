using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ulna : MonoBehaviour {

	public static Ulna Instance;
	//private Vector3 wrist_to_shoulder = new Vector3();
	//private Ray ray = new Ray();
	//private Plane plane = new Plane();
	public Vector3 marker = new Vector3();
	public Quaternion localQuat = new Quaternion();
	public float xrot;
	public float radius_rot;
	

	void Awake ()
	{
		Instance = this;
	}
	

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
	 /*	wrist_to_shoulder = ShoulderJoint.Instance.transform.position - distalUlna.Instance.transform.position;
		ray.origin =distalUlna.Instance.transform.position;
		ray.direction = wrist_to_shoulder;
		plane.SetNormalAndPosition (transform.right, transform.position);
		plane.SetNormalAndPosition (transform.right, Ulna.Instance.transform.position);
		float rayDistance;
		if (plane.Raycast (ray, out rayDistance)) {
			marker = ray.GetPoint (rayDistance);
		} else {
			Debug.Log ("ray did not intersect");
		}
		transform.LookAt(sphereMover.Instance.transform); */
		
		
	/*	if (Rotator.Instance.radius_rotation < 180) {
			transform.localRotation = Quaternion.Euler(-Rotator.Instance.radius_rotation,0f,0f);
		} else if (Rotator.Instance.radius_rotation > 180) {
			  transform.localRotation = Quaternion.Euler((360-Rotator.Instance.radius_rotation),0f,0f);
		}

		localQuat = transform.localRotation;
		xrot = localQuat.eulerAngles.x;
		radius_rot = Rotator.Instance.radius_rotation; */
		
	}


}
