using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera_mover : MonoBehaviour {

    public GameObject skeleton;
    private float speedMod = 60.0f;
    private float m_FieldOfView = 21.0f;
    private float min_FOV = 5.0f;
    private float max_FOV = 40.0f;
    private float FOV_increment = 0.2f;
                              
                             
   // private Vector3 point;



	// Use this for initialization
	void Start () {
        //  point = skeleton.transform.position;
        //  transform.LookAt(point);
        //transform.Rotate(0f,0f,0f);
        Camera.main.fieldOfView = m_FieldOfView;
		
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.RotateAround(skeleton.transform.position, Vector3.up, speedMod * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.RotateAround(skeleton.transform.position, Vector3.down, speedMod * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += Vector3.up * 4 * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.position += Vector3.down * 4 * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.RightBracket)) 
        {
            if (Camera.main.fieldOfView <= min_FOV) 
            {
                Camera.main.fieldOfView = min_FOV;
            }
            else 
            {
                Camera.main.fieldOfView = Camera.main.fieldOfView - FOV_increment;
            }
        
        }

        if (Input.GetKey(KeyCode.LeftBracket))
        {
            if (Camera.main.fieldOfView >= max_FOV)
            {
                Camera.main.fieldOfView = max_FOV;
            }
            else 
            {
                Camera.main.fieldOfView = Camera.main.fieldOfView + FOV_increment;
            }
        }
        
		
	}
}
