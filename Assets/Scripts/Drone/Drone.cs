using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Drone : MonoBehaviour
{
    #region enums
    public enum DroneState
    {
        DRONE_STATE_IDLE, //Drone is turned off
        DRONE_STATE_TURNEDON, //Drone just turned on
        DRONE_STATE_TAKINGOFF, //Drone Taking off
        DRONE_STATE_HOVERING, //Drone Hovering without moving
        DRONE_STATE_FLYING, //Drone flying and moving
        DRONE_STATE_LANDING //Drone landing
    }

    #endregion

    #region Variables
        [Header("Game Objects")]
            public User_Input user_input; //Game object getting and scaling user input
            public Variables variables;

    #region Pitch Roll Yaw
    [System.NonSerialized] public Vector3 orientation_target; //target orientation vector
    [System.NonSerialized] public Quaternion rot; //Drone rotation vector
    #region Pitch Parameters
    [System.NonSerialized] public bool isPitchEnabled; //is pitch enabled
    [System.NonSerialized] public bool isPitchLimited; //Pitch has a maximum value
    [System.NonSerialized] public bool resetPitch;
    [System.NonSerialized] public float minMaxPitch; //Max angle if pitch has maximum value
    [System.NonSerialized] public float pitch_target; // target to be set by the user
    [System.NonSerialized] public float pitch_power;
    #endregion

    #region Roll Parameters
    [System.NonSerialized] public bool isRollEnabled; //is roll enabled
    [System.NonSerialized] public bool isRollLimited; //Roll has a maximum value
    [System.NonSerialized] public bool resetRoll;
    [System.NonSerialized] public float minMaxRoll; //Max angle if Roll has maximum value
    [System.NonSerialized] public float roll_target; // target to be set by the user
    [System.NonSerialized] public float roll_power;
    #endregion

    #region Yaw Parameters
    [System.NonSerialized] public bool isYawEnabled; // is yaw enbled
    [System.NonSerialized] public bool isYawLimited; // is yaw limited to an angle 
    [System.NonSerialized] public bool resetYaw; //return yaw to 0
    [System.NonSerialized] public float minMaxYaw;
    [System.NonSerialized] public float yaw_target;
    [System.NonSerialized] public float yaw_power; //yaw power (speed of rotating)
    #endregion
    #endregion
    [System.NonSerialized] public float hovering_force_input; //force allowing the drone to maintain height
    [System.NonSerialized] public bool maintain_height; //controller to maintain height
    [System.NonSerialized] public List<Engine> engines; //list of engines user
    [System.NonSerialized] public DroneState state; //Drone state
    [System.NonSerialized] public Rigidbody rb; //drone's rigid body
    [System.NonSerialized] public float thrust_input; //userinput transformed
    [System.NonSerialized] public float drone_push; //percentage of hovering_force when drone is coming down
    #endregion
    
    #region Main Methods
    // Start is called before the first frame update
    void Awake()
    {
        variables = FindObjectOfType<Variables>();
        //Initialize parameters
        orientation_target = new Vector3(0.0f, transform.eulerAngles.y, 0.0f);
        isPitchEnabled = variables.isPitchEnabled;
        isPitchLimited = variables.isPitchLimited;
        resetPitch = true;
        minMaxPitch = variables.minMaxPitch; //Max angle if pitch has maximum value
        pitch_target = 0.0f; // target to be set by the user
        pitch_power = variables.pitch_power;
    

    
        isRollEnabled = variables.isRollEnabled; //is roll enabled
        isRollLimited = variables.isRollLimited; //Roll has a maximum value
        resetRoll = true;
        minMaxRoll = variables.minMaxRoll; //Max angle if Roll has maximum value
        roll_target = 0.0f; // target to be set by the user
        roll_power = variables.roll_power;

        isYawEnabled = variables.isYawEnabled; // is yaw enbled
        isYawLimited = variables.isYawLimited; // is yaw limited to an angle 
        resetYaw = false; //return yaw to 0
        minMaxYaw = variables.minMaxYaw;
        yaw_target = transform.eulerAngles.y; //original yaw
        yaw_power = variables.yaw_power; //yaw power (speed of rotating)
    

        hovering_force_input = 0.0f; //force allowing the drone to maintain height
        maintain_height = variables.maintain_height; //controller to maintain height
        engines = new List<Engine>(); //list of engines user

        drone_push = 0.4f;
        
        engines = GetComponentsInChildren<Engine>().ToList<Engine>();//Get list of 4 engines
        rb = GetComponent<Rigidbody>(); //initialize rigid body
        float hovering_force = rb.mass * Physics.gravity.magnitude / engines.Count; //calculate hovering force per engine
        hovering_force_input = (hovering_force / engines[0].max_thrust);
        //For debugging
        state = DroneState.DRONE_STATE_TURNEDON;   
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Debug.Log(state);
        //Debug.Log(user_input.throttle.y);
        switch (state)
        {
            case DroneState.DRONE_STATE_IDLE: //If it is not moving
                //Check if any command is given from the user
                break;
            case DroneState.DRONE_STATE_TURNEDON: //Turned on without doing anything
                if (user_input.throttle.y < 0.0f)
                {
                    thrust_input = -user_input.throttle.y * hovering_force_input;
                }
                else {
                    thrust_input = user_input.throttle.y / engines.Count;
                }

                foreach (Engine engine in engines)
                {
                    engine.setEngine(thrust_input);
                }
                if (thrust_input > hovering_force_input) {
                    state = DroneState.DRONE_STATE_FLYING;
                }

                break;
            case DroneState.DRONE_STATE_TAKINGOFF: //If it is taking off
                break;
            case DroneState.DRONE_STATE_HOVERING: //If it is taking off
                //If inputs are not 0 then it is flying
                bool check_hover = (((user_input.cyclic.x == 0.0f) && (user_input.cyclic.y == 0.0f)
                    && (user_input.throttle.x == 0.0f) && (user_input.throttle.y == 0.0f)) && maintain_height) 
                    || (!maintain_height && ((user_input.throttle.y == -1.0f) || (user_input.throttle.y == hovering_force_input*engines.Count)) 
                    && ((user_input.cyclic.x == 0.0f) && (user_input.cyclic.y == 0.0f) && (user_input.throttle.x == 0.0f)));
                //Debug.Log(check_hover);
                if (!check_hover) {
                    state = DroneState.DRONE_STATE_FLYING;
                    break;
                }

                if (resetPitch) { //Drone would hover correctly if this is true
                    pitch_target = 0.0f;
                }
                if (resetRoll)
                { //Drone would hover correctly if this is true
                    roll_target = 0.0f;
                }
                if (resetYaw)
                { //Drone would hover correctly if this is true
                    yaw_target = 0.0f;
                }
                orientation_target.z = Mathf.Lerp(orientation_target.z, pitch_target, pitch_power * Time.deltaTime);//add pitch to orientation target
                orientation_target.x = Mathf.Lerp(orientation_target.x, roll_target, roll_power * Time.deltaTime);//add roll to orientation target
                orientation_target.y = Mathf.Lerp(orientation_target.y, yaw_target, yaw_power * Time.deltaTime);//add roll to orientation target
                rot = Quaternion.Euler(orientation_target.x, orientation_target.y, orientation_target.z);//get the rotation vector
                rb.MoveRotation(rot); //rotate drone
                
                foreach (Engine engine in engines)
                {
                    
                    engine.setEngine(hovering_force_input);//allow the drone to hover
                    
                }
                variables.cube_drop = true;
                variables.timer_active = true;
                break;
            case DroneState.DRONE_STATE_FLYING: //If it is taking off
                // if all inputs are 0, then it is hovering
                bool check_flying = !((((user_input.cyclic.x == 0.0f) && (user_input.cyclic.y == 0.0f)
                    && (user_input.throttle.x == 0.0f) && (user_input.throttle.y == 0.0f)) && maintain_height)
                    || (!maintain_height && ((user_input.throttle.y == -1.0f) || (user_input.throttle.y == hovering_force_input * engines.Count))
                    && ((user_input.cyclic.x == 0.0f) && (user_input.cyclic.y == 0.0f) && (user_input.throttle.x == 0.0f))));
                //Debug.Log(check_flying);//Add later on if not maintain height and input is -1
                if (!check_flying) {
                    state = DroneState.DRONE_STATE_HOVERING;
                    break;
                }

                //First get the roll (x), pitch (z) and yaw(y) target from the user input depending if angle limit is activated or not
                if (isPitchEnabled) { //If user is allowed to change pitch
                    if (isPitchLimited)
                    { //if user want to continoulsy rotate or not
                        pitch_target = user_input.cyclic.y * minMaxPitch; //rotates only to target
                    }
                    else {
                        pitch_target += user_input.cyclic.y * pitch_power; //increment target
                    }

                    orientation_target.z = Mathf.Lerp(orientation_target.z, pitch_target, pitch_power*Time.deltaTime);//add pitch to orientation target
                }
                if (isRollEnabled)
                { //If user is allowed to change roll
                    if (isRollLimited)
                    { //if user want to continoulsy rotate or not
                        roll_target = user_input.cyclic.x * minMaxRoll; //rotates only to target
                    }
                    else
                    {
                        roll_target += user_input.cyclic.x * roll_power; //increment target
                    }
                    orientation_target.x = Mathf.Lerp(orientation_target.x, roll_target, roll_power*Time.deltaTime);//add pitch to orientation target
                }
                if (isYawEnabled)
                { //If user is allowed to change roll
                    if (isYawLimited)
                    { //if user want to continoulsy rotate or not
                        yaw_target = user_input.throttle.x * minMaxYaw; //rotates only to target
                    }
                    else
                    {
                        yaw_target += user_input.throttle.x * yaw_power; //increment target
                    }
                    orientation_target.y = Mathf.Lerp(orientation_target.y, yaw_target, yaw_power*Time.deltaTime);//add pitch to orientation target
                }

                rot = Quaternion.Euler(orientation_target.x, orientation_target.y, orientation_target.z);//get the rotation vector
                rb.MoveRotation(rot);//rotate drone
                //Add thrust force to engines
              
                foreach (Engine engine in engines)
                {
                    if (maintain_height)
                    {
                        //Controller allowing height adjustment
                        float adjusted_input = Controller.GetAdjustedInputMagnitude(user_input.throttle.y,engine.transform.forward,engine.max_thrust, engines.Count, rb.mass,drone_push);
                        engine.setEngine(adjusted_input);
                    }
                    else
                    {
                        if (user_input.throttle.y < 0)
                        {
                            thrust_input = -user_input.throttle.y * hovering_force_input;
                            //Debug.Log(thrust_input);
                        }
                        else {
                            thrust_input = user_input.throttle.y / engines.Count;
                        }
                        engine.setEngine(thrust_input);
                    }
                }
                variables.cube_drop = true;
                break;
            
            case DroneState.DRONE_STATE_LANDING: //If it is taking off
                break;
        }
        
    }
    #endregion
}
