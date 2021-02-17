using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UlnaJoint : MonoBehaviour {

	public static UlnaJoint Instance;
	

	public float flex_extend;
    public float flex_extend_new;
	public float pronate_supinate;
    public Vector3 elbowPosition;
    public Vector3 localEulers;

    public Quaternion relativeElbow = new Quaternion();



	void Awake ()
	{
		Instance = this;
	}

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
        //set rotation of radius and ulna.  TODO seperate animation of radius and ulna for
        //realistic forearm animation. 

        relativeElbow = (Rotator.Instance.forearm) * (Quaternion.Inverse(Rotator.Instance.arm));

       // transform.localRotation = Rotator.Instance.forearm;
        transform.localRotation = relativeElbow;

        /*   localEulers = transform.eulerAngles;
           float localx = transform.eulerAngles.x;
           if (localx <= 180){
               transform.Rotate((localx * -1), 0f, 0f);
           }
           else {
               transform.Rotate((localx + (360 - localx)), 0f, 0f);
           }

           float localz = transform.eulerAngles.z;
           if (localz <= 180)
           {
               transform.Rotate(0f, 0f, (localz * -1));
           }
           else
           {
               transform.Rotate(0f, 0f, (localz + (360 - localz)));
           } */



        elbowPosition = transform.position;
        //testposition = transform.position;

        //calculate motion angles
        Vector3 tmp = WristJoint.Instance.transform.position - transform.position;
        Vector3 tmp2 = ShoulderJoint.Instance.transform.position - transform.position;
        float angle = Vector3.Angle(tmp, tmp2);
        flex_extend = (180 - angle);

        //angle = transform.localEulerAngles.y;
        //flex_extend = (angle > 180) ? angle - 360 : angle; //converts from 0->360 to -180->+180

        angle = transform.localEulerAngles.x;
        pronate_supinate = (angle > 180) ? angle - 360 : angle;


        //set rotation of radius and ulna.  TODO seperate animation of radius and ulna for
        //realistic forearm animation.
       // transform.position = elbow_position.Instance.transform.position;
		//transform.localRotation = Rotator.Instance.forearm;

     /*   localEulers = transform.eulerAngles;
        float localx = transform.eulerAngles.x;
        if (localx <= 180){
            transform.Rotate((localx * -1), 0f, 0f);
        }
        else {
            transform.Rotate((localx + (360 - localx)), 0f, 0f);
        }

        float localz = transform.eulerAngles.z;
        if (localz <= 180)
        {
            transform.Rotate(0f, 0f, (localz * -1));
        }
        else
        {
            transform.Rotate(0f, 0f, (localz + (360 - localz)));
        } */



       /* elbowPosition = transform.position;
        //testposition = transform.position;

        //calculate motion angles
        Vector3 tmp = WristJoint.Instance.transform.position - transform.position;
        Vector3 tmp2 = ShoulderJoint.Instance.transform.position - transform.position;
        float angle = Vector3.Angle(tmp, tmp2);
        flex_extend = (180 - angle); 

		//angle = transform.localEulerAngles.y;
		//flex_extend = (angle > 180) ? angle - 360 : angle; //converts from 0->360 to -180->+180
		
		angle = transform.localEulerAngles.x;
		pronate_supinate = (angle > 180) ? angle - 360 : angle; */
		
	}
}
