using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform player;
    public Transform targetPos;
  

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

        
      
        fadeScreen.SetTrigger("fade");
        Invoke("ToEnter", 0.75f);
    }
    public void ToEnter()
    {
      
        player.transform.position = targetPos.transform.position;
        player.transform.rotation = targetPos.transform.rotation;
        Invoke("AfterEnter", 1f);
    }
    public void AfterEnter()
    {
        DialogManager.instance.ShowDialog(sceneWord);
        // if (sceneWord != "")
        // {
        //     TipManager.instance.ToShowTip(sceneWord);
        //     sceneWord = "";
        // }

    }
    // void OnTriggerEnter(Collider other)
    // {
    //     if(other.tag=="Player")
    //     {
            
    //         EnterScence();
    //     }
    // }

}
    

