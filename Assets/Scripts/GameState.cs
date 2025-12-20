using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public enum GameStateName
{
    beforeD1,
    afterD1,
    beforeV1,
    afterV1,
    beforeD2,
    afterD2,
    beforeV2,
    afterV2,
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
            case GameStateName.afterV1:
                //afterV1Event.Invoke();
                break;

        }
    }
}
