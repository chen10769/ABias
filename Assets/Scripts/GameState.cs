using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public enum GameStateName
{
    start,//开始
    D1,//做完点探测，离开电梯后
    V1,//第一次进门后
    V1_,//第一次触发迷雾后
    V1__,//调查完第一间所有物品
    V2,//进入第二间
    V2_,//调查完第二间所有物品
     V3,//进入第三间
    V3_,//调查完第三间所有物品
    D2
    
}
public class GameState : MonoBehaviour
{
    public static GameState instance;
    public GameStateName gameStateName;
 
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
    public void ChangeGameState(GameStateName stateName)
    {
        gameStateName = stateName;
        switch (stateName)
        {
            case GameStateName.V1__:
                DialogManager.instance.ShowDialog("调查完成");
                break;

        }
    }
}
