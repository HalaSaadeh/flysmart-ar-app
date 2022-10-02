using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class User_Input : MonoBehaviour
{
    #region Variables
    [System.NonSerialized] public Vector2 cyclic; //roll and pitch
    [System.NonSerialized] public Vector2 throttle; //thrust and yaw
    [System.NonSerialized] public BluetoothModule bluetooth;

    int formatted_roll;
    int formatted_pitch;
    int formatted_yaw;

    public Drone drone;
    public int resolution;
    public Variables variables;
    #endregion

    
    // Start is called before the first frame update
    void Start()
    {
        bluetooth = FindObjectOfType<BluetoothModule>();
        variables = FindObjectOfType<Variables>();
        resolution = variables.resolution;

        cyclic = new Vector2();
        throttle = new Vector2();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        formatted_yaw = ((int)bluetooth.yaw) - (((int)bluetooth.yaw) % resolution);
        throttle.x = formatted_yaw / drone.minMaxYaw;

        formatted_pitch = ((int)bluetooth.pitch) - (((int)bluetooth.pitch) % resolution);
        cyclic.y = formatted_pitch / drone.minMaxPitch;

        formatted_roll = ((int)bluetooth.roll) - (((int)bluetooth.roll) % resolution);
        cyclic.x = bluetooth.roll / drone.minMaxRoll;


        if (throttle.x > 1.0f) {
            throttle.x = 1.0f;
        }
        if (throttle.x < -1.0f)
        {
            throttle.x = -1.0f;
        }
        if (cyclic.y > 1.0f)
        {
            cyclic.y = 1.0f;
        }
        if (cyclic.y < -1.0f)
        {
            cyclic.y = -1.0f;
        }
        if (cyclic.x > 1.0f)
        {
            cyclic.x = 1.0f;
        }
        if (cyclic.x < -1.0f)
        {
            cyclic.x = -1.0f;
        }





        /*
        //For keyboard input
        #region Get roll input
        
            if (Input.GetKey(KeyCode.LeftArrow)) {
                cyclic.x = -1.0f;
            }
            else if (Input.GetKey(KeyCode.RightArrow)) {
                cyclic.x = 1.0f;
            }
            else {
                cyclic.x = 0.0f;
            }
        #endregion
        #region Get pitch input
            if (Input.GetKey("up"))
            {
                cyclic.y = 1.0f;
            }
            else if (Input.GetKey("down"))
            {
                cyclic.y = -1.0f;
            }
            else
            {
                cyclic.y = 0.0f;
            }
        #endregion
        #region Get yaw input
            if (Input.GetKey("a"))
            {
                throttle.x = -1.0f;
            }
            else if (Input.GetKey("d"))
            {
                throttle.x = 1.0f;
            }
            else
            {
                throttle.x = 0.0f;
            }
        #endregion
        #region Get thrust input
        if (Input.GetKey("s"))
        {
            throttle.y = -1.0f;
        }
        else if (Input.GetKey("w"))
        {
            throttle.y = 1.0f;
        }
        else
        {
            throttle.y = 0.0f;
        }
        #endregion
        */

        //For Joysticks

    }

    #region Input Methods
    /*
    private void OnCyclic(InputValue value)
    {
        cyclic = value.Get<Vector2>();
    }
    */
    private void OnThrottle(InputValue value)
    {
        throttle = value.Get<Vector2>();
    }
    /*
    private void OnPedals(InputValue value)
    {
        pedals = value.Get<Vector2>().x;
    }
    private void OnThrottle(InputValue value)
    {
        throttle = value.Get<Vector2>().y;
    }
    */
    #endregion

}
