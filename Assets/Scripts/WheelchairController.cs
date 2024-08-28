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

public class WheelchairController : MonoBehaviour
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

    public Rigidbody Wheelchair;
    public Rigidbody leftWheel; // 왼쪽 바퀴의 Rigidbody
    public Rigidbody rightWheel; // 오른쪽 바퀴의 Rigidbody
    public float speed = 500f; // 이동 속도
    public float turnSpeed = 100f; // 회전 속도
    public Camera Cam;
    public Vector3 CamRotation;
    public Vector3 prev_CamRotation;
    public bool Mobile;
    public Vector3 drone_pos;
    public Vector3 prev_pos;

    public string fileName = "bci_path_";
    public string path;
    void Start()
    {
        m_fnReceiveHandler = new AsyncCallback(handleDataReceive);
        // Change the IP here accordingly for socket connection. Enter IP (ipconfig in cmd of this computer)
        remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000); //127.0.0.1   143.248.137.106 # port = 14000 192.168.0.5
        //prev_pos = Wheelchair.transform.position;

        prev_CamRotation = Cam.transform.localEulerAngles;
        leftWheel.maxAngularVelocity = 8;
        rightWheel.maxAngularVelocity = 8;
        //StartCoroutine(getSocket());  
    }

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
        CamRotation = Cam.transform.localEulerAngles;
        Vector3 WheelchairRotation = Wheelchair.transform.localEulerAngles;
        Vector3 localVelocity = transform.InverseTransformDirection(Wheelchair.velocity);
        Vector3 localAngularVelocity = Wheelchair.transform.InverseTransformDirection(Wheelchair.angularVelocity);
        Debug.Log("Velocity: "+localVelocity);
        //Debug.Log("leftwheel: "+leftWheel.maxAngularVelocity);
        //Debug.Log("rightwheel: "+rightWheel.maxAngularVelocity);
        // float torqueFactor = 500f;
        // if (WheelchairRotation.z > 2 && WheelchairRotation.z <= 180) 
        // { Wheelchair.AddRelativeTorque(0, 0, -torqueFactor); }
        // if (WheelchairRotation.z > 180 && WheelchairRotation.z <= 358) 
        // { Wheelchair.AddRelativeTorque(0, 0, torqueFactor); }

        // // // x 축 회전
        // if (WheelchairRotation.x > 2 && WheelchairRotation.x <= 180) 
        // { Wheelchair.AddRelativeTorque(-torqueFactor, 0, 0); }
        // if (WheelchairRotation.x > 180 && WheelchairRotation.x <= 358) 
        // { Wheelchair.AddRelativeTorque(torqueFactor, 0, 0); }

        float leftSpeed = 0f;
        float rightSpeed = 0f;
        float currentSpeed = localVelocity.z;
        float acceleration = 10f;
        if (Mobile == false)
        {
        // 입력 값 가져오기
            if (Input.GetKey(KeyCode.W)) // 직진
            {
                if (speed<1000f) {speed += 0.5f;}
                if (Input.GetKey(KeyCode.A)){
                    //turnSpeed += 0.5f;
                    leftSpeed = 0f;
                    rightSpeed = speed+turnSpeed;
                }
                else if (Input.GetKey(KeyCode.D)){
                    //turnSpeed += 0.5f;
                    leftSpeed = speed+turnSpeed;
                    rightSpeed = 0f;
                }
                else{
                    leftSpeed = speed;
                    rightSpeed = speed;
                }
            }
            else if (Input.GetKey(KeyCode.S)) // 정지
            {
                //speed -= 0.5f;
                
                if(localVelocity.z>0){
                    leftSpeed = -speed;
                    rightSpeed = -speed;
                }
                else{
                    leftSpeed = 0f;
                    rightSpeed = 0f;
                }
                
            }
            else if (Input.GetKey(KeyCode.A)) // 왼쪽 회전
            {
                //turnSpeed += 0.5f;
                leftSpeed = -turnSpeed;
                rightSpeed = turnSpeed;
            }
            else if (Input.GetKey(KeyCode.D)) // 오른쪽 회전
            {
                //turnSpeed += 0.5f;
                leftSpeed = turnSpeed;
                rightSpeed = -turnSpeed;
            }
            else{
                if(speed>300f){speed -= 0.5f;}
                
                //turnSpeed -= 0.5f;
            }

            // 속도 제한
            //leftSpeed = Mathf.Clamp(leftSpeed, -600f, 600f);
            //rightSpeed = Mathf.Clamp(rightSpeed, -600f, 600f);

            // 바퀴 회전력 적용
            leftWheel.AddRelativeTorque(Vector3.right * leftSpeed);
            rightWheel.AddRelativeTorque(Vector3.right * rightSpeed);

            //leftWheel.AddRelativeForce(Vector3.right * leftSpeed);
            //rightWheel.AddRelativeForce(Vector3.right * rightSpeed);
        }
    }
}
