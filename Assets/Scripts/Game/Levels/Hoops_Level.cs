using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hoops_Level : MonoBehaviour
{
    #region Variables
    public GameObject hoops_object;
    public Drone drone;

    [System.NonSerialized] public int hoops_difficulty;
    [System.NonSerialized] public float forward_distance;
    [System.NonSerialized] public float shape_radius;
    [System.NonSerialized] public float start_finish_angle_radians;
    [System.NonSerialized] public float start_finish_angle_degrees;
    [System.NonSerialized] public float height_from_object;
    [System.NonSerialized] public int number_of_obstacles;

    [System.NonSerialized] public int collision_count;
    [System.NonSerialized] public int through_hoops_count;

    [System.NonSerialized] public Transform drone_transform;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        collision_count = 0;
        through_hoops_count = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetDifficulty(int level) {
        hoops_difficulty = level;
        switch (level)
        {
            case 2:
                //drone.isPitchEnabled = true;
                //drone.isRollEnabled = true;
                //drone.isYawEnabled = true;
                //drone.maintain_height = true;

                forward_distance = 1.0f;
                shape_radius = 3.0f;
                start_finish_angle_radians = 4.71239f;
                start_finish_angle_degrees = 270f;
                height_from_object = 1.0f;
                number_of_obstacles = 7;
                break;

            case 10:
                forward_distance = 1.0f;
                shape_radius = 3.0f;
                start_finish_angle_radians = 4.71239f;
                start_finish_angle_degrees = 270f;
                height_from_object = 1.0f;
                number_of_obstacles = 7;
                break;
        }


    }

    public void Generate_Hoops()
    {
        drone_transform = drone.gameObject.transform;

        float current_angle_radians = 3.14159f;
        float current_angle_degrees = 180.0f;

        float angle_increment_radians = -start_finish_angle_radians / number_of_obstacles;
        float angle_increment_degrees = -start_finish_angle_degrees / number_of_obstacles;

        //origin position to be calculated
        Vector3 origin_pose = new Vector3();
        
        //forward is the right direvtion and right is backward direction
        origin_pose = drone_transform.position + drone_transform.forward * shape_radius + drone_transform.up * height_from_object - drone_transform.right * forward_distance;


        //origin rotation to be calculated
        Vector3 origin_rotation = new Vector3();
        origin_rotation = drone_transform.eulerAngles + new Vector3(-90,90, 0);




        for (int i = 0; i < number_of_obstacles; i++)
        {
            

            Transform current_transform = new GameObject().transform;

            current_transform.position = origin_pose +  drone_transform.forward * shape_radius * Mathf.Cos(current_angle_radians)
                - drone_transform.right * shape_radius* Mathf.Sin(current_angle_radians);
            current_transform.eulerAngles = origin_rotation + new Vector3(0, 0, -current_angle_degrees+180);

            current_angle_degrees += angle_increment_degrees;
            current_angle_radians += angle_increment_radians;

            GameObject obj = Instantiate(hoops_object, current_transform.position, current_transform.rotation);
            obj.gameObject.transform.localScale -= new Vector3(hoops_difficulty - 1, hoops_difficulty - 1, hoops_difficulty - 1);
            obj.SetActive(true);


        }
    }
}
