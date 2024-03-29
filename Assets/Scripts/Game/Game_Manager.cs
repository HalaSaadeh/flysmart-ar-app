using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class Game_Manager : MonoBehaviour
{
    #region enums
    public enum GameState
    {
        GAME_STATE_PLAY, //use it for debugging
        GAME_STATE_MAIN_MENU
    }

    public enum PlayState {
        PLAY_SPAWN_DRONE,
        PLAY_TRAIN_LEVEL,
        PLAY_CUBES_LEVEL,
        PLAY_HOOPS_LEVEL
    }

    #endregion
    #region Variables
    [Header("Game Objects")]
    public Variables variables;
    public Drone drone;
    public GameObject user_interface;
    public User_Input user_input;
    public PlaceOnPlane placer;
    public Button cube_level_button;
    public Button hoops_level_button;
    public Button train_level_button;
    public Hoops_Level hoops_level_object;
    public Button settings_button;
    public GameObject settings;
    public GameObject main;
    public GameObject level_selection;
    public List<Button> level_buttons;
    public Pointer pointer;

    public GameObject canvas;


    public Cubes_Level cubes_level_object;
    bool already_clicked;

    private string level_type = "";


    //General States
    [System.NonSerialized] public static GameState game_state;
    [System.NonSerialized] public static bool is_state_initialized;

    

    //Sub states of train level
    [System.NonSerialized] public static PlayState play_state;
    [System.NonSerialized] public static bool is_play_state_initialized;

    [System.NonSerialized] float timer = 0.0f;
    [System.NonSerialized] float target_timer = 5.0f;
    [System.NonSerialized] bool timerReached = false;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        variables = FindObjectOfType<Variables>();
        is_state_initialized = false;
        is_play_state_initialized = false;
        already_clicked = false;
        target_timer = 5.0f;
        timerReached = false;


    }



    // Update is called once per frame
    void FixedUpdate()
    {
        game_state = variables.game_state;


        switch (game_state) {
            //===============================MAIN MENU========================================================
            case GameState.GAME_STATE_MAIN_MENU:
                if (!is_state_initialized) {

                    if (!timerReached)
                    {
                        timer += Time.deltaTime;
                        if (timer > target_timer)
                        {
                            timerReached = true;
                        }
                    }
                    else
                    {
                        canvas.SetActive(true);
                        train_level_button.onClick.AddListener(EventOnClickTrainButton);
                        hoops_level_button.onClick.AddListener(EventOnClickHoopsButton);
                        cube_level_button.onClick.AddListener(EventOnClickCubeButton);
                        settings_button.onClick.AddListener(EventOnClickSettingsButton);
                        foreach (var button in level_buttons)
                        {
                            button.onClick.AddListener(EventOnClickLevelButton);
                        }


                        pointer.gameObject.SetActive(true);
                        is_state_initialized = true;
                        break;
                    }
                }
                
                if (user_input.click_detected)
                {
                    if (pointer.is_pointing_to)
                    {
                            
                        pointer.pointed_to.GetComponent<Button>().onClick.Invoke();                   
                              
                    }
                }
                break;
            //================================ALL TYPES OF LEVELS===================================================
            case GameState.GAME_STATE_PLAY:
                if (!is_state_initialized)
                {
                    play_state = PlayState.PLAY_SPAWN_DRONE;
                    is_play_state_initialized = false;
                    is_state_initialized = true;
                }


                switch (play_state) {
                    //1. Spawn the drone
                    case PlayState.PLAY_SPAWN_DRONE:
                        if (!is_play_state_initialized)
                        {
                            placer.gameObject.SetActive(true);
                            is_play_state_initialized = true;
                        }
                        else
                        {
                            if (placer.is_object_placed)
                            {
                                placer.is_object_placed = false;
                                placer.gameObject.SetActive(false);
                                play_state = variables.play_state;
                                is_play_state_initialized = false;

                            }
                        }
                        break;

                    //2. Play
                    case PlayState.PLAY_TRAIN_LEVEL:
                        if (!is_play_state_initialized)
                        {
                            user_interface.SetActive(true);
                            is_state_initialized = true;
                            is_play_state_initialized = true;
                        }
                        break;

                    case PlayState.PLAY_HOOPS_LEVEL:
                        if (!is_play_state_initialized)
                        {
                            Debug.Log(variables.level);
                            user_interface.SetActive(true);
                            hoops_level_object.gameObject.SetActive(true);
                            hoops_level_object.SetDifficulty(variables.level);
                            hoops_level_object.Generate_Hoops();
                            is_play_state_initialized = true;

                        }
                        break;

                    case PlayState.PLAY_CUBES_LEVEL:
                        if (!is_play_state_initialized)
                        {
                            cubes_level_object.gameObject.SetActive(true);
                            user_interface.SetActive(true);
                            is_play_state_initialized = true;
                        }

                                              

                        break;
                }

                break;

        }
    }


    #region Button Events
    void EventOnClickTrainButton()
    {
        is_state_initialized = false;
        variables.game_state = GameState.GAME_STATE_PLAY;
        variables.play_state = PlayState.PLAY_TRAIN_LEVEL;
        SceneManager.LoadScene("Levels");
    }

    void EventOnClickHoopsButton()
    {
        main.SetActive(false);
        level_selection.SetActive(true);
        variables.level_type = "hoops";
        level_type = "hoops";
    }
    void EventOnClickCubeButton()
    {
        main.SetActive(false);
        level_selection.SetActive(true);
        variables.level_type = "cubes";
        level_type = "cubes";
    }

    void EventOnClickSettingsButton() {
        main.SetActive(false);
        settings.SetActive(true);
        
    }
    void EventOnClickLevelButton()
    {
        //var button_name = EventSystem.current.currentSelectedGameObject.name;
        var button_name = pointer.object_name;
        switch (button_name)
        {
            case "Level_1":
                variables.level = 1;
                break;
            case "Level_2":
                variables.level = 2;
                break;
            case "Level_3":
                variables.level = 3;
                break;
            case "Level_4":
                variables.level = 4;
                break;
            case "Level_5":
                variables.level = 5;
                break;
            case "Level_6":
                variables.level = 6;
                break;
            case "Level_7":
                variables.level = 7;
                break;
            case "Level_8":
                variables.level = 8;
                break;
            case "Level_9":
                variables.level = 9;
                break;
            case "Level_10":
                variables.level = 10;
                break;
        }

        if (level_type == "cubes")
        {
            is_state_initialized = false;
            variables.game_state = GameState.GAME_STATE_PLAY;
            variables.play_state = PlayState.PLAY_CUBES_LEVEL;
            SceneManager.LoadScene("Levels");
        }
        if (level_type == "hoops")
        {

            is_state_initialized = false;
            variables.game_state = GameState.GAME_STATE_PLAY;
            variables.play_state = PlayState.PLAY_HOOPS_LEVEL;
            SceneManager.LoadScene("Levels");
        }

    }

    #endregion

}