using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
{
    // Start is called before the first frame update
    public WheelCollider[] wheel_col;
    public Transform[] wheels;
    float torque=100;
    float angle=45;
    
    // void Start()
    // {
        
    // }

    // Update is called once per frame
    void Update()
    {
        for(int i=0; i<wheel_col.Length;i++)
        {
            //wheel_col[i].motorTorque=Input.GetAxis("Vertical")*torque;
            var pos = transform.position;
            var rot = transform.rotation;
            wheel_col[i].GetWorldPose(out pos, out rot);
            //wheels[i].position=pos;
            //wheels[i].rotation=rot;
        }
    }
}
