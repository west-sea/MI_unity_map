using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
public class WheelchairKeyboard : MonoBehaviour
{
    public class AsyncObject
    {
        public Byte[] Buffer;
        public Socket WorkingSocket;
        public AsyncObject(Int32 bufferSize)
        {
            this.Buffer = new Byte[bufferSize];
        }
    }
    
    private AsyncCallback m_fnReceiveHandler;
    private AsyncCallback m_fnSendHandler;
    Socket client = null;
    IPEndPoint remoteEP = null;
    byte[] bytes = new byte[1024];
    List<string> outputlist_str;
    public decimal out0=0;
    public decimal out1=0;
    public decimal out2=0;
    public decimal out3=0;
    public int y_angle = 0;
    List<decimal> outputlist = new List<decimal>(){(decimal) 0,(decimal) 0,(decimal) 0,(decimal) 0};

    CultureInfo culture = new CultureInfo("en-US");

    public Boolean socket_received = true;
    string input = "r";
    public float error_cnt = 0;
    public float error = 0;
    public float kp = 1f;
    public float ki = 0f;
    public float kd = 100f;
    public Boolean au = true;
    public int direction = 0;
    public int cnt = 0;
    public int vertical = 0;
    public int iRx = 0;
    public Rigidbody Drone;
    public GameObject RButton;
    public GameObject LButton;
    public Camera Cam;
    
    // Adjust these values in method FixedUpdate()
    /*Speed*/
    public int ForwardBackward = 200; // Original Value: 4
    /*Speed*/
    public int Tilt = 50; //50
    /*Speed*/
    public int FlyLeftRight = 50; //50
    /*Speed*/
    public int UpDown = 50; //50
    /* Direction Range (in degree) */
    public int DirRange = 10; // if the user tries to look towards direction x degree, the direction will point there as long as the user looks within the range of x-10 ~ x+10 


    private Vector3 DroneRotation;
    public Vector3 drone_pos;
    public Vector3 prev_pos;
    public Vector3 CamRotation;
    public Vector3 prev_CamRotation;
    public bool Mobile;
    private float angle;
    private float Rx;
    private float Ry;
    private float Lx;
    private float Ly;

    public string fileName = "bci_path_";
    public string path;

    void Start()
    {
        m_fnReceiveHandler = new AsyncCallback(handleDataReceive);
        // Change the IP here accordingly for socket connection. Enter IP (ipconfig in cmd of this computer)
        remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000); //127.0.0.1   143.248.137.106 # port = 14000 192.168.0.5
        prev_pos = Drone.transform.position;

        prev_CamRotation = Cam.transform.localEulerAngles;
        StartCoroutine(getSocket());  
    }

    // Update is called once per frame
    private void handleDataReceive(IAsyncResult ar)
    {
        AsyncObject ao = (AsyncObject)ar.AsyncState;
        Int32 recvBytes;

        recvBytes = ao.WorkingSocket.EndReceive(ar);

        if (recvBytes > 0)
        {
            Byte[] msgByte = new Byte[recvBytes];
            Array.Copy(ao.Buffer, msgByte, recvBytes);
            input = Encoding.ASCII.GetString(msgByte);
            socket_received = true;
        }
        try
        {
            ao.WorkingSocket.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
        }
        catch (Exception)
        {
            print("connection failed");
            return;
        }
    }

    
    private IEnumerator getSocket()
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            client.Connect(remoteEP);
            Debug.Log("print: conntected"); // delete later
        }
        catch
        {
            print("connection failed");
        }

        AsyncObject ao = new AsyncObject(4096);
        ao.WorkingSocket = client;
        // For socket send (if(true) part)
        while(true)
        {
            if(true)
            {
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(Cam.transform.localEulerAngles.ToString());
                Debug.Log("Before send (time): "+DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());
                client.Send(byData);
                Debug.Log("After send (time): "+DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());
                Debug.Log("SENT!: "+Cam.transform.localEulerAngles.ToString());

                socket_received=false;
            }
            client.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
        
        // byte[] buffer = new byte[1024];
            yield return new WaitForSecondsRealtime((float)0.1);
        }
    }


    void FixedUpdate()
    {
        /* Speed not applied for some reason unless value assigned here */
        ForwardBackward = 8;
        UpDown = 8;
        Tilt = 8;

        CamRotation = Cam.transform.localEulerAngles;
        Debug.Log("Input: "+input);
        Debug.Log("Velocity: "+Drone.velocity);

        // Debug.Log("CameraAngle: "+ CamRotation.x.ToString() + "  " + CamRotation.y.ToString() + "  " + CamRotation.z.ToString());
        DroneRotation = Drone.transform.localEulerAngles;
        if (DroneRotation.z > 10 && DroneRotation.z <= 180) { Drone.AddRelativeTorque(0, 0, -10); }//if tilt too big(stabilizes drone on z-axis)
        if (DroneRotation.z > 180 && DroneRotation.z <= 350) { Drone.AddRelativeTorque(0, 0, 10); }//if tilt too big(stabilizes drone on z-axis)
        if (DroneRotation.z > 1 && DroneRotation.z <= 10) { Drone.AddRelativeTorque(0, 0, -3); }//if tilt not very big(stabilizes drone on z-axis)
        if (DroneRotation.z > 350 && DroneRotation.z < 359) { Drone.AddRelativeTorque(0, 0, 3); }//if tilt not very big(stabilizes drone on z-axis)


        if (Mobile == true)
        {
            Drone.AddRelativeTorque(0, Lx / 5, 0);//tilt drone left and right

        }

        if (DroneRotation.x > 10 && DroneRotation.x <= 180) { Drone.AddRelativeTorque(-10, 0, 0); }//if tilt too big(stabilizes drone on x-axis)
        if (DroneRotation.x > 180 && DroneRotation.x <= 350) { Drone.AddRelativeTorque(10, 0, 0); }//if tilt too big(stabilizes drone on x-axis)
        if (DroneRotation.x > 1 && DroneRotation.x <= 10) { Drone.AddRelativeTorque(-3, 0, 0); }//if tilt not very big(stabilizes drone on x-axis)
        if (DroneRotation.x > 350 && DroneRotation.x < 359) { Drone.AddRelativeTorque(3, 0, 0); }//if tilt not very big(stabilizes drone on x-axis)

        //Drone.AddForce(0,9,0);//drone not lose height very fast, if you want not to lose height al all change 9 into 9.80665 
        Drone.AddForce(0, (float)9.81, 0);
        drone_pos = Drone.transform.position;
        // Debug.Log("Drone_Speed: "+ForwardBackward.ToString()+" "+UpDown.ToString()+" "+Tilt.ToString());
        //(7, 17)(17, 55)(55, 65)
        //(-15, -5)(-5, 55)(55, 65)(65, 125)(125, 135)
        
        if (Mobile == false)
        {
            /*  if statements with Input.GetKey(?) is for controlling the drone using keyboard inputs  */ 
            /*
                W: fly forward A: rotate left S: fly back D: rotate right Up: fly up Down: fly down Right: tilt right Left: tilt left
            */
            y_angle = CamRotation.y<360 && CamRotation.y>180 ? (int)CamRotation.y-360:(int)CamRotation.y; 
            //Debug.Log("angle: "+y_angle);
            if (Input.GetKey(KeyCode.W)) { Drone.AddRelativeForce(0, 0, ForwardBackward); }
            else if (Input.GetKey(KeyCode.LeftArrow)){ Drone.AddRelativeForce(FlyLeftRight / -1, 0, 0); Drone.AddRelativeTorque(0, 0, 10); error_cnt = 0; }//rotate drone left
            else if (Input.GetKey(KeyCode.A)) { Drone.AddRelativeTorque(0, Tilt / -1, 0); }//tilt drone left
            else if (Input.GetKey(KeyCode.D)) { Drone.AddRelativeTorque(0, Tilt, 0); }//tilt drone right
            else if (Input.GetKey(KeyCode.RightArrow)) { Drone.AddRelativeForce(FlyLeftRight, 0, 0); Drone.AddRelativeTorque(0, 0, -10); error_cnt = 0; }//rotate drone right
            //else if (Input.GetKey(KeyCode.S)) { Drone.AddRelativeForce(0, 0, ForwardBackward / -1);/*Drone.AddRelativeTorque (-10, 0, 0);*/}// drone fly backward
            else if (Input.GetKey(KeyCode.UpArrow)) { Drone.AddRelativeForce(0, UpDown, 0); error_cnt = 0; }//drone fly up
            else if (Input.GetKey(KeyCode.DownArrow)) { Drone.AddRelativeForce(0, UpDown / -1, 0); error_cnt = 0; }//drone fly down
            
            // Additional if for these arrow keys will allow the drone to tilt and go up/down quicker
            if (Input.GetKey(KeyCode.LeftArrow)) { Drone.AddRelativeForce(FlyLeftRight / -1, 0, 0); Drone.AddRelativeTorque(0, 0, 10); error_cnt = 0; }//rotate drone left 
            if (Input.GetKey(KeyCode.RightArrow)) { Drone.AddRelativeForce(FlyLeftRight, 0, 0); Drone.AddRelativeTorque(0, 0, -10); error_cnt = 0; }//rotate drone right
            // if (Input.GetKey(KeyCode.S)) { Drone.AddRelativeForce(0, 0, ForwardBackward / -1); Drone.AddRelativeTorque(-10, 0, 0); }// drone fly backward // not too sure why we had additional S key
            // if (Input.GetKey(KeyCode.UpArrow)) { Drone.AddRelativeForce(0, UpDown, 0); }//drone fly up
            // if (Input.GetKey(KeyCode.DownArrow)) { Drone.AddRelativeForce(0, UpDown / -1, 0); }//drone fly down

            /*  F: Move towards the camera direction  */
            if (Input.GetKey(KeyCode.F)) 
            {
                /*  First adjust y rotation (rotation along the plane or ground) */
                if (CamRotation.y > DirRange && CamRotation.y < 360-DirRange)
                {
                     if (CamRotation.y < 180) {Drone.AddRelativeTorque(0, Tilt, 0);} //rotate drone right
                     else {Drone.AddRelativeTorque(0, Tilt / -1, 0);} //rotate drone left
                }
                /* y rotation fixed within the range (fixed along the plane), move towards upward/front direction */
                else
                {
                    // TODO: Specific value of added force may change according to experimental settings
                    Drone.AddRelativeForce(0, 0, (float)ForwardBackward * (float)Math.Cos((360-CamRotation.x)*Math.PI/180));  // with updown (0, (float)UpDown * (float)Math.Sin((360-CamRotation.x)*Math.PI/180), (float)ForwardBackward * (float)Math.Cos((360-CamRotation.x)*Math.PI/180));          
                }
            }

            if (input == "rr" || input == "r") // rest
            {
                Drone.AddRelativeForce(0, 0, 0);
            }
            else if (input == "ll" || input == "l") // turn right 
            {
                Drone.AddRelativeTorque(0, Tilt, 0);
            }
            else if (input == "jj" || input == "j") // turn left
            {
                Drone.AddRelativeTorque(0, Tilt / -1, 0);
            }
            else if (input == "ii" || input == "i") // move forward
            {
                Drone.AddRelativeForce(0, 0, ForwardBackward);
            }
            /* For six class drone control */
            else if (input == "ww" || input == "w") //drone fly up
            { 
                Drone.AddRelativeForce(0, UpDown, 0);
            }
            else if (input == "ss" || input == "s") //drone fly down
            { 
                Drone.AddRelativeForce(0, UpDown / -1, 0); 

            }


            // /* For two class drone control */
            // if (input == "l") // for concentration movement (TODO: change input value later)
            // {
            //     /*  First adjust y rotation (rotation along the plane or ground) */
            //     if (CamRotation.y > DirRange && CamRotation.y < 360-DirRange)
            //     {
            //          if (CamRotation.y < 180) {Drone.AddRelativeTorque(0, Tilt, 0);} //rotate drone right
            //          else {Drone.AddRelativeTorque(0, Tilt / -1, 0);} //rotate drone left
            //     }
            //     /* y rotation fixed within the range (fixed along the plane), move towards upward/front direction */
            //     else
            //     {
            //         // TODO: Specific value of added force may change according to experimental settings
            //         Drone.AddRelativeForce(0, 0, (float)ForwardBackward * (float)Math.Cos((360-CamRotation.x)*Math.PI/180));  // with updown (0, (float)UpDown * (float)Math.Sin((360-CamRotation.x)*Math.PI/180), (float)ForwardBackward * (float)Math.Cos((360-CamRotation.x)*Math.PI/180));                    
            //     }
            // }
        }
        if (Mobile == true)
        {
            Drone.AddRelativeForce(0, 0, Ly / 2); if (Ly > 5) { Drone.AddRelativeTorque(10, 0, 0); }; if (Ly < -5) { Drone.AddRelativeTorque(-10, 0, 0); }//drone fly forward or backward

            Drone.AddRelativeForce(Rx, 0, 0); if (Rx > 5) { Drone.AddRelativeTorque(0, 0, -10); }; if (Rx < -5) { Drone.AddRelativeTorque(0, 0, 10); }


            Drone.AddRelativeForce(0, Ry / 2, 0);//drone fly up or down
        }
        if(input == "qq" || input == "q")
        {
            Application.Quit();
        }
    }
}
