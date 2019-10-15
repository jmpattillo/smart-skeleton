using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class elbow_position : MonoBehaviour {

    public static elbow_position Instance;
    public Vector3 elbowPosition;

    private void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start () {


		
	}
	
	// Update is called once per frame
	void Update () {

        elbowPosition = transform.position;
		
	}
}
