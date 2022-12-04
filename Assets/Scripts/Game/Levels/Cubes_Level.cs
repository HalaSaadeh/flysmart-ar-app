using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cubes_Level : MonoBehaviour
{

    public GameObject cube;
    public Drone drone;
    public Variables variables;
    public Text _Score;
    private int score;


    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        variables = FindObjectOfType<Variables>();
        float repeat_speed = 2.5f - 0.25f * (variables.level - 1);
        InvokeRepeating("Spawn", 3.5f, 2.5f);
    }


    
    void Spawn()
    {
        if (variables.cube_drop)
        {
            Transform dronetransform = drone.transform;

            Transform current_transform = new GameObject().transform;
            current_transform.position = dronetransform.position + new Vector3(0, 2, 0);
            Instantiate(cube, current_transform);
            Debug.Log("hi");

            Debug.Log(current_transform.position.ToString());
            score += 5;
            _Score.text = "Score: " + score;
        }
        
    }
}