using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using UnityEngine;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class WheelchairRecord : MonoBehaviour
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

    public static string[] GetFileNames(string filter)
    {
        string[] files = Directory.GetFiles(@"C:\Users\Chaeeun\Documents\2024_spring\Drone_Simulator_Keyboard\path\", "*.txt"); // First parameter: Windows: @"C:\Drone_path\"   Android: Application.persistentDataPath
        for (int i = 0; i < files.Length; i++)
            files[i] = Path.GetFileName(files[i]);
        return files;
    }
    
    public static string[] GetCollisionFileNames(string filter)
    {
        string[] files = Directory.GetFiles(@"C:\Users\Chaeeun\Documents\2024_spring\Drone_Simulator_Keyboard\collision\", "*.txt"); // First parameter: Windows: @"C:\Drone_path\"   Android: Application.persistentDataPath
        for (int i = 0; i < files.Length; i++)
            files[i] = Path.GetFileName(files[i]);
        return files;
    }

    public static string[] GetCommandFileNames(string filter)
    {
        string[] files = Directory.GetFiles(@"C:\Users\Chaeeun\Documents\2024_spring\Drone_Simulator_Keyboard\command\", "*.txt"); // First parameter: Windows: @"C:\Drone_path\"   Android: Application.persistentDataPath
        for (int i = 0; i < files.Length; i++)
            files[i] = Path.GetFileName(files[i]);
        return files;
    }

    public GameObject wheelchair;
    public Rigidbody wheelchair_rb;
    public Vector3 wheelchair_pos;
    public Vector3 prev_wheelchair_pos;
    private Vector3 wheelchair_rotation;
    private Vector3 prev_wheelchair_rotation;
    public string pathfileName;
    public string collisionFileName;
    public string commandFileName;
    public string path;
    public string collision_path;
    public string command_path;
    private char ControlType; // 0: original, 1: random

    public test ControlScript;
    public string SubjectName;

    private string RecordCommand = "N";
    private string PrevCommand = "N";
    private char RecordRandom;
    public int boundary_collision = 0;
    public int road_collision = 0;
    public int fence_collision = 0;
    bool run = true;

    private float logInterval = 0.02f;
    private float nextLogTime = 0;
    private string zone = "None";
    void Start()
    {   
        m_fnReceiveHandler = new AsyncCallback(handleDataReceive);
        // Change the IP here accordingly for socket connection. Enter IP (ipconfig in cmd of this computer)
        remoteEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 14000); //127.0.0.1   143.248.137.106 # port = 14000 192.168.0.5
        //ControlScript = wheelchair.GetComponent<test>();
        if (ControlScript.random) {ControlType='r';}
        else {ControlType='n';}

        pathfileName = String.Format("[{0}]wheelchair_path_{1}_",SubjectName,ControlType);
        collisionFileName = String.Format("[{0}]collision_num_{1}_",SubjectName,ControlType);
        //commandFileName = String.Format("[{0}]command_num_{1}",SubjectName,ControlType);

        int i = 0;
        string[] pathFiles = Directory.GetFiles(@"C:\Users\Chaeeun\Documents\2024_spring\Drone_Simulator_Keyboard\path\", "*.txt"); // First parameter: Windows: @"C:\Drone_path\"   Android: Application.persistentDataPath
        string[] collisionFiles = Directory.GetFiles(@"C:\Users\Chaeeun\Documents\2024_spring\Drone_Simulator_Keyboard\collision\", "*.txt");
        //string[] commandFiles = Directory.GetFiles(@"C:\Users\Chaeeun\Documents\2024_spring\Drone_Simulator_Keyboard\command\", "*.txt");
        for (i = 0; i < pathFiles.Length; i++)
        {
            pathFiles[i] = Path.GetFileName(pathFiles[i]);
        }

        for (i = 0; i < collisionFiles.Length; i++)
        {
            collisionFiles[i] = Path.GetFileName(collisionFiles[i]);
        }

        // for (i = 0; i < commandFiles.Length; i++)
        // {
        //     commandFiles[i] = Path.GetFileName(commandFiles[i]);
        // }

        for (i = 0; ; i++)
        {
            if (!pathFiles.Contains(pathfileName + i.ToString() + ".txt"))
            {
                pathfileName = pathfileName + i.ToString();
                break;
            }
        }
        pathfileName = "C:\\Users\\Chaeeun\\Documents\\2024_spring\\Drone_Simulator_Keyboard\\path\\" + pathfileName; // Windows: "C:\\Drone_path\\" + fileName;  Android: Path.Combine(Application.persistentDataPath, fileName); // search drone_path in Internal Storage of quest 2 later on
        path = @pathfileName;


        for (i = 0; ; i++)
        {
            if (!collisionFiles.Contains(collisionFileName + i.ToString() + ".txt"))
            {
                collisionFileName = collisionFileName + i.ToString();
                break;
            }
        }
        collisionFileName = "C:\\Users\\Chaeeun\\Documents\\2024_spring\\Drone_Simulator_Keyboard\\collision\\" + collisionFileName; // Windows: "C:\\Drone_path\\" + fileName;  Android: Path.Combine(Application.persistentDataPath, fileName); // search drone_path in Internal Storage of quest 2 later on
        collision_path = @collisionFileName;

        // for (i = 0; ; i++)
        // {
        //     if (!commandFiles.Contains(commandFileName + i.ToString() + ".txt"))
        //     {
        //         commandFileName = commandFileName + i.ToString();
        //         break;
        //     }
        // }
        // commandFileName = "C:\\Users\\Chaeeun\\Documents\\2024_spring\\Drone_Simulator_Keyboard\\command\\" + commandFileName; // Windows: "C:\\Drone_path\\" + fileName;  Android: Path.Combine(Application.persistentDataPath, fileName); // search drone_path in Internal Storage of quest 2 later on
        // command_path = @commandFileName;

        wheelchair = gameObject;

        wheelchair_pos = wheelchair.transform.position;
        wheelchair_rotation = wheelchair_rb.transform.localEulerAngles;
        //StartCoroutine(getSocket());  
        
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
            if(RecordCommand!=PrevCommand)
            {
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(RecordCommand);
                //Debug.Log("Before send (time): "+DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());
                client.Send(byData);
                //Debug.Log("After send (time): "+DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());

                socket_received=false;

                client.BeginReceive(ao.Buffer, 0, ao.Buffer.Length, SocketFlags.None, m_fnReceiveHandler, ao);
                PrevCommand=RecordCommand;
            }
            yield return new WaitForSecondsRealtime((float)0.1);
        
        // byte[] buffer = new byte[1024];
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (run){
            prev_wheelchair_pos = wheelchair_pos;
            wheelchair_pos = wheelchair.transform.position;
            prev_wheelchair_rotation = wheelchair_rotation;
            wheelchair_rotation = wheelchair_rb.transform.localEulerAngles;
            RecordCommand=ControlScript.CurrCommand;
            zone=ControlScript.zone;

            if (ControlScript.keycode!='q'){RecordRandom='r';}
            else {RecordRandom='n';}

            if (Time.time >= nextLogTime){
                nextLogTime = Time.time + logInterval;
                LogPosition();
            }

            //StartCoroutine(Pos_check());
        }
        
    }

    void LogPosition()
    {
        using (StreamWriter writer = new StreamWriter(pathfileName + ".txt", true))
        {
            writer.WriteLine(wheelchair_pos + "/" + wheelchair_rotation + "/" + zone + "/"+RecordCommand + "/" + RecordRandom + "/" + (DateTimeOffset.Now.ToUnixTimeMilliseconds()*1e-3).ToString());
            //Debug.Log("wheelchair_POS: " + wheelchair_pos);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        string collision_type="N";
        int collision_num=0;
        string CurrCommand="Q";
        string CurrRandom = "n";

        if(collision.collider.tag == "Boundary")
        {
            boundary_collision+=1;
            Debug.Log("Boundary Collision: " + boundary_collision.ToString());
            collision_type = "B";
            collision_num = boundary_collision;
        }
        else if(collision.collider.tag == "Road")
        {
            road_collision +=1;
            Debug.Log("Road Collision: " + road_collision.ToString());
            collision_type = "R";
            collision_num = road_collision;
        }
        else if(collision.collider.tag == "fence")
        {
            fence_collision +=1;
            Debug.Log("fence Collision: " + fence_collision.ToString());
            collision_type = "F";
            collision_num = fence_collision;
        }

        
        if (ControlScript == null){
            Debug.Log("NULLLL");

        }

        
        CurrCommand = ControlScript.CurrCommand;
        if (ControlScript.keycode!='q'){CurrRandom="r";}
        else {CurrRandom="n";}

        using (StreamWriter writer = new StreamWriter(collisionFileName + ".txt", true))
        { // x < -40 z > 16
            //run = false;
            // drone position, time, ring collision (1 if collided), boundary collision (1 if collided), ring pass (1,2,3,4 for each ring pass)
            //writer.WriteLine(string.Format("{0}\t{1}\t{2}", drone_pos.ToString(), (System.DateTime.UtcNow - epochStart).TotalSeconds,boundary_collision));//System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond)); //(int)(System.DateTime.UtcNow - epochStart).TotalSeconds)); // write drone position and UTC time.
            if (collision_type!="N"){
                writer.WriteLine(wheelchair_pos + "/" + collision_type + "/" + collision_num + "/" + CurrCommand + "/" + CurrRandom + "," + (DateTimeOffset.Now.ToUnixTimeMilliseconds()*1e-3).ToString()); // without time
            }
            //Debug.Log("wheelchair_POS: "+wheelchair_pos+" Previous: "+prev_wheelchair_pos);
            // Reset values
            //boundary_collision = 0;
            //yield return new WaitForSecondsRealtime((float)1.0); 
            //run = true;

        }

    }

    IEnumerator Pos_check()
    {   

        using (StreamWriter writer = new StreamWriter(pathfileName + ".txt", true))
        { // x < -40 z > 16
            run = false;
            // drone position, time, ring collision (1 if collided), boundary collision (1 if collided), ring pass (1,2,3,4 for each ring pass)
            //writer.WriteLine(string.Format("{0}\t{1}\t{2}", drone_pos.ToString(), (System.DateTime.UtcNow - epochStart).TotalSeconds,boundary_collision));//System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond)); //(int)(System.DateTime.UtcNow - epochStart).TotalSeconds)); // write drone position and UTC time.

            writer.WriteLine(wheelchair_pos + "/"+wheelchair_rotation+"/"+ RecordCommand+"/"+ RecordRandom + "/"+(DateTimeOffset.Now.ToUnixTimeMilliseconds()*1e-3).ToString()); // without time
            
            
            //Debug.Log("wheelchair_POS: "+wheelchair_pos);
            // Reset values
            //boundary_collision = 0;
            yield return new WaitForSecondsRealtime((float)0.02); 
            run = true;

        }
    }
}
