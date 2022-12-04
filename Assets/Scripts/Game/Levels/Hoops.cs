using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hoops : MonoBehaviour
{
    public enum BoundState
    {
        BOUND_STATE_IN, 
        BOUND_STATE_OUT 
    }
    public Hoops_Level hoops_level;
    

    [System.NonSerialized] public Collider hoops_collider;
    [System.NonSerialized] public BoundState bound_state;
    [System.NonSerialized] public bool through_hoop_once;

    public Text _Score;
    public Text _CollisionCounter;
    private int score;
    private int collisions;

    public Variables variables;
    public Text _GameMessage;


    // Start is called before the first frame update
    void Start()
    {
        hoops_collider = GetComponent<Collider>();
        bound_state = BoundState.BOUND_STATE_OUT;
        through_hoop_once = false;
        score = 0;
        collisions = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (bound_state == BoundState.BOUND_STATE_OUT && !through_hoop_once)
        {
            if (hoops_collider.bounds.Contains(hoops_level.drone.gameObject.transform.position))
            {
                bound_state = BoundState.BOUND_STATE_IN;

            }
        }
        else
        {
            if (!hoops_collider.bounds.Contains(hoops_level.drone.gameObject.transform.position) && !through_hoop_once)
            {
                hoops_level.through_hoops_count++;
                score += 5;
                _Score.text = "Score: " + score;
                through_hoop_once = true;
                
            }
        }
        if (hoops_level.through_hoops_count == hoops_level.number_of_obstacles)
        {
            variables.timer_active = false;
            _GameMessage.text = "You win!";
        }
        
    }

    //On collision what to do
    void OnCollisionEnter(Collision col)
    {
        hoops_level.collision_count++;
        score -= 2;
        collisions += 1;
        _CollisionCounter.text = "Collisions: " + collisions;
        _Score.text = "Score: " + score;
    }
}
