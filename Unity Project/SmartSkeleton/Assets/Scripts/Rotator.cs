using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Rotator : MonoBehaviour {

	public static Rotator Instance;
	
	const double scale = (1.0 / (1<<14)); //this is copied from the adafruit library for the BNO055

	//private UnitySerialPort unitySerialPort;
    private UDPClient udpClient;

    private byte[] thisdata;

	//private Quaternion theseangles;
	public Quaternion torso = new Quaternion();
	public Quaternion arm = new Quaternion();
	public Quaternion forearm = new Quaternion();
	public Quaternion hand = new Quaternion();
	public Quaternion femur = new Quaternion();
	public Quaternion leg = new Quaternion();
	public Quaternion foot = new Quaternion();

	public Quaternion cal_arm = new Quaternion(0f,0f,0f,1f);
	public Quaternion cal_forearm = new Quaternion(0f,0f,0f,1f);
	public Quaternion cal_hand = new Quaternion(0f,0f,0f,1f);
	public Quaternion cal_femur = new Quaternion(0f,0f,0f,1f);
	public Quaternion cal_leg = new Quaternion(0f,0f,0f,1f);
	public Quaternion cal_foot = new Quaternion(0f,0f,0f,1f);
	
	private Quaternion baseline = new Quaternion(0f,0f,0f,1f);

	
	public bool isCalibrated = false;
  //  private bool firstCalibration = true;
    private int frameCounter = 0;
    private bool allQuaternionsReceived = false;
    private bool upperLimbQuatsReceived = false;
    private bool lowerLimbQuatsReceived = false;
    private bool calibStatusReceived = false;
    private bool offsetsReceived = false;
    private bool statusReceived = false;


	private double w_torso_quat = 1;
	private double x_torso_quat = 0;
	private double y_torso_quat = 0;
	private double z_torso_quat = 0;

	private double w_arm_quat = 1;
	private double x_arm_quat = 0;
	private double y_arm_quat = 0;
	private double z_arm_quat = 0;

	private double w_forearm_quat = 1;
	private double x_forearm_quat = 0;
	private double y_forearm_quat = 0;
	private double z_forearm_quat = 0;

	private double w_hand_quat = 1;
	private double x_hand_quat = 0;
	private double y_hand_quat = 0;
	private double z_hand_quat = 0;

	private double w_femur_quat = 1;
	private double x_femur_quat = 0;
	private double y_femur_quat = 0;
	private double z_femur_quat = 0;

	private double w_leg_quat = 1;
	private double x_leg_quat = 0;
	private double y_leg_quat = 0;
	private double z_leg_quat = 0;

	private double w_foot_quat = 1;
	private double x_foot_quat = 0;
	private double y_foot_quat = 0;
	private double z_foot_quat = 0;

    private float timeSense;

	void Awake () {

		Instance = this;
	}


	void Start () {
	    // Register reference to the UnitySerialPort. This
        // was defined in the scripts Awake function so we
        // know it is instantiated before this call.

       // unitySerialPort = UnitySerialPort.Instance;
        udpClient = UDPClient.Instance;
		thisdata = new byte[128];
        timeSense = Time.time;
		

	}
	
	// Update is called once per frame
	void Update ()
	{


		//if (unitySerialPort.DataBuffer != null) { //need another way to detect.  A boolean set to true in UDPclient script when data is received set that boolean to false here after dealing with it
        if (udpClient.dataready) {
            //unitySerialPort.DataBuffer.CopyTo (thisdata, 0);
            int end = udpClient.Databuffer.Length - 1;

            if (udpClient.Databuffer[0] != udpClient.Databuffer[end]) { udpClient.SendValue("XQX"); Debug.Log("bad data"); }
            if (udpClient.Databuffer[0] == 0x51) { if (udpClient.Databuffer[end] == 0x51) { allQuaternionsReceived = true; udpClient.Databuffer.CopyTo(thisdata, 0); } }
            if (udpClient.Databuffer[0] == 0x55) { if (udpClient.Databuffer[end] == 0x55) { upperLimbQuatsReceived = true; udpClient.Databuffer.CopyTo(thisdata, 1); } }
            if (udpClient.Databuffer[0] == 0x4F) { if (udpClient.Databuffer[end] == 0x4F) { lowerLimbQuatsReceived = true; udpClient.Databuffer.CopyTo(thisdata, 1); } }
            if (udpClient.Databuffer[0] == 0x43) { if (udpClient.Databuffer[end] == 0x43) { calibStatusReceived = true; udpClient.Databuffer.CopyTo(thisdata, 1); } }
            if (udpClient.Databuffer[0] == 0x53) { if (udpClient.Databuffer[end] == 0x53) { offsetsReceived = true; udpClient.Databuffer.CopyTo(thisdata, 1); } }
            if (udpClient.Databuffer[0] == 0x49) { if (udpClient.Databuffer[end] == 0x49) { statusReceived = true; udpClient.Databuffer.CopyTo(thisdata, 1); } }
            if (udpClient.Databuffer[0] == 0x47) { if (udpClient.Databuffer[end] == 0x47) { statusReceived = true; udpClient.Databuffer.CopyTo(thisdata, 1); } }
            //maybe add additional check of buffer length. 
            udpClient.dataready = false;





            //put handing routines for different kinds of incoming data here or in the UDP client script?

            if (allQuaternionsReceived)
            { //TODO clean all this up so it is not needlessly calculating data not sent.  change data types?
              // print("got the quats");
              //print(UDPClient.ByteArrayToString(thisdata));
                w_torso_quat = ((short)((((ushort)(thisdata[2])) << 8) | ((ushort)(thisdata[1])))) * scale;
                x_torso_quat = ((short)((((ushort)(thisdata[4])) << 8) | ((ushort)(thisdata[3])))) * scale;
                y_torso_quat = ((short)((((ushort)(thisdata[6])) << 8) | ((ushort)(thisdata[5])))) * scale;
                z_torso_quat = ((short)((((ushort)(thisdata[8])) << 8) | ((ushort)(thisdata[7])))) * scale;

                w_arm_quat = ((short)((((ushort)(thisdata[10])) << 8) | ((ushort)(thisdata[9])))) * scale;
                x_arm_quat = ((short)((((ushort)(thisdata[12])) << 8) | ((ushort)(thisdata[11])))) * scale;
                y_arm_quat = ((short)((((ushort)(thisdata[14])) << 8) | ((ushort)(thisdata[13])))) * scale;
                z_arm_quat = ((short)((((ushort)(thisdata[16])) << 8) | ((ushort)(thisdata[15])))) * scale;

                w_forearm_quat = ((short)((((ushort)(thisdata[18])) << 8) | ((ushort)(thisdata[17])))) * scale;
                x_forearm_quat = ((short)((((ushort)(thisdata[20])) << 8) | ((ushort)(thisdata[19])))) * scale;
                y_forearm_quat = ((short)((((ushort)(thisdata[22])) << 8) | ((ushort)(thisdata[21])))) * scale;
                z_forearm_quat = ((short)((((ushort)(thisdata[24])) << 8) | ((ushort)(thisdata[23])))) * scale;

                w_hand_quat = ((short)((((ushort)(thisdata[26])) << 8) | ((ushort)(thisdata[25])))) * scale;
                x_hand_quat = ((short)((((ushort)(thisdata[28])) << 8) | ((ushort)(thisdata[27])))) * scale;
                y_hand_quat = ((short)((((ushort)(thisdata[30])) << 8) | ((ushort)(thisdata[29])))) * scale;
                z_hand_quat = ((short)((((ushort)(thisdata[32])) << 8) | ((ushort)(thisdata[31])))) * scale;

                w_femur_quat = ((short)((((ushort)(thisdata[34])) << 8) | ((ushort)(thisdata[33])))) * scale;
                x_femur_quat = ((short)((((ushort)(thisdata[36])) << 8) | ((ushort)(thisdata[35])))) * scale;
                y_femur_quat = ((short)((((ushort)(thisdata[38])) << 8) | ((ushort)(thisdata[37])))) * scale;
                z_femur_quat = ((short)((((ushort)(thisdata[40])) << 8) | ((ushort)(thisdata[39])))) * scale;

                w_leg_quat = ((short)((((ushort)(thisdata[42])) << 8) | ((ushort)(thisdata[41])))) * scale;
                x_leg_quat = ((short)((((ushort)(thisdata[44])) << 8) | ((ushort)(thisdata[43])))) * scale;
                y_leg_quat = ((short)((((ushort)(thisdata[46])) << 8) | ((ushort)(thisdata[45])))) * scale;
                z_leg_quat = ((short)((((ushort)(thisdata[48])) << 8) | ((ushort)(thisdata[47])))) * scale;

                w_foot_quat = ((short)((((ushort)(thisdata[50])) << 8) | ((ushort)(thisdata[49])))) * scale;
                x_foot_quat = ((short)((((ushort)(thisdata[52])) << 8) | ((ushort)(thisdata[51])))) * scale;
                y_foot_quat = ((short)((((ushort)(thisdata[54])) << 8) | ((ushort)(thisdata[53])))) * scale;
                z_foot_quat = ((short)((((ushort)(thisdata[56])) << 8) | ((ushort)(thisdata[55])))) * scale; 

                assignQuats();
                allQuaternionsReceived = false;
                if (udpClient.repeating) { udpClient.SendValue("XQX");  }
               // float currentTime = Time.time;
                //Debug.Log(currentTime - timeSense);
               // timeSense = currentTime;

            }

           

            if (statusReceived) { print(UDPClient.ByteArrayToString(thisdata)); statusReceived = false; }

            if (calibStatusReceived) {}



           /* if ((Time.frameCount > 4)&&(firstCalibration)) { //this automatically calibrates when application is started.  
                captureCalibration();                        //a delay of four frames give time for positions to be registered
                firstCalibration = false;
            } */

            if (!isCalibrated) {
                frameCounter++;
                if (frameCounter == 4) {
                    captureCalibration();
                    frameCounter = 0;
                }
            }
		
		}

//        else { Debug.Log("data is not ready"); }

        if (Input.GetKeyDown(KeyCode.C))
        { 
          //captureCalibration();
            isCalibrated = false; //this triggers recalibration
        }




	} //end void update

	private void assignQuats ()
	{

		torso.w = (float)w_torso_quat;
		torso.x = (float)x_torso_quat;
		torso.y = (float)y_torso_quat;
		torso.z = (float)z_torso_quat; 



      /*  torso.w = (float)w_torso_quat;
        torso.x = (float)x_torso_quat;
        torso.y = (float)z_torso_quat;
        torso.z = (float)y_torso_quat; 

        torso.w = (float)w_torso_quat;
        torso.x = (float)y_torso_quat;
        torso.y = (float)x_torso_quat;
        torso.z = (float)z_torso_quat; 

        torso.w = (float)w_torso_quat;
        torso.x = (float)y_torso_quat;
        torso.y = (float)z_torso_quat;
        torso.z = (float)x_torso_quat; 

        torso.w = (float)w_torso_quat;
        torso.x = (float)z_torso_quat;
        torso.y = (float)y_torso_quat;
        torso.z = (float)x_torso_quat; 

        torso.w = (float)w_torso_quat;
        torso.x = (float)z_torso_quat;
        torso.y = (float)x_torso_quat;
        torso.z = (float)y_torso_quat;  

        torso.w = (float)x_torso_quat;
        torso.x = (float)w_torso_quat;
        torso.y = (float)y_torso_quat;
        torso.z = (float)z_torso_quat; 

        torso.w = (float)x_torso_quat;
        torso.x = (float)w_torso_quat;
        torso.y = (float)z_torso_quat;
        torso.z = (float)y_torso_quat; 

        torso.w = (float)x_torso_quat;
        torso.x = (float)y_torso_quat;
        torso.y = (float)w_torso_quat;
        torso.z = (float)z_torso_quat; 

        torso.w = (float)x_torso_quat;
        torso.x = (float)y_torso_quat;
        torso.y = (float)z_torso_quat;
        torso.z = (float)w_torso_quat; 

        torso.w = (float)x_torso_quat;
        torso.x = (float)z_torso_quat;
        torso.y = (float)y_torso_quat;
        torso.z = (float)w_torso_quat; 

        torso.w = (float)x_torso_quat;
        torso.x = (float)z_torso_quat;
        torso.y = (float)w_torso_quat;
        torso.z = (float)y_torso_quat; 

        torso.w = (float)y_torso_quat;
        torso.x = (float)w_torso_quat;
        torso.y = (float)x_torso_quat;
        torso.z = (float)z_torso_quat; 

        torso.w = (float)y_torso_quat;
        torso.x = (float)w_torso_quat;
        torso.y = (float)z_torso_quat;
        torso.z = (float)x_torso_quat; 

        torso.w = (float)y_torso_quat;
        torso.x = (float)x_torso_quat;
        torso.y = (float)w_torso_quat;
        torso.z = (float)z_torso_quat; 

        torso.w = (float)y_torso_quat;
        torso.x = (float)x_torso_quat;
        torso.y = (float)z_torso_quat;
        torso.z = (float)w_torso_quat; 

        torso.w = (float)y_torso_quat;
        torso.x = (float)z_torso_quat;
        torso.y = (float)x_torso_quat;
        torso.z = (float)w_torso_quat; 

        torso.w = (float)y_torso_quat;
        torso.x = (float)z_torso_quat;
        torso.y = (float)w_torso_quat;
        torso.z = (float)x_torso_quat; 

        torso.w = (float)z_torso_quat;
        torso.x = (float)w_torso_quat;
        torso.y = (float)x_torso_quat;
        torso.z = (float)y_torso_quat; 

        torso.w = (float)z_torso_quat;
        torso.x = (float)w_torso_quat;
        torso.y = (float)y_torso_quat;
        torso.z = (float)x_torso_quat; 

        torso.w = (float)z_torso_quat;
        torso.x = (float)x_torso_quat;
        torso.y = (float)w_torso_quat;
        torso.z = (float)y_torso_quat; 

        torso.w = (float)z_torso_quat;
        torso.x = (float)x_torso_quat;
        torso.y = (float)y_torso_quat;
        torso.z = (float)w_torso_quat; 

        torso.w = (float)z_torso_quat;
        torso.x = (float)y_torso_quat;
        torso.y = (float)x_torso_quat;
        torso.z = (float)w_torso_quat; 

        torso.w = (float)z_torso_quat;
        torso.x = (float)y_torso_quat;
        torso.y = (float)w_torso_quat;
        torso.z = (float)x_torso_quat; */


		arm.w = (float)w_arm_quat;
		arm.x = (float)x_arm_quat;
		arm.y = (float)y_arm_quat;
		arm.z = (float)z_arm_quat;

		forearm.w = (float)w_forearm_quat;
		forearm.x = (float)x_forearm_quat;
		forearm.y = (float)y_forearm_quat;
		forearm.z = (float)z_forearm_quat;

		hand.w = (float)w_hand_quat;
		hand.x = (float)x_hand_quat;
		hand.y = (float)y_hand_quat;
		hand.z = (float)z_hand_quat;

		femur.w = (float)w_femur_quat;
		femur.x = (float)x_femur_quat;
		femur.y = (float)y_femur_quat;
		femur.z = (float)z_femur_quat;

		leg.w = (float)w_leg_quat;
		leg.x = (float)x_leg_quat;
		leg.y = (float)y_leg_quat;
		leg.z = (float)z_leg_quat;

		foot.w = (float)w_foot_quat;
		foot.x = (float)x_foot_quat;
		foot.y = (float)y_foot_quat;
		foot.z = (float)z_foot_quat;

        //try rotating torso quat

       // torso *= Quaternion.Euler(Vector3.back * 90);
      //  torso = Quaternion.Inverse(torso) * baseline;


		//normalize quats to torso
		/*arm = Quaternion.Inverse (torso) * arm;
		forearm = Quaternion.Inverse (torso) * forearm;
		hand = Quaternion.Inverse (torso) * hand;
		femur = Quaternion.Inverse (torso) * femur;
		leg = Quaternion.Inverse (torso) * leg;
		foot = Quaternion.Inverse (torso) * foot; */

		//normalize quats to torso
		arm = Quaternion.Inverse (torso) * arm;
		forearm = Quaternion.Inverse (torso) * forearm;
		hand = Quaternion.Inverse (torso) * hand;
		femur = Quaternion.Inverse (torso) * femur;
		leg = Quaternion.Inverse (torso) * leg;
		foot = Quaternion.Inverse (torso) * foot; 

		//apply calibration

		if (isCalibrated) {
			arm = cal_arm * arm;
			forearm = cal_forearm * forearm;
			hand = cal_hand * hand;
			femur = cal_femur * femur;
			leg = cal_leg * leg;
			foot = cal_foot * foot;
		} 

		//store angles for uniaxial and biaxial motions
	/*	elbow_angles = forearm.eulerAngles;
		wrist_angles = hand.eulerAngles;
		shoulder_angles = arm.eulerAngles;

		elbow_ext_flex = (elbow_angles.y - 360) * -1;
		radius_rotation = elbow_angles.x;
		wrist_ext_flex = wrist_angles.y;
		wrist_adduct_abduct = wrist_angles.z;
		shoulder_ext_flex = shoulder_angles.y;
		shoulder_adduct_abduct = shoulder_angles.z;
		shoulder_rotation = shoulder_angles.x; */

	}

	private void captureCalibration ()
	{
		if (!isCalibrated) {
			//set cal quaternions to current quaternion

			cal_arm = Quaternion.Inverse (arm) * baseline;
			cal_forearm = Quaternion.Inverse(forearm) * baseline;
			cal_hand = Quaternion.Inverse(hand) * baseline;
			cal_femur = Quaternion.Inverse(femur) * baseline;
			cal_leg = Quaternion.Inverse(leg) * baseline;
			cal_foot = Quaternion.Inverse(foot) * baseline;			
			isCalibrated = true;
		} else {
			isCalibrated = false;
		}
		
	}

    public void resetCalibration ()
    {
        if (isCalibrated) {
            //isCalibrated = false;
            if (frameCounter == 1)
            {
                cal_arm = Quaternion.Inverse(arm) * baseline;
                cal_forearm = Quaternion.Inverse(forearm) * baseline;
                cal_hand = Quaternion.Inverse(hand) * baseline;
                cal_femur = Quaternion.Inverse(femur) * baseline;
                cal_leg = Quaternion.Inverse(leg) * baseline;
                cal_foot = Quaternion.Inverse(foot) * baseline;
                frameCounter = 0;
            }
            else { frameCounter++; }
            //isCalibrated = true;
        }
    }

}
