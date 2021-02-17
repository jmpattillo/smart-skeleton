using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnkleJoint : MonoBehaviour {


	public static AnkleJoint Instance;
    public Quaternion relativeAnkle = new Quaternion();
    public float flex_extend;
    public float invert_evert;

	void Awake ()
	{
		Instance = this;
	}	


// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        //relativeAnkle = (Rotator.Instance.foot) * (Quaternion.Inverse(KneeJoint.Instance.relativeKnee));
        relativeAnkle = (Rotator.Instance.foot) * (Quaternion.Inverse(Rotator.Instance.leg));
        transform.localRotation = relativeAnkle;

        float angle = transform.localEulerAngles.y;
        flex_extend = (angle > 180) ? angle - 360 : angle;

        angle = transform.localEulerAngles.z;
        invert_evert = (angle > 180) ? angle - 360 : angle;

	//transform.rotation = Rotator.Instance.foot;
		
	}
}
