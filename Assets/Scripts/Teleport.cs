using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform player;
    public Transform office1Pos;
    public Transform office2Pos;
    public Transform office3Pos;
    public Transform backPos;
    public Transform nextPos;
    public Animator fadeScreen;
    public string sceneWord;

    public PolygonCollider2D bound;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

 
    public void EnterScence()
    {
        if (GameState.instance.gameStateName == GameStateName.afterD1)
        {
            GameState.instance.ChangeGameState(GameStateName.beforeV1);
            nextPos = office1Pos;
            sceneWord = "寻找电脑";
        }
        else if (GameState.instance.gameStateName == GameStateName.afterV1)
        {
            GameState.instance.ChangeGameState(GameStateName.beforeD2);
            nextPos = backPos;
            sceneWord = "请前往电梯";
        }
        else if (GameState.instance.gameStateName == GameStateName.afterD2)
        {
            GameState.instance.ChangeGameState(GameStateName.beforeV2);
            nextPos = office2Pos;
            sceneWord = "寻找电脑";
        }
        else if (GameState.instance.gameStateName == GameStateName.afterV2)
        {
            GameState.instance.ChangeGameState(GameStateName.beforeD2);
            nextPos = backPos;
            sceneWord = "请前往电梯";
        }
        else
        {
           
            return;
        }
        fadeScreen.SetTrigger("fade");
        Invoke("ToEnter", 0.75f);
    }
    public void ToEnter()
    {
      
        player.transform.position = nextPos.transform.position;
        Invoke("AfterEnter", 1f);
    }
    public void AfterEnter()
    {
        if (sceneWord != "")
        {
            TipManager.instance.ToShowTip(sceneWord);
            sceneWord = "";
        }
        
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag=="Player")
        {
            
            EnterScence();
        }
    }

}
    

