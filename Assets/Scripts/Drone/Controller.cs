using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller
{
    public static float GetAdjustedInputMagnitude(float throttle,Vector3 forward,float max_thrust,int engine_count,float mass,float drone_push) {
        
        if (throttle == 0.0f) //if no throttle,maintain height
        {
            //We will find by how much we need to scale the unit direction vector in order to get a vertical component equal to the weight
            float y1 = forward.y; //we need the y of the unit vecto
            float y2 = mass * Physics.gravity.magnitude;
            float hovering = (y2 / max_thrust) / engine_count;
            Vector3 total_force = (y2 / y1) * forward; //multiply the scale by the force
            float input = (total_force.magnitude / max_thrust) / engine_count;
            if (input < 0.0f) {
                input = hovering * (1 + input + drone_push);
            }
            return input;
        }
        else {
            float hovering_force = mass * Physics.gravity.magnitude;
            if (throttle < 0)
            {

                throttle = (hovering_force + throttle * (hovering_force - drone_push * hovering_force)) / max_thrust;

            }
            else {
                throttle = (hovering_force + throttle * (max_thrust - hovering_force))/max_thrust;
            }
            return throttle/engine_count;
        }
        
    }
}
