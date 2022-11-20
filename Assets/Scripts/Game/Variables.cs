using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Variables : MonoBehaviour
{
    #region Variables to move
    [System.NonSerialized] public  Game_Manager.GameState game_state;
    [System.NonSerialized] public  Game_Manager.PlayState play_state;

    [System.NonSerialized] public bool isPitchEnabled; //is pitch enabled
    [System.NonSerialized] public bool isPitchLimited; //Pitch has a maximum value
    [System.NonSerialized] public float minMaxPitch; //Max angle if pitch has maximum value
    [System.NonSerialized] public float pitch_power;

    [System.NonSerialized] public bool isRollEnabled; //is roll enabled
    [System.NonSerialized] public bool isRollLimited; //Roll has a maximum value
    [System.NonSerialized] public float minMaxRoll; //Max angle if Roll has maximum value
    [System.NonSerialized] public float roll_power;

    [System.NonSerialized] public bool isYawEnabled; // is yaw enbled
    [System.NonSerialized] public bool isYawLimited; // is yaw limited to an angle 
    [System.NonSerialized] public float minMaxYaw;
    [System.NonSerialized] public float yaw_power; //yaw power (speed of rotating)

    [System.NonSerialized] public bool maintain_height; //controller to maintain height
    [System.NonSerialized] public float max_thrust;
    [System.NonSerialized] public float max_moment;

    [System.NonSerialized] public int resolution;

    [System.NonSerialized] public int level;


    #endregion
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        
    }
    void Start() {
        game_state = Game_Manager.GameState.GAME_STATE_MAIN_MENU;
        isPitchEnabled = true;
        isPitchLimited = true;
        minMaxPitch = 30f; //Max angle if pitch has maximum value
        pitch_power = 4f;



        isRollEnabled = true; //is roll enabled
        isRollLimited = true; //Roll has a maximum value
        minMaxRoll = 30f; //Max angle if Roll has maximum value
        roll_power = 4f;

        isYawEnabled = true; // is yaw enbled
        isYawLimited = false; // is yaw limited to an angle 
        minMaxYaw = 30f;
        yaw_power = 4f; //yaw power (speed of rotating)

        maintain_height = true; //controller to maintain height
        max_thrust = 20.0f;
        max_moment = 5000.0f;

        resolution = 2;
        level = 0;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
