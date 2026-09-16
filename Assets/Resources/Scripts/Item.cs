using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [TextArea(3, 10)] public string word;
    public Text ItemText;
    public Image ItemImage;
    public Image image;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ShowItem()
    {
        ItemImage.sprite = image.sprite;
        ItemText.text = word;
        ItemGet.instance.curItemSelect = this;

    }
}
