using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Variables : MonoBehaviour
{
    #region Variables to move
    [System.NonSerialized] public Game_Manager.GameState game_state;
    [System.NonSerialized] public Game_Manager.PlayState play_state;

    #endregion
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        
    }
    void Start() {
        game_state = Game_Manager.GameState.GAME_STATE_MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
