using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemGet : MonoBehaviour
{
    public Image images;
    public Text texts;
    public static ItemGet instance;

    public GameObject getItemPanel;
    public GameObject evidenceListPanel;
    public List<Item> evidenceList;
    public Item curItem;
    public Item curItemSelect;
    public Image curItemImage;
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
    public void GetItem(string word)
    {
        //FirstPersonController.instance.UnlockCursor();
        Item target = FindEvidenceByName(word);
        Sprite sprite = target.GetComponent<Item>().image.sprite;
        getItemPanel.SetActive(true);
        if (sprite != null)
            images.sprite = sprite;
        if (word != null)
            texts.text = "获得：" + word;
        target.gameObject.SetActive(true);
       
        // if (curItem == null)
        // {
        //     curItemSelect = target;
        //     ChooseEvidence();
        // }
         CheckAllDone();
        

    }
    public void CheckAllDone()
    {
        if (GameState.instance.gameStateName == GameStateName.V1_)
        {

            if (evidenceList[1].gameObject.activeSelf && evidenceList[2].gameObject.activeSelf
            && evidenceList[3].gameObject.activeSelf && evidenceList[4].gameObject.activeSelf)
            {
                DialogManager.instance.flowchart.SetBooleanVariable("flag", true);
                GameState.instance.ChangeGameState(GameStateName.V1__);
            }
            else
            {
                DialogManager.instance.flowchart.SetBooleanVariable("flag", false);
            }
        }
        else if (GameState.instance.gameStateName == GameStateName.V2)
        {

            if (evidenceList[5].gameObject.activeSelf && evidenceList[6].gameObject.activeSelf
            && evidenceList[7].gameObject.activeSelf)
            {
                GameState.instance.ChangeGameState(GameStateName.V2_);
                DialogManager.instance.flowchart.SetBooleanVariable("flag", true);
            }
            else
            {
                DialogManager.instance.flowchart.SetBooleanVariable("flag", false);
            }
        }
        else if (GameState.instance.gameStateName == GameStateName.V3)
        {

            if (evidenceList[8].gameObject.activeSelf && evidenceList[9].gameObject.activeSelf
            )
            {
                GameState.instance.ChangeGameState(GameStateName.V3_);
                DialogManager.instance.flowchart.SetBooleanVariable("flag", true);
            }
            else
            {
                DialogManager.instance.flowchart.SetBooleanVariable("flag", false);
            }
        }
        else
        {
            DialogManager.instance.flowchart.SetBooleanVariable("flag", false);
        }
    }
    public Item FindEvidenceByName(string name)
    {
        return evidenceList.Find(obj => obj.name == name);
    }
    public void ShowEvidenceList()
    {
        evidenceListPanel.SetActive(true);
        if (curItemSelect != null)
        {
            curItemSelect.ShowItem();

        }
        else
        {
            foreach (var item in evidenceList)
            {
                if (item == null) break;
                if (item.gameObject.activeInHierarchy)
                {
                    curItemSelect = item;
                    item.ShowItem();
                    break;
                }
            }
        }

    }
    public void ChooseEvidence()
    {
        if (curItemSelect == null) return;
        curItem = curItemSelect;
        curItemImage.enabled = true;
        curItemImage.sprite = curItem.image.sprite;
        evidenceListPanel.SetActive(false);
    }
    

}
