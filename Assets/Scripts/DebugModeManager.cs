using UnityEngine;
using System.IO;
using JetBrains.Annotations;

public class DebugModeManager : MonoBehaviour
{
    public bool debugMode = false;
    private float holdTime = 0f;
    private const float requiredHoldTime = 2f;
    private bool hasToggled = false;

    private string stateFilePath;
    public GameObject settingbutton;
    public GameObject settingbutton1;


    void Awake()
    {
        // 保存文件放在游戏根目录（可执行文件旁）
        stateFilePath = Path.Combine(Application.dataPath, "../debugState.txt");

        // 读取状态
        if (File.Exists(stateFilePath))
        {
            string state = File.ReadAllText(stateFilePath).Trim();
            debugMode = state == "true";
        } else
        {
            // 如果文件不存在，创建并默认写入 true
            debugMode = true;
            File.WriteAllText(stateFilePath, "true");
        }
        if (debugMode)
        {
            //TipManager.instance.ToShowTip("进入设置模式");
            settingbutton.SetActive(true);
            settingbutton1.SetActive(true);
        }

        else
        {
            //TipManager.instance.ToShowTip("进入被试模式");
            settingbutton.SetActive(false);
            settingbutton1.SetActive(false);
        }

        Debug.Log("当前 DebugMode 状态: " + debugMode);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.T))
        {
            holdTime += Time.deltaTime;

            if (holdTime >= requiredHoldTime && !hasToggled)
            {
                // 切换状态
                debugMode = !debugMode;
                hasToggled = true;

                if (debugMode)
                {
                    TipManager.instance.ToShowTip("进入设置模式");
                    settingbutton.SetActive(true);
                    settingbutton1.SetActive(true);
                }

                else
                {
                    TipManager.instance.ToShowTip("进入被试模式");
                    settingbutton.SetActive(false);
                    settingbutton1.SetActive(false);
                }

                // 保存状态
                File.WriteAllText(stateFilePath, debugMode ? "true" : "false");
            }
        }
        else
        {
            // 松开按键时重置
            holdTime = 0f;
            hasToggled = false;
        }
    }
    public void ExitApp()
    {
        Application.Quit();
    }
}
