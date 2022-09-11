using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Pointer : MonoBehaviour
{
    public GameObject canvas;

    [System.NonSerialized] public GameObject pointed_to;
    [System.NonSerialized] public bool is_pointing_to;

    // Start is called before the first frame update
    void Start()
    {
        is_pointing_to = false;
        pointed_to = null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 distance_vector = canvas.transform.position - Camera.main.transform.position;
        float distance = distance_vector.magnitude;
        Vector3 new_position = Camera.main.transform.position + distance * Camera.main.transform.forward;
        new_position.z = canvas.transform.position.z;
        Quaternion new_rotation = canvas.transform.rotation;

        transform.position = new_position;
        transform.rotation = new_rotation;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        is_pointing_to = true;
        pointed_to = other.gameObject;
        
    }

    void OnTriggerExit2D(Collider2D other)
    {
        is_pointing_to = false;
        pointed_to = null;
    }
}
