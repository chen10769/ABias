using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefaultInput : MonoBehaviour
{
    public string inputword;
    private InputField text;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponentInChildren<InputField>();
        if (text.text == "")
        {
            text.text = inputword;
        }
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
}
