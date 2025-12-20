using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Information : MonoBehaviour
{
    public InputField studentIdInput;
    public InputField phoneNumberInput;
    public InputField groupInput;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
     public void GetInformation()
    {

        if (studentIdInput.text == ""||phoneNumberInput.text==""||groupInput.text==""
        )
        {
            TipManager.instance.ToShowTip("请输入所有的信息！");
            return;
        }
        PersistentObject.instance.studentId=studentIdInput.text;
        PersistentObject.instance.phoneNumber=phoneNumberInput.text;
        PersistentObject.instance.group=groupInput.text;
        HeneGames.Sceneloader.LoadingScreen.instance.LoadScene(1);
    }
}
