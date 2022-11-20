using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    public bool click_detected;
    bool already_closed;

    public bool is_scroll_gesture;
    public string current_state;
    public string past_state;

    public float sum;
    public float height_sensitivity;

    public Text current_hand_state;
    public Text another_one;
    
    #endregion

    
    // Start is called before the first frame update
    void Start()
    {
        bluetooth = FindObjectOfType<BluetoothModule>();
        variables = FindObjectOfType<Variables>();
        resolution = variables.resolution;
        is_scroll_gesture = variables.gesture;

        already_closed = false;
        click_detected = false;

        cyclic = new Vector2();
        throttle = new Vector2();

        current_state = "Steady";
        past_state = "Steady";

        sum = 0.0f;
        height_sensitivity = 10.0f;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        try
        {
            formatted_yaw = ((int)bluetooth.yaw) - (((int)bluetooth.yaw) % resolution);
            throttle.x = formatted_yaw / drone.minMaxYaw;

            formatted_pitch = ((int)bluetooth.pitch) - (((int)bluetooth.pitch) % resolution);
            cyclic.y = - formatted_pitch / drone.minMaxPitch;

            formatted_roll = ((int)bluetooth.roll) - (((int)bluetooth.roll) % resolution);
            cyclic.x = bluetooth.roll / drone.minMaxRoll;


            if (throttle.x > 1.0f)
            {
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

            //Thrust Mapping
            //First check mode
            if (is_scroll_gesture)
            {
                current_state = bluetooth.droneStatusText;

                if(bluetooth.currentGesture != "Closed Grip")
                {
                    if (current_state == "Steady")
                    {
                        throttle.y = Mathf.Lerp(throttle.y, 0.0f, height_sensitivity * Time.deltaTime);
                    }
                    if (current_state == "UpFast")
                    {
                        throttle.y = Mathf.Lerp(throttle.y, 0.5f, height_sensitivity * Time.deltaTime);
                    }
                    if (current_state == "UpSlow")
                    {
                        throttle.y = Mathf.Lerp(throttle.y, 0.5f, height_sensitivity * Time.deltaTime);
                    }
                    if (current_state == "DownFast")
                    {
                        throttle.y = Mathf.Lerp(throttle.y, -0.5f, height_sensitivity * Time.deltaTime);
                    }
                    if (current_state == "DownSlow")
                    {
                        throttle.y = Mathf.Lerp(throttle.y, -0.5f, height_sensitivity * Time.deltaTime);
                    }
                }
                else
                {
                    throttle.y = Mathf.Lerp(throttle.y, 0.0f, height_sensitivity * Time.deltaTime);
                }

                
            }
            else {
                current_state = bluetooth.droneStatusText;

                if (current_state == "UpFast" && past_state != "UpFast") {
                    sum += 1.0f;
                }
                if (current_state == "UpSlow" && past_state != "UpSlow") {
                    sum += 0.5f;
                }
                if (current_state == "DownFast" && past_state != "DownFast")
                {
                    sum -= 1.0f;
                }
                if (current_state == "DownSlow" && past_state != "DownSlow")
                {
                    sum -= 0.5f;
                }

                if (sum > 1.0f)
                {
                    sum = 1.0f;
                }
                if (sum < -1.0f)
                {
                    sum = -1.0f;
                }

                throttle.y = Mathf.Lerp(throttle.y, sum, height_sensitivity * Time.deltaTime);

                
                past_state = current_state;

            }
        }
        catch{ 
        
        }

        //Click mapping
        try
        {
            if (current_hand_state != null) {
                current_hand_state.text = bluetooth.currentGesture;
            }
            if(another_one != null)
            {
                another_one.text = bluetooth.currentGesture;
            }
            
            

            if (bluetooth.currentGesture == "Closed Grip" && !already_closed)
            {
                already_closed = true;
            }
            else
            {
                if (already_closed)
                {
                    current_hand_state.text = bluetooth.currentGesture;
                    click_detected = true;
                    already_closed = false;
                }
                else
                {
                    click_detected = false;
                }
            }
        }
        catch
        {

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
