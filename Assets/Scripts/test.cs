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

//(경로, 충돌, (key input, timestamp) , random key)

public class test : MonoBehaviour
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
    public bool random=false; 
    public float interval = 1f; // 초당 한 번 실행하기 위한 간격
    private float timer_interval = 0f;
    private float timer_duation = 0f;
    public char keycode = 'q';
    public string CurrCommand = "q";

    private string PrevCommand = "q"; 

    public float duration = 3f;
    public Camera Cam;


    
    // Adjust these values in method FixedUpdate()
    /*Speed*/
    public int ForwardBackward = 200; // Original Value: 4
    /*Speed*/
    public int Tilt = 50; //50
    /*Speed*/
    public int FlyLeftRight = 10; //50
    /*Speed*/
    public int UpDown = 50; //50
    /* Direction Range (in degree) */
    public int DirRange = 10; // if the user tries to look towards direction x degree, the direction will point there as long as the user looks within the range of x-10 ~ x+10 

    public float ForwardBackwardSpeed = 100f;

    // private float RightRotationSpeed = 0f;
    // private float LeftRotationSpeed = 0f;
    public float RotationSpeed = 1000f;
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

    private int curr_torque = 0;
    public char prev_key = 'q';
    public string zone = "None";
    private bool hasEntered = false;

   

    void Start()
    {
        m_fnReceiveHandler = new AsyncCallback(handleDataReceive);
        // Change the IP here accordingly for socket connection. Enter IP (ipconfig in cmd of this computer)
        remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000); //127.0.0.1   143.248.137.106 # port = 14000 192.168.0.5
        prev_pos = Drone.transform.position;

        prev_CamRotation = Cam.transform.localEulerAngles;
        //StartCoroutine(getSocket());  
        StartSocketConnection();
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

    private void StartSocketConnection()
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            client.Connect(remoteEP);
            Debug.Log("Connected to socket server");

            AsyncObject ao = new AsyncObject(4096);
            ao.WorkingSocket = client;
            client.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
        }
        catch (Exception e)
        {
            Debug.LogError("Socket connection failed: " + e.Message);
        }
    }

    private void SendData(string data)
    {
        if (client != null && client.Connected)
        {
            byte[] byData = Encoding.ASCII.GetBytes(data);
            client.Send(byData);
        }
    }

    
    void OnTriggerEnter(Collider other){
        
        if (!hasEntered){
            if(other.CompareTag("Straight")){
                zone = "Straight";
            }
            else if (other.CompareTag("Curve1")){
                zone = "Curve1";
            }
            else if (other.CompareTag("Curve2")){
                zone = "Curve2";
            }
            else if (other.CompareTag("Finish")){
                zone = "Finish";
            }
            SendData(zone);
            Debug.Log(zone+"/"+(DateTimeOffset.Now.ToUnixTimeMilliseconds()*1e-3).ToString());
            Debug.Log(hasEntered);
            
            hasEntered = true;
        }

    }

    void OnTriggerExit(Collider other)
    {
        hasEntered = false;
        Debug.Log("EXIT");
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

    IEnumerator getKeyCode()
    {
        while (true)
        {
            // Execute your code here
            Debug.Log("Executing at interval: " + Time.time);

            // Wait for the specified interval
            yield return new WaitForSeconds(interval);
        }
    }

    void FixedUpdate()
    {
        /* Speed not applied for some reason unless value assigned here */
        // ForwardBackward = 50;
        // UpDown = 8;
        // Tilt = 30;
        CamRotation = Cam.transform.localEulerAngles;
        Vector3 localVelocity = transform.InverseTransformDirection(Drone.velocity);
        Vector3 localAngularVelocity = Drone.transform.InverseTransformDirection(Drone.angularVelocity);
        //ForwardBackwardSpeed = localVelocity.z;
        //RotationSpeed = localAngularVelocity.y;
        // Debug.Log("Input: "+input);
        float baseForce = 50f; // 기본 힘
        float acceleration = 5f;  // 가속도 조절을 위한 변수
        float deceleration = 5f;  // 감속도 조절을 위한 변수
        float maxForce = 100f; // 최대 힘, 휠체어의 움직임을 제한하지 않기 위해 필요
        //Debug.Log("Velocity: "+localVelocity);

        // 속도 크기 (magnitude)를 계산하여 현재 속도를 구합니다.
        float currentSpeed = localVelocity.magnitude;

        // 가속도나 감속도를 속도에 따라 조정합니다.
        float speedFactor = Mathf.Clamp(currentSpeed, 0, maxForce);
        float forwardForce = baseForce * (1 + speedFactor);
        // Debug.Log("CameraAngle: "+ CamRotation.x.ToString() + "  " + CamRotation.y.ToString() + "  " + CamRotation.z.ToString());
        DroneRotation = Drone.transform.localEulerAngles;
        // 회전 스태빌라이즈이션
        float torqueFactor = 1000f; // 회전 스태빌라이즈이션 토크 강도
        //Debug.Log("LOCAL:"+DroneRotation);
        //Debug.Log("GLOBAL:"+Drone.angularVelocity);
        

        // z 축 회전
        if (DroneRotation.z > 2 && DroneRotation.z <= 180) 
        { Drone.AddRelativeTorque(0, 0, -torqueFactor); }
        if (DroneRotation.z > 180 && DroneRotation.z <= 358) 
        { Drone.AddRelativeTorque(0, 0, torqueFactor); }

        // x 축 회전
        if (DroneRotation.x > 2 && DroneRotation.x <= 180) 
        { Drone.AddRelativeTorque(-torqueFactor, 0, 0); }
        if (DroneRotation.x > 180 && DroneRotation.x <= 358) 
        { Drone.AddRelativeTorque(torqueFactor, 0, 0); }





        // // 속도 기반 스태빌라이즈이션
        // float speedThreshold = 5f; // 스태빌라이즈이션을 적용할 속도 임계값
        // float speedStabilizeFactor = 2f; // 속도 스태빌라이즈이션 토크 강도

        // // 속도가 임계값을 넘어가면 추가 스태빌라이즈이션 토크 적용
        // if (localVelocity.magnitude > speedThreshold)
        // {
        //     // 회전 속도에 따라 반대 방향의 토크를 적용하여 회전을 제어
        //     Drone.AddRelativeTorque(-localAngularVelocity.x * speedStabilizeFactor, 0, -localAngularVelocity.z * speedStabilizeFactor);
        // }

        //드론이 뒤집히지 않도록 중력에 대한 토크를 추가
        // Vector3 gravityDirection = -Physics.gravity.normalized;
        // float gravityTorqueFactor = 1000f; // 중력 토크 강도
        // Drone.AddTorque(Vector3.Cross(Drone.transform.up, gravityDirection) * gravityTorqueFactor);


        // if (DroneRotation.z > 10 && DroneRotation.z <= 180) { Drone.AddRelativeTorque(0, 0, -10); }//if tilt too big(stabilizes drone on z-axis)
        // if (DroneRotation.z > 180 && DroneRotation.z <= 350) { Drone.AddRelativeTorque(0, 0, 10); }//if tilt too big(stabilizes drone on z-axis)
        // if (DroneRotation.z > 1 && DroneRotation.z <= 10) { Drone.AddRelativeTorque(0, 0, -3); }//if tilt not very big(stabilizes drone on z-axis)
        // if (DroneRotation.z > 350 && DroneRotation.z < 359) { Drone.AddRelativeTorque(0, 0, 3); }//if tilt not very big(stabilizes drone on z-axis)


        // if (Mobile == true)
        // {
        //     Drone.AddRelativeTorque(0, Lx / 5, 0);//tilt drone left and right

        // }

        // if (DroneRotation.x > 10 && DroneRotation.x <= 180) { Drone.AddRelativeTorque(-10, 0, 0); }//if tilt too big(stabilizes drone on x-axis)
        // if (DroneRotation.x > 180 && DroneRotation.x <= 350) { Drone.AddRelativeTorque(10, 0, 0); }//if tilt too big(stabilizes drone on x-axis)
        // if (DroneRotation.x > 1 && DroneRotation.x <= 10) { Drone.AddRelativeTorque(-3, 0, 0); }//if tilt not very big(stabilizes drone on x-axis)
        // if (DroneRotation.x > 350 && DroneRotation.x < 359) { Drone.AddRelativeTorque(3, 0, 0); }//if tilt not very big(stabilizes drone on x-axis)

        //Drone.AddForce(0,9,0);//drone not lose height very fast, if you want not to lose height al all change 9 into 9.80665 
        //Drone.AddForce(0, (float)9.81, 0);
        drone_pos = Drone.transform.position;
        // Debug.Log("Drone_Speed: "+ForwardBackward.ToString()+" "+UpDown.ToString()+" "+Tilt.ToString());
        //(7, 17)(17, 55)(55, 65)
        //(-15, -5)(-5, 55)(55, 65)(65, 125)(125, 135)
        if (random){
            timer_interval += Time.fixedDeltaTime;
            if (timer_interval >= interval){
                // Create an instance of Random
                if(Input.GetKey(KeyCode.W)){
                    System.Random random = new System.Random();
                    int randomSample = random.Next(0, 100);
                    if(randomSample<5){
                        keycode='A';
                    }
                    else if(randomSample>=5 && randomSample<10){
                        keycode='D';
                    }
                }
                timer_interval = 0f;
            }
            if(keycode!='q'){
                timer_duation += Time.fixedDeltaTime;
                if(timer_duation>=duration){
                    timer_duation = 0f;
                    keycode = 'q';
                }
            }
        }
        //Debug.Log(prev_key);

        if (Mobile == false)
        {
            /*  if statements with Input.GetKey(?) is for controlling the drone using keyboard inputs  */ 
            /*
                W: fly forward A: rotate left S: fly back D: rotate right Up: fly up Down: fly down Right: tilt right Left: tilt left
            */
            y_angle = CamRotation.y<360 && CamRotation.y>180 ? (int)CamRotation.y-360:(int)CamRotation.y; 
            //Debug.Log("Velocity: "+localVelocity);
            //Debug.Log("angle: "+y_angle);

            // Add 0729 / if not rotating, decrease torque until 0

            /*if(!Input.GetKey(KeyCode.A) && keycode!='A' && !Input.GetKey(KeyCode.D) && keycode!='D') {
                if(Drone.angularVelocity.y > 0) {
                    //Drone.angularVelocity = new Vector3(0, Drone.angularVelocity.y - Tilt, 0);
                    Drone.angularVelocity = new Vector3(0,0,0);
                } else if(Drone.angularVelocity.y < 0) {
                    //Drone.AddRelativeTorque(0, Tilt, 0);
                    Drone.angularVelocity = new Vector3(0,0,0);
                    //Drone.angularVelocity = new Vector3(0, Drone.angularVelocity.y + Tilt, 0);
                }
            }*/
            


            if (Input.GetKey(KeyCode.W)&&keycode=='q')
            {   
                if(ForwardBackwardSpeed >1200f){
                    ForwardBackwardSpeed =1200f;
                } 
                else{
                    ForwardBackwardSpeed += 1.0f;
                }

                if (Input.GetKey(KeyCode.A)){
                    Drone.AddRelativeTorque(0, -ForwardBackwardSpeed, 0);
                    Drone.AddRelativeForce(0, 0, ForwardBackwardSpeed);
                    CurrCommand = "WA";

                }
                else if (Input.GetKey(KeyCode.D)){
                    Drone.AddRelativeTorque(0, ForwardBackwardSpeed, 0);
                    Drone.AddRelativeForce(0, 0, ForwardBackwardSpeed);
                    CurrCommand = "WD";
                }
                else{
                    Drone.AddRelativeForce(0, 0, ForwardBackwardSpeed);
                    CurrCommand = "W";
                }
                
                //if (RotationSpeed>500f){RotationSpeed -= 1.0f;}
                // if (Input.GetKey(KeyCode.A)){
                //     //RotationSpeed += 0.5f; // 증가 속도
                //     Drone.AddRelativeTorque(0, -ForwardBackwardSpeed, 0); // 왼쪽으로 회전 토크 적용

                //     //Drone.AddRelativeForce(0, 0, ForwardBackwardSpeed);
                // }
                // else if (Input.GetKey(KeyCode.D)){
                //     //RotationSpeed += 0.5f; // 증가 속도
                //     Drone.AddRelativeTorque(0, ForwardBackwardSpeed, 0); // 왼쪽으로 회전 토크 적용

                //     //Drone.AddRelativeForce(0, 0, ForwardBackwardSpeed);
                // }
                // else{
                //     //ForwardBackwardSpeed += 0.5f; // Increase speed
                //     //Debug.Log(forwardForce);
                //     Debug.Log("Velocity: "+localVelocity);
                //     //ForwardBackwardSpeed = Mathf.Clamp(ForwardBackwardSpeed, 0, MaxForwardBackwardSpeed); // Clamp speed to max value
                //     //ForwardBackwardSpeed = 100;
                //     Drone.AddRelativeForce(0, 0, ForwardBackwardSpeed);

                // }
                
            }
            else if (Input.GetKey(KeyCode.S)){
                if(localVelocity.z>0){ Drone.AddRelativeForce(0, 0, -ForwardBackwardSpeed);}
                if (ForwardBackwardSpeed>500f){ForwardBackwardSpeed -= 10.0f;}
                CurrCommand = "S";

            }
            else if (Input.GetKey(KeyCode.A) || keycode=='A')
            {
                //RotationSpeed += 1.0f; // 증가 속도
                //if (ForwardBackwardSpeed>300f){ForwardBackwardSpeed -= 0.01f;}
                // if (ForwardBackwardSpeed<500f){Drone.AddRelativeTorque(0, -RotationSpeed, 0);}
                // else{Drone.AddRelativeTorque(0, -ForwardBackwardSpeed, 0);}
                if (ForwardBackwardSpeed<500f){Drone.AddRelativeTorque(0, -RotationSpeed, 0);}
                else{Drone.AddRelativeTorque(0, -ForwardBackwardSpeed, 0);}
                CurrCommand = "A";
            }
            // d 키를 누르면 오른쪽으로 회전속도 증가
            else if (Input.GetKey(KeyCode.D)|| keycode=='D')
            {
                //RotationSpeed += 1.0f; // 증가 속도
                if (ForwardBackwardSpeed<500f){Drone.AddRelativeTorque(0, RotationSpeed, 0);}
                // if (RotationSpeed<100f){Drone.AddRelativeTorque(0, RotationSpeed, 0);}
                // else{Drone.AddRelativeTorque(0, ForwardBackwardSpeed, 0);}
                else{Drone.AddRelativeTorque(0, ForwardBackwardSpeed, 0);}
                CurrCommand = "D";
            }
            else if(Input.GetKey(KeyCode.Space)){
                float spaceTorqueFactor = 1500f;
                // z 축 회전
                if (DroneRotation.z > 5 && DroneRotation.z <= 180) 
                { Drone.AddRelativeTorque(0, 0, -spaceTorqueFactor); }
                if (DroneRotation.z > 180 && DroneRotation.z <= 355) 
                { Drone.AddRelativeTorque(0, 0, spaceTorqueFactor); }

                // x 축 회전
                if (DroneRotation.x > 5 && DroneRotation.x <= 180) 
                { Drone.AddRelativeTorque(-spaceTorqueFactor, 0, 0); }
                if (DroneRotation.x > 180 && DroneRotation.x <= 355) 
                { Drone.AddRelativeTorque(spaceTorqueFactor, 0, 0); }
                CurrCommand = "SPACE";
            }
            else{
                if (ForwardBackwardSpeed>500f){ForwardBackwardSpeed -= 3.0f;}
                CurrCommand = "N";
            }
    
            if (CurrCommand!=PrevCommand){
                SendData(CurrCommand);
                //Debug.Log(CurrCommand+"/"+(DateTimeOffset.Now.ToUnixTimeMilliseconds()*1e-3).ToString());
                PrevCommand = CurrCommand;
            }


            // else if (Input.GetKey(KeyCode.D))
            // {
            //     RotationSpeed += 1f; // Increase speed
            //     //ForwardBackwardSpeed = Mathf.Clamp(ForwardBackwardSpeed, 0, MaxForwardBackwardSpeed); // Clamp speed to max value
            //     Drone.AddRelativeTorque(0, RotationSpeed, 0);
            // }
            // else if (Input.GetKey(KeyCode.A))
            // {
            //     RotationSpeed += 1f; // Increase speed
            //     //ForwardBackwardSpeed = Mathf.Clamp(ForwardBackwardSpeed, 0, MaxForwardBackwardSpeed); // Clamp speed to max value
            //     Drone.AddRelativeTorque(0, -RotationSpeed, 0);
            // }
            // else
            // {
            //     ForwardBackwardSpeed = 4f; // Reset speed when W is not pressed
            // }
            
            
            // if (Input.GetKey(KeyCode.W)) 
            // { 
            //     if (Input.GetKey(KeyCode.A)) { 
            //         Debug.Log("AAAAAAAAAAAAAA");
            //         Drone.AddRelativeTorque(0, Tilt / -1, 0);
            //         Drone.AddRelativeForce(0, 0, ForwardBackward); }
            //     else if (Input.GetKey(KeyCode.D)){ 
            //         Debug.Log("DDDDDDDDDDDDDD");
            //         Drone.AddRelativeTorque(0, Tilt, 0);
            //         Drone.AddRelativeForce(0, 0, ForwardBackward);}
            //     else {Drone.AddRelativeForce(0, 0, ForwardBackward);}
            // }
            //RotationSpeed = 50f; // 두 키가 눌리지 않으면 회전 속도 초기화
            // if (!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.D)){
            //     if (ForwardBackwardSpeed>500f){ForwardBackwardSpeed -= 3.0f;}
            // }
            
            //if (RotationSpeed>500f){RotationSpeed -= 1.0f;}
            // else
            // {
            //     ForwardBackwardSpeed -= SpeedDecreaseRate * Time.deltaTime;
            //     if (ForwardBackwardSpeed < 0)
            //         ForwardBackwardSpeed = 0;
                
            //     RightRotationSpeed -= RotationSpeedDecreaseRate * Time.deltaTime;
            //     if (RightRotationSpeed < 0)
            //         RightRotationSpeed = 0;

            //     LeftRotationSpeed += RotationSpeedDecreaseRate * Time.deltaTime;
            //     if (LeftRotationSpeed > 0)
            //         LeftRotationSpeed = 0;
            // }
            // else{
            //     ForwardBackwardSpeed = 0f;
            //     RightRotationSpeed = 0f;
            //     LeftRotationSpeed = 0f;
            // }
            // else if (Input.GetKey(KeyCode.A)) { Drone.AddRelativeTorque(0, Tilt / -1, 0); }//tilt drone left
            // else if (Input.GetKey(KeyCode.D)) { Drone.AddRelativeTorque(0, Tilt, 0); }//tilt drone right
            
            
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

            // Apply stabilization force to prevent flipping
                // 회전 스태빌라이즈이션
            
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
