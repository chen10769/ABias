using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipManager : MonoBehaviour
{
    public GameObject tipPanel;
    public Text tipText;
    public static TipManager instance;
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
    public void ToShowTip(string word)
    {
        tipText.text = word;
        tipPanel.SetActive(false);
        tipPanel.SetActive(true);
    }
}
