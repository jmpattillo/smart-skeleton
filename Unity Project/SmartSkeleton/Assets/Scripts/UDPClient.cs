using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;

public class UDPClient : MonoBehaviour {

    public int portListen = 65001;
    public string ipSend = "192.168.4.1";
    public int portSend = 4210;
    public byte[] Databuffer;
    public bool dataready = false;
    public bool repeating = false;

    //public GameObject[] notifyObjects;
    //public string messageToNotify;

  //  private string received = "";
   // private bool repeat = false;
  //  private bool quaternion_recieved = false;

    private UdpClient client;
    private Thread receiveThread;
    private IPEndPoint remoteEndPoint;

    private GameObject panel;
    // private IPAddress ipAddressSend;
    public static UDPClient Instance;

  //  private float currentTime;
    private float elapsedTime;
    private float hangTime;


    private void Awake()
    {

        Instance = this;
        //Check if the ip address entered is valid. If not, sendMessage will broadcast to all ip addresses 

        IPAddress ip;
        if (IPAddress.TryParse(ipSend, out ip))
        {

            remoteEndPoint = new IPEndPoint(ip, portSend);

        }
        else
        {

            remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, portSend);
        }

        //Initialize client and thread for receiving

        client = new UdpClient(portListen);

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

    }

    // Use this for initialization
    void Start()
    {

       // currentTime = 0.0f;
        elapsedTime = 0.0f;
        hangTime = 0.5f;


    }

    // Update is called once per frame
    void Update()
    {
        //Check if a message has been recibed
       /* if (received != "")
        {

            Debug.Log("UDPClient: message received \'" + received + "\'");

            //Notify each object defined in the array with the message received

            received = "";
        } */

        if (Input.GetKeyDown(KeyCode.Q)) { SendValue("XQX"); }
        //if (Input.GetKeyDown(KeyCode.I)) { SendValue("XIX"); repeating = false; }
        //if (Input.GetKeyDown(KeyCode.Z)) { SendValue("XZX"); }
        //if (Input.GetKeyDown(KeyCode.D)) { SendValue("XDX"); }
        if (Input.GetKeyDown(KeyCode.R))
        {
            repeating = true;
            SendValue("XQX");
            panel = GameObject.Find("Intro text");
            panel.GetComponent<Text> ().enabled = false;
            panel = GameObject.Find("Intro panel");
            panel.gameObject.SetActive(false);

        }

        //if (Input.GetKeyDown(KeyCode.O)) { SendValue("X9X"); }
        //if (Input.GetKeyDown(KeyCode.I)) { SendValue("X0X"); }
        //if (Input.GetKeyDown(KeyCode.K)) { SendValue("XgX"); }
        //if (Input.GetKeyDown(KeyCode.L)) { SendValue("XcX"); }
        //if (Input.GetKeyDown(KeyCode.M)) { Debug.Log("Mark"); }

        //if (quaternion_recieved) { quaternion_recieved = false; if (repeat) { print("I'm repeating"); SendValue("XQX"); } }

        //put function here to check for hangs


        if (repeating) {
            float t = Time.time - elapsedTime;
           // Debug.Log(t);
            if (t >= hangTime) { SendValue("XQX"); Debug.Log("resent request " + t); }
        }



    }

    //Call this method to send a message from this app to ipSend using portSend
    public void SendValue(string valueToSend)
    {
        try
        {
            if (valueToSend != "")
            {

                //Get bytes from string
                byte[] data = Encoding.UTF8.GetBytes(valueToSend);

                // Send bytes to remote client
                client.Send(data, data.Length, remoteEndPoint);
               // Debug.Log("UDPClient: send \'" + valueToSend + "\'");
                //Clear message
                valueToSend = "";
                elapsedTime = Time.time;

            }
        }
        catch (Exception err)
        {
            Debug.LogError("Error udp send : " + err.Message);
        }

    }

    //This method checks if the app receives any message
    public void ReceiveData()
    {

        while (true)
        {

            try
            {
                // Bytes received
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                Databuffer = client.Receive(ref anyIP);
                string thistime = ByteArrayToString(Databuffer);
               // Debug.Log("UDPClient: received \'" + thistime + "\'");
                dataready = true;
                //int end = data.Length - 1;
                //if (data[0] == 0x51) { if (data[end] == 0x51) { print("quaternion received"); quaternion_recieved = true; } }

                // Bytes into text
                //string text = ByteArrayToString(data);
                //text = Encoding.UTF8.GetString(data);

                //received = text;

            }
            catch (Exception err)
            {
                Debug.Log("Error:" + err.ToString());
            }
           
        }
    }

    //Exit UDP client
    public void OnDisable()
    {
        if (receiveThread != null)
        {
            receiveThread.Abort();
            receiveThread = null;
        }
        client.Close();
        Debug.Log("UDPClient: exit");
    }

   public static string ByteArrayToString(byte[] ba)
    {
        StringBuilder hex = new StringBuilder(ba.Length * 2);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    } 
}
