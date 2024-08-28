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

public class HandleControl : MonoBehaviour
{
    public Rigidbody wheelchair; // Rigidbody of the wheelchair
    public float forwardSpeed = 300f; // Base forward speed
    public float rotationSpeed = 500f; // Rotation speed when turning
    public float maxSpeed = 1000f; // Maximum speed for the wheelchair
    public float acceleration = 2f; // Acceleration rate per second

    public float speedScale = 50f;

    void FixedUpdate()
    {   
        Vector3 wheelchairRotation = wheelchair.transform.localEulerAngles;
        Vector3 localVelocity = transform.InverseTransformDirection(wheelchair.velocity);
        float torqueFactor = 1000f; // 회전 스태빌라이즈이션 토크 강도
        Debug.Log(localVelocity);
        // z 축 회전
        if (wheelchairRotation.z > 2 && wheelchairRotation.z <= 180) 
        { wheelchair.AddRelativeTorque(0, 0, -torqueFactor); }
        if (wheelchairRotation.z > 180 && wheelchairRotation.z <= 358) 
        { wheelchair.AddRelativeTorque(0, 0, torqueFactor); }

        // x 축 회전
        if (wheelchairRotation.x > 2 && wheelchairRotation.x <= 180) 
        { wheelchair.AddRelativeTorque(-torqueFactor, 0, 0); }
        if (wheelchairRotation.x > 180 && wheelchairRotation.x <= 358) 
        { wheelchair.AddRelativeTorque(torqueFactor, 0, 0); }

        HandleRotation(); // Handle rotation based on A and D keys
        HandleForwardMovement(); // Handle forward movement based on W key
        HandleSpeedIncrease(); // Handle speed increase with Space bar
        
        // Handle speed increase with Space bar
        // if (Input.GetKey(KeyCode.Space))
        // {
        //     forwardSpeed += acceleration * Time.deltaTime;
        //     forwardSpeed = Mathf.Clamp(forwardSpeed, 0f, maxSpeed);
        // }

        // // Handle rotation based on A and D keys
        // if (Input.GetKey(KeyCode.A))
        // {
        //     // Rotate left
        //     wheelchair.AddRelativeTorque(0f, -forwardSpeed * Time.deltaTime*speedScale, 0f);
        // }
        // else if (Input.GetKey(KeyCode.D))
        // {
        //     // Rotate right
        //     wheelchair.AddRelativeTorque(0f, forwardSpeed * Time.deltaTime*speedScale, 0f);
        // }

        // // Move forward based on W key
        // if (Input.GetKey(KeyCode.W))
        // {
        //     // Move forward
        //     Debug.Log(forwardSpeed * Time.deltaTime*speedScale);
        //     wheelchair.AddRelativeForce(0f, 0f, forwardSpeed * Time.deltaTime *speedScale);
        // }
    }

    void HandleRotation()
    {
        //float rotationInput = 0f;
        float rotationInput = Input.GetAxis("Horizontal");
        // Handle rotation based on A and D keys
        // if (Input.GetKey(KeyCode.A))
        // {
        //     //Debug.Log("GOT AAAAAAAAAAAAA");
        //     rotationInput -= 1f; // Left rotation input
        // }
        // if (Input.GetKey(KeyCode.D))
        // {
        //     rotationInput += 1f; // Right rotation input
        // }

        // Apply rotation torque

        wheelchair.AddRelativeTorque(0f, rotationInput * forwardSpeed * Time.deltaTime* speedScale, 0f);
    }

    void HandleForwardMovement()
    {
        float forwardInput = 0f;

        // Handle forward movement based on W key
        if (Input.GetKey(KeyCode.W))
        {
            forwardInput = 1f; // Forward movement input
        }

        // Apply forward force
        wheelchair.AddRelativeForce(0f, 0f, forwardInput * forwardSpeed * Time.deltaTime * speedScale);
    }

    void HandleSpeedIncrease()
    {
        // Handle speed increase with Space bar
        if (Input.GetKey(KeyCode.Space))
        {
            forwardSpeed += acceleration * Time.deltaTime;
            forwardSpeed = Mathf.Clamp(forwardSpeed, 0f, maxSpeed);
        }
    }
}
