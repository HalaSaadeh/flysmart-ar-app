using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public PlaceOnPlane placer;
    public Button cube_level_button;
    public Button hoops_level_button;
    public Button train_level_button;
    public Hoops_Level hoops_level_object;

    public Cubes_Level cubes_level_object;


    //General States
    [System.NonSerialized] public static GameState game_state;
    [System.NonSerialized] public static bool is_state_initialized;

    //Sub states of train level
    [System.NonSerialized] public static PlayState play_state;
    [System.NonSerialized] public static bool is_play_state_initialized;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        variables = FindObjectOfType<Variables>();
        is_state_initialized = false;
        is_play_state_initialized = false;

    }



    // Update is called once per frame
    void FixedUpdate()
    {
        game_state = variables.game_state;


        switch (game_state) {
            //===============================MAIN MENU========================================================
            case GameState.GAME_STATE_MAIN_MENU:
                if (!is_state_initialized) {
                    train_level_button.onClick.AddListener(EventOnClickTrainButton);
                    hoops_level_button.onClick.AddListener(EventOnClickHoopsButton);
                    cube_level_button.onClick.AddListener(EventOnClickCubeButton);
                    is_state_initialized = true;
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

                    case PlayState.PLAY_CUBES_LEVEL:
                        if (!is_play_state_initialized)
                        {
                            hoops_level_object.gameObject.SetActive(true);
                            hoops_level_object.SetDifficulty(2); //To be changed according to user
                            hoops_level_object.Generate_Hoops();
                            user_interface.SetActive(true);
                            is_play_state_initialized = true;

                        }

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
        is_state_initialized = false;
        variables.game_state = GameState.GAME_STATE_PLAY;
        variables.play_state = PlayState.PLAY_HOOPS_LEVEL;
    void EventOnClickCubeButton()
    {
        is_state_initialized = false;
        variables.game_state = GameState.GAME_STATE_PLAY;
        variables.play_state = PlayState.PLAY_CUBES_LEVEL;
        SceneManager.LoadScene("Levels");
    }
    #endregion

}
