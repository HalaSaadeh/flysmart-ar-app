using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    #region Variables
    public Variables variables;
    [System.NonSerialized] public Drone parent_drone; //The drone that contains this engine
    [System.NonSerialized] public Vector3 force_applied; //Initially no force is applied
    [System.NonSerialized] public Vector3 moment_applied =new Vector3(0.0f, 0.0f, 0.0f);
    [System.NonSerialized] public float force_magnitude; //force applied magnitude
    [System.NonSerialized] public float moment_magnitude;
    [System.NonSerialized] public float max_thrust;
    [System.NonSerialized] public float max_moment;
    #endregion
    // Start is called before the first frame update
    void Awake()
    {
        variables = FindObjectOfType<Variables>();
        force_applied = new Vector3(0.0f, 0.0f, 0.0f); //Initially no force is applied
        moment_applied = new Vector3(0.0f, 0.0f, 0.0f);
        force_magnitude = 0.0f; //force applied magnitude
        moment_magnitude = 0.0f;
        max_thrust = variables.max_thrust;
        max_moment = variables.max_moment;
        parent_drone = this.transform.parent.gameObject.GetComponent<Drone>();  //get the drone object
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        parent_drone.gameObject.GetComponent<Rigidbody>().AddForce(force_applied, ForceMode.Force); //Add force to the drone
        transform.Rotate(moment_applied);
        
    }

    //Set the force and propeller speed applied from the drone parent
    //engine input should be between 0 and 1
    public void setEngine(float engine_input) {
        //Get force applied
        float target_force = engine_input * max_thrust; //get the force target from the user input
        force_magnitude = Mathf.Lerp(force_magnitude, target_force, Time.deltaTime); //allow smooth transition to the target force
        force_applied = transform.forward*force_magnitude; //Calculates the thrust force applied on the engine from an input coming from the drone

        //Get moment applied
        float target_moment = Mathf.Abs(engine_input * max_moment);
        // Debug.Log(target_moment);
        moment_magnitude = Mathf.Lerp(moment_magnitude, target_moment, Time.deltaTime);
        moment_applied.z = moment_magnitude;
        
    }

    

}
