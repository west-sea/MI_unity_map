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


public class RightWheel : MonoBehaviour
{

    public Rigidbody wheel;
    public Vector3 prev_pos;
    public float ForwardBackwardSpeed = 200f; // Original Value: 4
    private float RotationSpeed = 100f;
    void Start()
    {
        prev_pos = wheel.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //CamRotation = Cam.transform.localEulerAngles;
        Vector3 localVelocity = transform.InverseTransformDirection(wheel.velocity);
        Vector3 localAngularVelocity = wheel.transform.InverseTransformDirection(wheel.angularVelocity);
        if (Input.GetKey(KeyCode.W))
        {
                //ForwardBackwardSpeed += 0.5f;
                wheel.AddRelativeForce(0, 0, ForwardBackwardSpeed);
                // if (Input.GetKey(KeyCode.A)){
                //     RotationSpeed += 0.5f; // 증가 속도
                //     Drone.AddRelativeTorque(0, -RotationSpeed, 0); // 왼쪽으로 회전 토크 적용
                //     ForwardBackwardSpeed += 0.5f;

                //     Drone.AddRelativeForce(0, 0, ForwardBackwardSpeed);
                // }
                // else if (Input.GetKey(KeyCode.D)){
                //     RotationSpeed += 0.5f; // 증가 속도
                //     Drone.AddRelativeTorque(0, RotationSpeed, 0); // 왼쪽으로 회전 토크 적용
                //     ForwardBackwardSpeed += 0.5f;

                //     Drone.AddRelativeForce(0, 0, ForwardBackwardSpeed);
                // }
                // else{
                //     //ForwardBackwardSpeed += 0.5f; // Increase speed
                //     //Debug.Log(forwardForce);
                //     Debug.Log("Velocity: "+localVelocity);
                //     //ForwardBackwardSpeed = Mathf.Clamp(ForwardBackwardSpeed, 0, MaxForwardBackwardSpeed); // Clamp speed to max value
                //     //ForwardBackwardSpeed = 100;
                //     ForwardBackwardSpeed += 0.5f;
                //     if(ForwardBackwardSpeed >800f){
                //         ForwardBackwardSpeed =800f;
                //         Drone.AddRelativeForce(0, 0, ForwardBackwardSpeed);

                //     }
                //     else{
                //         Drone.AddRelativeForce(0, 0, ForwardBackwardSpeed);

                //     }
                
            }
            else if (Input.GetKey(KeyCode.A))
            {
                //ForwardBackwardSpeed += 0.5f;
                wheel.AddRelativeForce(0, 0, ForwardBackwardSpeed);
            }
            // d 키를 누르면 오른쪽으로 회전속도 증가
            else if (Input.GetKey(KeyCode.D))
            {
                //ForwardBackwardSpeed += 0.5f;
                wheel.AddRelativeForce(0, 0, ForwardBackwardSpeed);
            }
    
    }
}
