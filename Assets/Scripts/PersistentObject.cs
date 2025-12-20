using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.IO;

public class PersistentObject : MonoBehaviour
{

    public bool isSettingMode = false;
    public static PersistentObject instance;
    public string studentId;   // 学号
    public string phoneNumber; // 电话号码
    public string group;       // 组别
    
    



    void Awake()
    {
        if (instance != null && instance != this)
        {
            // 已经有一个 PersistentObject，销毁新生成的
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
