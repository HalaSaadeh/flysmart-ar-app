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
        GAME_STATE_TRAIN_LEVEL, //use it for debugging
        GAME_STATE_MAIN_MENU
    }

    public enum TrainLevelState { 
        TRAIN_LEVEL_SPAWN_DRONE,
        TRAIN_LEVEL_PLAY
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


    //General States
    [System.NonSerialized] public static GameState game_state;
    [System.NonSerialized] public static bool is_state_initialized;

    //Sub states of train level
    [System.NonSerialized] public static TrainLevelState train_state;
    [System.NonSerialized] public static bool is_train_state_initialized;

    #endregion
    // Start is called before the first frame update
    void Start()
    {
        variables = FindObjectOfType<Variables>();
        is_state_initialized = false;
    }



    // Update is called once per frame
    void Update()
    {
        game_state = variables.game_state;

        switch (game_state) {
            //===============================MAIN MENU========================================================
            case GameState.GAME_STATE_MAIN_MENU:
                if (!is_state_initialized) {
                    train_level_button.onClick.AddListener(EventOnClickTrainButton);
                    is_state_initialized = true;
                }
                break;
            //================================TRAINING LEVEL===================================================
            case GameState.GAME_STATE_TRAIN_LEVEL:
                if (!is_state_initialized)
                {
                    train_state = TrainLevelState.TRAIN_LEVEL_SPAWN_DRONE;
                    is_train_state_initialized = false;
                    is_state_initialized = true;
                }
                

                switch (train_state) {
                    //1. Spawn the drone
                    case TrainLevelState.TRAIN_LEVEL_SPAWN_DRONE:
                        if (!is_train_state_initialized)
                        {
                            placer.gameObject.SetActive(true);
                            is_train_state_initialized = true;
                        }
                        else
                        {
                            if (placer.is_object_placed)
                            {
                                placer.is_object_placed = false;
                                placer.gameObject.SetActive(false);
                                train_state = TrainLevelState.TRAIN_LEVEL_PLAY;
                                is_train_state_initialized = false;
                                
                            }
                        }
                        break;
                    //2. Play
                    case TrainLevelState.TRAIN_LEVEL_PLAY:
                        if (!is_train_state_initialized)
                        {
                            user_interface.SetActive(true);
                            is_state_initialized = true;
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
        variables.game_state = GameState.GAME_STATE_TRAIN_LEVEL;
        SceneManager.LoadScene("Train");
    }
    #endregion

}



