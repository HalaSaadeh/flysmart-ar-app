using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Button left_button;
    public Button right_button;
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
    

    
    void Start()
    {
        left_button = transform.Find("Left_Page").GetComponent<Button>();
        right_button = transform.Find("Right_Page").GetComponent<Button>();
        left_button.onClick.AddListener(Go_Left);
        right_button.onClick.AddListener(Go_Right);

        this.options = new List<Option>();
        this.pages = new List<Page>();
        this.y_offset =-200.0f;
        //initial_position = new Vector3(400.0f, 800.0f, 0.0f);
        initial_position = new Vector3(240.0f, 1040.0f, 0.0f);
        current_position = initial_position;
        options_per_page = 3;
        current_page_index = 0;
        requested_page_index = current_page_index;

        
        create_setting("settings option", new string[] { "Just this","2" }, new string[] { "Just this","E" }, "var", 0);
        create_setting("settings option 1", new string[] { "Just this","B" }, new string[] { "Just this","F" }, "var", 0);
        create_setting("settings option 2", new string[] { "Just","C" }, new string[] { "Just this","G" }, "var", 0);
        create_setting("settings option 3", new string[] { "Just this","D" }, new string[] { "Just this","H" }, "var", 0);
        create_setting("settings option 3", new string[] { "Just this", "E" }, new string[] { "Just this", "H" }, "var", 0);
        create_setting("settings option 3", new string[] { "Just this", "F" }, new string[] { "Just this", "H" }, "var", 0);
        create_setting("settings option 3", new string[] { "Just this", "G" }, new string[] { "Just this", "H" }, "var", 0);

        create_pages();
        shown_page = pages[current_page_index];
        shown_page.SetActive(true);

    }
    void Update() {
        
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
}
