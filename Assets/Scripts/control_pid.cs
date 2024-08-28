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

public class PIDController
{
    public float Kp;
    public float Ki;
    public float Kd;

    private float integral;
    private float lastError;

    public PIDController(float kp, float ki, float kd)
    {
        Kp = kp;
        Ki = ki;
        Kd = kd;
    }

    public float Compute(float setpoint, float actual, float deltaTime)
    {
        float error = setpoint - actual;
        integral += error * deltaTime;
        float derivative = (error - lastError) / deltaTime;
        lastError = error;
        return Kp * error + Ki * integral + Kd * derivative;
    }
}

public class control_pid : MonoBehaviour
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

    public float SpeedIncreaseRate = 5f;
    public float SpeedDecreaseRate = 3f;
    public float MaxForwardBackwardSpeed = 200f;
    private float ForwardBackwardSpeed = 0f;

    public float RotationSpeedIncreaseRate = 2f;
    public float RotationSpeedDecreaseRate = 2f;
    public float MaxRotationSpeed = 100f;
    private float RotationSpeed = 0f;

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

    private PIDController pidX;
    private PIDController pidZ;

    public float targetAngleX = 0f;
    public float targetAngleZ = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        m_fnReceiveHandler = new AsyncCallback(handleDataReceive);
        // Change the IP here accordingly for socket connection. Enter IP (ipconfig in cmd of this computer)
        remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000); //127.0.0.1   143.248.137.106 # port = 14000 192.168.0.5
        prev_pos = Drone.transform.position;

        prev_CamRotation = Cam.transform.localEulerAngles;

        pidX = new PIDController(1f, 0f, 0.1f); // Adjust these values for better control
        pidZ = new PIDController(1f, 0f, 0.1f);
        
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
    // Update is called once per frame
    void FixedUpdate()
    {
        CamRotation = Cam.transform.localEulerAngles;
        Vector3 localVelocity = transform.InverseTransformDirection(Drone.velocity);
        // Debug.Log("Input: "+input);
        Debug.Log("Velocity: "+localVelocity);


        // Debug.Log("CameraAngle: "+ CamRotation.x.ToString() + "  " + CamRotation.y.ToString() + "  " + CamRotation.z.ToString());
        DroneRotation = Drone.transform.localEulerAngles;
                if (Mobile == false)
        {
            y_angle = CamRotation.y < 360 && CamRotation.y > 180 ? (int)CamRotation.y - 360 : (int)CamRotation.y;

            if (Input.GetKey(KeyCode.W))
            {
                ForwardBackwardSpeed += SpeedIncreaseRate * Time.deltaTime;
                if (ForwardBackwardSpeed > MaxForwardBackwardSpeed)
                    ForwardBackwardSpeed = MaxForwardBackwardSpeed;
                Drone.AddRelativeForce(0, 0, ForwardBackwardSpeed);
            }
            else
            {
                ForwardBackwardSpeed -= SpeedDecreaseRate * Time.deltaTime;
                if (ForwardBackwardSpeed < 0)
                    ForwardBackwardSpeed = 0;
                Drone.AddRelativeForce(0, 0, ForwardBackwardSpeed);
            }

            if (Input.GetKey(KeyCode.A))
            {
                RotationSpeed += RotationSpeedIncreaseRate * Time.deltaTime;
                if (RotationSpeed > MaxRotationSpeed)
                    RotationSpeed = MaxRotationSpeed;
                Drone.AddRelativeTorque(0, -RotationSpeed, 0);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                RotationSpeed += RotationSpeedIncreaseRate * Time.deltaTime;
                if (RotationSpeed > MaxRotationSpeed)
                    RotationSpeed = MaxRotationSpeed;
                Drone.AddRelativeTorque(0, RotationSpeed, 0);
            }
            else
            {
                RotationSpeed -= RotationSpeedDecreaseRate * Time.deltaTime;
                if (RotationSpeed < 0)
                    RotationSpeed = 0;
                if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
                {
                    RotationSpeed = 0;
                }
            }

            float deltaTime = Time.fixedDeltaTime;
            float controlX = pidX.Compute(targetAngleX, DroneRotation.x, deltaTime);
            float controlZ = pidZ.Compute(targetAngleZ, DroneRotation.z, deltaTime);

            Drone.AddRelativeTorque(controlX, 0, controlZ);
        }
        else if (Mobile == true)
        {
            Drone.AddRelativeForce(0, 0, Ly / 2);
            if (Ly > 5) { Drone.AddRelativeTorque(10, 0, 0); }
            if (Ly < -5) { Drone.AddRelativeTorque(-10, 0, 0); }
            Drone.AddRelativeForce(Rx, 0, 0);
            if (Rx > 5) { Drone.AddRelativeTorque(0, 0, -10); }
            if (Rx < -5) { Drone.AddRelativeTorque(0, 0, 10); }
            Drone.AddRelativeForce(0, Ry / 2, 0);
        }

        if (input == "qq" || input == "q")
        {
            Application.Quit();
        }
    
    }
}
