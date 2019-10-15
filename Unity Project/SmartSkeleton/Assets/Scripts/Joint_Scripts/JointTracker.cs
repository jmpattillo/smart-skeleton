using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointTracker : MonoBehaviour {

   // public Vector3 wristPosition;// = new Vector3();
  //  public Vector3 elbowPosition;
   // public Vector3 shoulderPosition;
   // public float wristTOshoulder;
    //public float elbowTOwrist;
    //public float shoulderTOelbow;
    //public float angleOFelbow;

	// Use this for initialization
	void Start () {
        
     //   shoulderPosition = ShoulderJoint.Instance.transform.TransformPoint(0f, 0f, 0f);

       // shoulderTOelbow = Vector3.Distance(shoulderPosition, elbowPosition);
	}
	
	// Update is called once per frame
	void Update () {

  //      wristPosition = WristJoint.Instance.transform.TransformPoint(0f,0f,0f);
    //    elbowPosition = UlnaJoint.Instance.transform.TransformPoint(0f, 0f, 0f);

      //  Vector3 tmp = wristPosition - elbowPosition;
       // Vector3 tmp2 = shoulderPosition - elbowPosition;
       // angleOFelbow = Vector3.Angle(tmp, tmp2);

      //  wristTOshoulder = Vector3.Distance(wristPosition, shoulderPosition);
      //  elbowTOwrist = Vector3.Distance(elbowPosition, wristPosition);

      //  float tmp =  Mathf.Acos(((shoulderTOelbow * shoulderTOelbow) + (elbowTOwrist * elbowTOwrist) - (wristTOshoulder * wristTOshoulder)) / (2 * shoulderTOelbow * elbowTOwrist));
      //  angleOFelbow = tmp * Mathf.Rad2Deg; 
	}
}
