using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KneeJoint : MonoBehaviour {

	public static KneeJoint Instance;

    public float flex_extend;
    public Vector3 kneePosition;

    public Quaternion relativeKnee = new Quaternion();

	void Awake ()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        relativeKnee = (Rotator.Instance.leg) * (Quaternion.Inverse(Rotator.Instance.femur));
        transform.localRotation = relativeKnee;

        kneePosition = transform.position;
        Vector3 tmp = AnkleJoint.Instance.transform.position - kneePosition;
        Vector3 tmp2 = HipJoint.Instance.transform.position - kneePosition;
        float angle = Vector3.Angle(tmp, tmp2);
        flex_extend = (180 - angle);
		
	}
}
