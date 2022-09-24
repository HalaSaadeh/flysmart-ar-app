using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Page
{
    public List<Option> my_options;
    public int size;

    public Page() {
        my_options = new List<Option>();
        size = 0;
    }

    public void Add(Option option) {
        my_options.Add(option);
        size++;
    }

    public void SetActive(bool active) {
        
        foreach (Option option in my_options) {
            option.gameObject.SetActive(active);
        }
        
    }
    public object Clone() {
        return this.MemberwiseClone();
    }
}
