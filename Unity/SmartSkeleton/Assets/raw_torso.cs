using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class raw_torso : MonoBehaviour {

    public static raw_torso Instance;

    public Vector3 rawEulers = new Vector3();
    public Quaternion temp = new Quaternion();
    public int thisVariation;
    public bool xneg;
    public bool yneg;
    public bool zneg;
    public float rightrotation;
    public float backrotation;
    public float uprotation;


    private int xblah;
    private int yblah;
    private int zblah;

    //private int blah;

    void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start () {
        thisVariation = 0;
        xneg = false;
        yneg = false;
        zneg = false;
		
	}
	
	// Update is called once per frame
	void Update () {

        if (xneg) { xblah = -1; } else { xblah = 1; }
        if (yneg) { yblah = -1; } else { yblah = 1; }
        if (zneg) { zblah = -1; } else { zblah = 1; }


        temp = ConvertoUnity(Rotator.Instance.torso, thisVariation);
        //temp = Rotator.Instance.torso;

        //print(Rotator.Instance.torso);
        // temp = Rotator.Instance.torso;
        // temp = QuaternionExt.GetNormalized(temp);
        // rawEulers = temp.eulerAngles;
        // temp = Quaternion.Euler(rawEulers);
        // temp = Quaternion.Euler(rawEulers.y, rawEulers.z, rawEulers.x);
        // transform.localRotation = Rotator.Instance.torso;
        temp *= Quaternion.Euler(Vector3.right * rightrotation);
        temp *= Quaternion.Euler(Vector3.up * uprotation);
        temp *= Quaternion.Euler(Vector3.back * backrotation); 
        transform.localRotation = temp;

		
	}

    Quaternion ConvertoUnity(Quaternion input, int mixup){

        if (mixup == 1)
        {
            return new Quaternion(
            xblah * input.x,
            zblah * input.z,
            yblah * input.y,
            input.w
                );
        }

        else if (mixup == 2)
        {
            return new Quaternion(
                yblah * input.y,
                xblah * input.x,
                zblah * input.z,
            input.w
                );
        }

        else if (mixup == 3)
        {
            return new Quaternion(
                yblah * input.y,
                zblah * input.z,
                xblah * input.x,
            input.w
                );
        }

        else if (mixup == 4)
        {
            return new Quaternion(
                zblah * input.z,
                xblah * input.x,
                yblah * input.y,
            input.w
                );
        }

        else if (mixup == 5)
        {
            return new Quaternion(
                zblah * input.z,
                yblah * input.y,
                xblah * input.x,
            input.w
                );
        }

        else {
            return new Quaternion(
                xblah * input.x,
                yblah * input.y,
                zblah * input.z,
                input.w);
            }

    }

}
