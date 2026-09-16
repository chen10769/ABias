using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public bool isUI = false;
    public static UIManager instance;
    public GameObject startButton;
    public Animator crossHairAni;
    public Animator damageScreen;
    public GameObject comp;
    public GameObject pausePanel;
    public GameObject companel;
     public GameObject breakPanel; // 休息面板（包含倒计时文本）
     public GameObject practiceResultPanel;        // 显示练习结果的面板
     public GameObject EvidenceListPanel;        
     public Text practiceResultText;               // 面板上的提示文字
     public Text breakText;
    public GameObject Investigate;
    public Text scoreText;
   
   
    void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
     void Update()
    {
       
    }
    public void ExitApp()
    {
        Application.Quit();
    }
    // 公共方法：触发准星变红并渐变恢复
}
