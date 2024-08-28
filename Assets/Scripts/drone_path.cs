using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class drone_path : MonoBehaviour
{

    public GameObject drone;
    public Vector3 drone_pos;
    public Vector3 prev_drone_pos;
    public Camera Cam;
    public string pressed_key;
    bool run = true;
    public System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
    public int boundary_collision = 0;
    public Vector3 CamRotation;

    public static string[] GetFileNames(string filter)
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.txt"); // First parameter: Windows: @"C:\Drone_path\"   Android: Application.persistentDataPath
        for (int i = 0; i < files.Length; i++)
            files[i] = Path.GetFileName(files[i]);
        return files;
    }
    public string fileName = "drone_path_4class_";
    public string path;

    // Use this for initialization
    void Start()
    {
        pressed_key="n";
        int i = 0;
        string[] savedFiles = Directory.GetFiles(Application.persistentDataPath, "*.txt"); // First parameter: Windows: @"C:\Drone_path\"   Android: Application.persistentDataPath
        for (i = 0; i < savedFiles.Length; i++)
        {
            savedFiles[i] = Path.GetFileName(savedFiles[i]);
        }

        for (i = 0; ; i++)
        {
            if (!savedFiles.Contains(fileName + i.ToString() + ".txt"))
            {
                fileName = fileName + i.ToString();
                break;
            }
        }
        fileName = Path.Combine(Application.persistentDataPath, fileName); // Windows: "C:\\Drone_path\\" + fileName;  Android: Path.Combine(Application.persistentDataPath, fileName); // search drone_path in Internal Storage of quest 2 later on
        path = @fileName;

        drone = gameObject;

        drone_pos = drone.transform.position;

        // Might delete later

    }

    // Update is called once per frame
    void Update()
    {
        if (run == true)
        {
            prev_drone_pos = drone_pos;
            drone_pos = drone.transform.position;
            if (Input.GetKey(KeyCode.W)) { pressed_key="w"; }
            else if (Input.GetKey(KeyCode.A)) { pressed_key="a"; }//tilt drone left
            else if (Input.GetKey(KeyCode.D)) {pressed_key="d"; }//tilt drone right
            else{pressed_key="n";}
            StartCoroutine(Pos_check());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Boundary")
        {
            boundary_collision++;
            Debug.Log("Boundary Collision: " + boundary_collision.ToString());
        }
    }

    IEnumerator Pos_check()
    {
        using (StreamWriter writer = new StreamWriter(fileName + ".txt", true))
        {
            if(drone_pos.x > 197 && drone_pos.x < 415)
            {
                run = false;
                // drone position, time, ring collision (1 if collided), boundary collision (1 if collided), ring pass (1,2,3,4 for each ring pass)
                CamRotation = Cam.transform.localEulerAngles;
                // Debug.Log("CameraAngle: "+ CamRotation.x.ToString() + "  " + CamRotation.y.ToString() + "  " + CamRotation.z.ToString());
                writer.WriteLine(string.Format("{0}\t{1}\t{2}\t0\t{3}\t{4}", drone_pos.ToString(), (System.DateTime.UtcNow - epochStart).TotalSeconds,boundary_collision,pressed_key,CamRotation.ToString()));//System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond)); //(int)(System.DateTime.UtcNow - epochStart).TotalSeconds)); // write drone position and UTC time.
                //writer.WriteLine(drone_pos); // without time
                Debug.Log("Drone_POS: "+drone_pos+" Previous: "+prev_drone_pos);
                // Reset values
                boundary_collision = 0;
                yield return new WaitForSecondsRealtime((float)0.5);
                run = true;
            }
        }
    }
}