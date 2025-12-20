using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetIsUi : MonoBehaviour
{
    
   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnDisable()
    {
        UIManager.instance.isUI = false;
       //FirstPersonController.instance.LockCursor();
    }

    // Update is called once per frame
    void Update()
    {
        UIManager.instance.isUI = true;
        // FirstPersonController.instance.UnlockCursor();
        if (Input.GetMouseButtonDown(0))
        {
            FirstPersonController.instance.UnlockCursor();
        }
        
    }
}
