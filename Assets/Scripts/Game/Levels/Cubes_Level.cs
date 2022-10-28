using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cubes_Level : MonoBehaviour
{

    public GameObject cube;
    public Drone drone;
    public static bool drop;


    // Start is called before the first frame update
    void Start()
    {
        drop = false;
        InvokeRepeating("Spawn", 1.0f, 1.0f);
    }
    public static void setDrop(bool input)
    {
        drop = input;
    }

    
    void Spawn()
    {
        if (drop)
        {
            Transform dronetransform = drone.transform;

            Transform current_transform = new GameObject().transform;
            current_transform.position = dronetransform.position; //new Vector3(0, 3, 0);
            Instantiate(cube, current_transform.position, Quaternion.identity);
            Debug.Log("hi");

            Debug.Log(current_transform.position.ToString());
        }
        
    }
}