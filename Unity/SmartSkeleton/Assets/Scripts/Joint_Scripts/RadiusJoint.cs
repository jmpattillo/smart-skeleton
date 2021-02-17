using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadiusJoint : MonoBehaviour {

    public static RadiusJoint Instance;

   // private Vector3 radiusAxis;
   // private Vector3 distal;

    void Awake() {
        Instance = this;
    }


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        transform.localRotation = Rotator.Instance.forearm;

      /*  distal = distalUlna.Instance.transform.position;
        radiusAxis = transform.position - distal;
        float rotangle = Rotator.Instance.forearm.eulerAngles.x;
        transform.rotation = Quaternion.AngleAxis(rotangle, radiusAxis);

        float localx = transform.eulerAngles.x;
        if (localx <= 180)
        {
            transform.Rotate((localx * -1), 0f, 0f);
        }
        else
        {
            transform.Rotate((localx + (360 - localx)), 0f, 0f);
        }


        float localy = transform.eulerAngles.y;
        if (localy <= 180)
        {
            transform.Rotate(0f, (localy * -1), 0f);
        }
        else
        {
            transform.Rotate(0f, (localy + (360 - localy)), 0f);
        }



        float localz = transform.eulerAngles.z;
        if (localz <= 180)
        {
            transform.Rotate(0f, 0f, (localz * -1));
        }
        else
        {
            transform.Rotate(0f, 0f, (localz + (360 - localz))); 
        } 


        transform.RotateAround(transform.position, radiusAxis, transform.eulerAngles.x); */





       // if (Input.GetKeyDown(KeyCode.Q)) { transform.RotateAround(transform.position, radiusAxis, 45f); }

		
	}
}
