using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scoreController : MonoBehaviour {

    public Text text;
    public static scoreController Instance;

    private void Awake()
    {
        Instance = this;
    }


    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
