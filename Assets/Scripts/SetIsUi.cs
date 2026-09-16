using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
        

    }
    // 检测鼠标是否在 UI 元素上
    bool IsPointerOverUI()
    {
        // EventSystem.current.IsPointerOverGameObject() 检查当前指针是否在 UI 上
        return EventSystem.current.IsPointerOverGameObject();
    }
}
