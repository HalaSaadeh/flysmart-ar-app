using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    public Button left_button;
    public Button right_button;
    
    public string option_name;
    public string[] shown_values;

    public object[] object_values;

    public object current_option;
    public object default_option;
    public int current_index;
    public int default_index;

    public string variable_name;

    public int requested_index;
    
    
    public void Create_Option(string name, string[] shown_values, object[] object_values, string variable_name,  int default_index) {

        this.option_name = name;
        this.shown_values = shown_values;
        this.object_values = object_values;
        this.variable_name = variable_name;


        this.default_index = default_index;
        this.requested_index = this.default_index;
        this.current_index = this.default_index;

        this.default_option = this.object_values[this.default_index];
        this.current_option = this.object_values[this.current_index];

        transform.Find("Settings_Name").GetComponent<Text>().text = this.option_name;
        transform.Find("Option_Value").GetComponent<Text>().text = this.shown_values[this.current_index];
        left_button = transform.Find("Left_Option").GetComponent<Button>();
        right_button = transform.Find("Right_Option").GetComponent<Button>();
        left_button.onClick.AddListener(Go_Left);
        right_button.onClick.AddListener(Go_Right);

    }

    void Start() { 
    
    }

    void Update() {
        if (requested_index != current_index)
        {
            if (requested_index > object_values.Length-1)
            {
                requested_index = 0;
            }
            else if (requested_index < 0)
            {
                requested_index = object_values.Length - 1;
            }
            else
            {
                this.current_index = requested_index;
                this.current_option = this.object_values[this.current_index];

                transform.Find("Option_Value").GetComponent<Text>().text = this.shown_values[this.current_index];

               
            }
        }
    }


    void Go_Left() {
        this.requested_index--;
    }
    void Go_Right() {
        this.requested_index++;
    }
    
}

