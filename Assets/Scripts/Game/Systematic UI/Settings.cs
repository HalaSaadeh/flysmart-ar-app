using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Settings : MonoBehaviour
{
    public Button left_button;
    public Button right_button;
    public Button back_button;
    public Button submit_button;
    public List<Option> options;
    public List<Page> pages;
    public Option prefab;
    public float y_offset;
    public Vector3 initial_position;
    public Vector3 current_position;
    public int options_per_page;
    public int nb_of_pages;

    public int current_page_index;
    public int requested_page_index;
    Page shown_page;

    
    public GameObject main;
    public Variables variables;
    

    
    void Start()
    {
        //variables = FindObjectOfType<Variables>();
        left_button = transform.Find("Left_Page").GetComponent<Button>();
        right_button = transform.Find("Right_Page").GetComponent<Button>();
        back_button = transform.Find("Back_Button").GetComponent<Button>();
        submit_button = transform.Find("Submit").GetComponent<Button>();

        left_button.onClick.AddListener(Go_Left);
        right_button.onClick.AddListener(Go_Right);
        back_button.onClick.AddListener(Go_Back);
        submit_button.onClick.AddListener(Confirm);


        this.options = new List<Option>();
        this.pages = new List<Page>();
        //this.y_offset =-200.0f;
        this.y_offset = -1.0f;
        initial_position = prefab.transform.position;
        //initial_position = new Vector3(240.0f, 1040.0f, 0.0f);
        current_position = initial_position;
        options_per_page = 4;
        current_page_index = 0;
        requested_page_index = current_page_index;

        
        create_setting("Pitch", new string[] { "Enabled","Disabled" }, generate_bool(), "isPitchEnabled", 0);
        create_setting("Limit Pitch", new string[] { "On","Off" }, generate_bool(), "isPitchLimited", 0);
        create_setting("Max Pitch", convert_float(generate_float(5.0f, 45.0f, 5.0f)), generate_float(5.0f,45.0f,5.0f), "minMaxPitch", 5);
        create_setting("Pitch Power", convert_float(generate_float(1.0f, 10.0f, 1.0f)), generate_float(1.0f, 10.0f, 1.0f), "pitch_power", 3);

        create_setting("Roll", new string[] { "Enabled", "Disabled" }, generate_bool(), "isRollEnabled", 0);
        create_setting("Limit Roll", new string[] { "On", "Off" }, generate_bool(), "isRollLimited", 0);
        create_setting("Max Roll", convert_float(generate_float(5.0f, 45.0f, 5.0f)), generate_float(5.0f, 45.0f, 5.0f), "minMaxRoll", 5);
        create_setting("Roll Power", convert_float(generate_float(1.0f, 10.0f, 1.0f)), generate_float(1.0f, 10.0f, 1.0f), "roll_power", 3);

        create_setting("Yaw", new string[] { "Enabled", "Disabled" }, generate_bool(), "isYawEnabled", 0);
        create_setting("Limit Yaw", new string[] { "On", "Off" }, generate_bool(), "isYawLimited", 1);
        create_setting("Max Yaw", convert_float(generate_float(5.0f, 45.0f, 5.0f)), generate_float(5.0f, 45.0f, 5.0f), "minMaxYaw", 5);
        create_setting("Yaw Power", convert_float(generate_float(1.0f, 10.0f, 1.0f)), generate_float(1.0f, 10.0f, 1.0f), "yaw_power", 3);

        create_setting("Max Thrust Force", convert_float(generate_float(10.0f, 30.0f, 5.0f)), generate_float(10.0f, 30.0f, 5.0f), "max_thrust", 2);
        create_setting("Max Engine Torque", convert_float(generate_float(1000.0f, 10000.0f, 1000.0f)), generate_float(1000.0f, 10000.0f, 1000.0f), "max_moment", 4);
        create_setting("Elevation Controller", new string[] { "Enabled", "Disabled" }, generate_bool(), "maintain_height", 0);

        create_setting("Resolution", convert_int(generate_int(1, 5, 1)), generate_int(1, 5, 1), "resolution", 1);
        create_setting("Gesture", new string[] {"Scroll","Position"},generate_bool(),"gesture",1);

        create_pages();
        shown_page = pages[current_page_index];
        shown_page.SetActive(true);

    }
    void FixedUpdate() {
        
        if (requested_page_index != current_page_index) {
            if (requested_page_index > nb_of_pages-1)
            {
                requested_page_index = 0;
            }
            else if (requested_page_index < 0)
            {
                requested_page_index = nb_of_pages - 1;
            }
            else
            {
                shown_page.SetActive(false);
                shown_page = pages[requested_page_index];
                shown_page.SetActive(true);
                current_page_index = requested_page_index;

            }
        }
    }

    void create_setting(string name, string[] name_string, object[] values,string var_name,int default_index) {
        GameObject option_object = Instantiate(prefab.gameObject, Vector3.zero, Quaternion.identity, transform);
        Option option = option_object.GetComponent<Option>();
        option.Create_Option(name, name_string, values, var_name, default_index);
        options.Add(option);
    }

    void create_pages() {
        nb_of_pages = 0;
        Page current_page = new Page();
       
        for (int i = 0; i < options.Count;i++) {
            Option current_option = options[i];
            if (current_page.size < options_per_page)
            {

                current_option.gameObject.transform.position = current_position;
                current_page.Add(current_option);
                current_position += new Vector3(0.0f, this.y_offset, 0.0f);
                
            }
            else {
                pages.Add(current_page);
                nb_of_pages++;
                current_page = new Page();
                current_position = initial_position;
                current_option.gameObject.transform.position = current_position;
                current_page.Add(current_option);
                current_position += new Vector3(0.0f, this.y_offset, 0.0f);

            }
        }
        
        pages.Add(current_page);
        nb_of_pages++;
        
        
    }

    void Go_Left()
    {
        this.requested_page_index--;
    }
    void Go_Right()
    {
        this.requested_page_index++;
    }
    void Go_Back()
    {

        gameObject.SetActive(false);
        main.SetActive(true);

    }

    void Confirm()
    {
        
        //Set All values
        foreach (Option option in options) {
    
            variables.GetType().GetField(option.variable_name).SetValue(variables,option.current_option);

        }
        
        gameObject.SetActive(false);
        main.SetActive(true);

    }


    object[] generate_bool() { 
        return new object[] { true, false};
    }

    object[] generate_float(float first, float last, float increment) {
        float add = first;
        int array_size = (int)((last - first) / increment + 1);
        object[] output = new object[array_size];

        for (int i = 0; i < array_size; i++) {
            output[i] = add;
            add += increment;
        }
        
        return output;
    }

    string[] convert_float(object[] array)
    {
        string[] output = new string[array.Length];
        for (int i = 0; i < array.Length; i++) {
            output[i] = "" + array[i];
        }
        return output;
    }

    object[] generate_int(int first, int last, int increment)
    {
        int add = first;
        int array_size = (int)((last - first) / increment + 1);
        object[] output = new object[array_size];

        for (int i = 0; i < array_size; i++)
        {
            output[i] = add;
            add += increment;
        }

        return output;
    }

    string[] convert_int(object[] array)
    {
        string[] output = new string[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            output[i] = "" + array[i];
        }
        return output;
    }

}
