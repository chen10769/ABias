using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;
using UnityEngine.UI;
using Unity.VisualScripting;

public class DialogManager : MonoBehaviour
{
    public Flowchart flowchart;
    public static DialogManager instance;
    public Text storyText;
    void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {

        if (GameState.instance.gameStateName == GameStateName.start&&PersistentObject.instance.isSettingMode==false)
        {
            //ModifySayCommand("start", 3, "哈哈哈哈哈哈哈哈哈哈哈哈哈{color=red}苹果{/color}哈哈哈哈哈哈哈哈哈哈哈哈");
            if (DotProbeTask.instance.havePre)
            {
                ShowDialog("start");
            }
            else
            {
                 StartCoroutine(DotProbeTask.instance.SkipExperiment());
            }                       
        }
    }
    public void BackToDt()
    {
        if (DotProbeTask.instance.havePost)
        {
            ShowDialog("run");
           
                DotProbeTask.instance.hadDoPractice = false;
            
            
        }
        else
        {
            FirstPersonController.instance.UnlockCursor();
            UIManager.instance.companel.SetActive(true);
        }   
    }
    public void ModifySayCommand(string blockName, int commandIndex, string newText)
    {
        Block block = flowchart.FindBlock(blockName);

        if (block != null && commandIndex < block.CommandList.Count)
        {
            Say sayCommand = block.CommandList[commandIndex] as Say;
            if (sayCommand != null)
            {
                sayCommand.storyText = newText;/*1*/
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ShowDialog(string names)
    {
        if (flowchart.HasBlock(names))
        {
            flowchart.ExecuteBlock(names);

        }
    }
    public void SetTextColor(Color color)
    {
        if (storyText == null)
        {
            storyText = GameObject.Find("StoryText").GetComponent<Text>();
        }
        storyText.color = color;
    }

}
