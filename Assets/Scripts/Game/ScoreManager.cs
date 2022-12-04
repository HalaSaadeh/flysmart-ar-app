using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public Variables variables;
    public Text _CollisionCounter;
    public Text _Timer;
    public Text _Stopwatch;
    public Text _GameMessage;
    private int collisionCount;
    private float currentTimeTimer;
    private float currentTimeStopwatch;

    // Start is called before the first frame update
    void Start()
    {
        variables = FindObjectOfType<Variables>();
        collisionCount = 0;
        currentTimeTimer = 60;
        currentTimeStopwatch = 0;
    }

    void Update()
    {
        if (variables.level_type.Equals("hoops"))
        {
            _Timer.text = " ";
        }
        else if (variables.level_type.Equals("cubes"))
        {
            if (variables.timer_active)
            {
                currentTimeTimer = currentTimeTimer - Time.deltaTime;
                _Timer.text = currentTimeTimer.ToString();
                if (currentTimeTimer <= 0)
                {
                    winGame();
                }
            }
        }
        if (variables.timer_active)
        {
            currentTimeStopwatch = currentTimeStopwatch + Time.deltaTime;
            _Stopwatch.text = currentTimeStopwatch.ToString();
        }

    }
    void updateCollisionCount()
    {
        collisionCount++;

        _CollisionCounter.text = "Collisions: " + collisionCount;
    }

    void loseGame()
    {
        Destroy(gameObject);
        variables.timer_active = false;
        _GameMessage.text = "You lose!";
    }

    void winGame()
    {
        Destroy(gameObject);
        variables.timer_active = false;
        _GameMessage.text = "You win!";
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log("Object that collided with me: " + col.gameObject.name);
        string collided_object = col.gameObject.name;
        if (collided_object.Equals("Cube(Clone)")) {
            updateCollisionCount();
            loseGame();
        }
        
    }
}
