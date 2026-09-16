using UnityEngine;
using UnityEngine.Events;

public class Evidence : MonoBehaviour
{
    public Outline outline;
    public Interactable interactable;

    public UnityEvent clickEvent;
    private bool haveClick = false;

    public string evidenceName;
    public bool SetFalseAfterGet = false;

    public bool willDoVs;
    public bool hadDoVs;

    void Start()
    {
        outline.enabled = false;
    }

    /// <summary>
    /// 鼠标悬停时调用
    /// </summary>
    public void OnHover()
    {
        if (haveClick) return;
        if (interactable.canInteract)
        {
            ShowEvidence();
        }
        else
        {
            CloseEvidence();
        }
    }

    /// <summary>
    /// 鼠标离开时调用
    /// </summary>
    public void OnHoverExit()
    {
        CloseEvidence();
    }

    /// <summary>
    /// 点击时调用
    /// </summary>
    public void OnClick()
    {
        if (haveClick) return;
        if (!interactable.canInteract) return;
        if (GameState.instance.gameStateName == GameStateName.V1)
        {
            //DialogManager.instance.ShowDialog("糟糕");
            GameState.instance.ChangeGameState(GameStateName.V1_);
             haveClick = true;
            clickEvent.Invoke();
        }

        // if (GameState.instance.gameStateName >= GameStateName.V1_ && !hadDoVs && willDoVs && VisualSearchExperiment.instance.Trigger())
        // {
        //     DialogManager.instance.ShowDialog("糟糕");
        //     hadDoVs = true;
        // }
        else
        {
            haveClick = true;
            clickEvent.Invoke();
        }

        CloseEvidence();
    }
    public void SetHaveClick(bool flag)
    {
        haveClick = flag;
    }

    public void ShowEvidence()
    {
        outline.enabled = true;
        UIManager.instance.Investigate.SetActive(true);
    }

    public void CloseEvidence()
    {
        outline.enabled = false;
        UIManager.instance.Investigate.SetActive(false);
    }

    public void GetEvidence()
    {
        if (evidenceName != null)
        {
            ItemGet.instance.GetItem(evidenceName);
            if (SetFalseAfterGet)
            {
                gameObject.SetActive(false);
            }
             
        }

    }
    public void CheckEvidence()
    {
        if (evidenceName == "door1")
        {
            if (ItemGet.instance.curItem != null && ItemGet.instance.curItem.name == "守卫的ID卡")
            {
                DialogManager.instance.ShowDialog("进门1");
            }
            else
            {
                DialogManager.instance.ShowDialog("无ID卡");
                haveClick = false;
            }
        }
        else if (evidenceName == "door1_")
        {
            if (GameState.instance.gameStateName >= GameStateName.V1__)
            {
                DialogManager.instance.ShowDialog("回去1");

            }
            else
            {
                DialogManager.instance.ShowDialog("未调查完成");
                haveClick = false;
            }
        }
        else if (evidenceName == "door2")
        {
            if (ItemGet.instance.curItem != null && ItemGet.instance.curItem.name == "守卫的ID卡")
            {
                DialogManager.instance.ShowDialog("进门2");
            }
            else
            {
                DialogManager.instance.ShowDialog("无ID卡2");
                haveClick = false;
            }
        }
        else if (evidenceName == "door2_")
        {
            if (GameState.instance.gameStateName >= GameStateName.V2_)
            {
                DialogManager.instance.ShowDialog("回去2");
            }
            else
            {
                DialogManager.instance.ShowDialog("未调查完成");
                haveClick = false;
            }
        }
        else if (evidenceName == "door3")
        {
            if (ItemGet.instance.curItem != null && ItemGet.instance.curItem.name == "守卫的ID卡")
            {
                DialogManager.instance.ShowDialog("进门3");
            }
            else
            {
                DialogManager.instance.ShowDialog("无ID卡2");
                haveClick = false;
            }
        }
        else if (evidenceName == "door3_")
        {
            if (GameState.instance.gameStateName >= GameStateName.V3_)
            {
                DialogManager.instance.ShowDialog("回去3");
            }
            else
            {
                DialogManager.instance.ShowDialog("未调查完成");
                haveClick = false;
            }
        }
        else if (evidenceName == "补贴批复文件")
        {
            if (ItemGet.instance.curItem != null && ItemGet.instance.curItem.name == "员工笔记2")
            {
                DialogManager.instance.ShowDialog("获得批复文件");
            }
            else
            {
                DialogManager.instance.ShowDialog("调查打印机");
                haveClick = false;
            }
        }
        else if (evidenceName == "邮件")
        {
            if (ItemGet.instance.curItem != null && ItemGet.instance.curItem.name == "U盘")
            {
                DialogManager.instance.ShowDialog("获得邮件");
            }
            else
            {
                DialogManager.instance.ShowDialog("调查电脑");
                haveClick = false;
            }
        }
        else if (evidenceName == "保险箱")
        {
            if (ItemGet.instance.curItem != null && ItemGet.instance.curItem.name == "钥匙")
            {
                DialogManager.instance.ShowDialog("打开保险箱");
            }
            else
            {
                DialogManager.instance.ShowDialog("调查保险箱");
                haveClick = false;
            }
        }
        else if (evidenceName == "交易与通信记录")
        {
            if (ItemGet.instance.curItem != null && ItemGet.instance.curItem.name == "邮件")
            {
                DialogManager.instance.ShowDialog("获得记录");
            }
            else
            {
                DialogManager.instance.ShowDialog("调查电脑1");
                haveClick = false;
            }
        }
       
    }
}
