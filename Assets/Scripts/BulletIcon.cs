using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletIcon : MonoBehaviour
{
        public float floatSpeed = 1f;       // 控制浮动速度
    public float floatHeight = 0.5f;    // 控制浮动高度
    private Vector3 startPos;
    public GameObject tip;
    public GameObject getBulletUI;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }
    void OnEnable()
    {
        tip.SetActive(true);
    }
    void OnDisable()
    {
        if(tip)
        tip.SetActive(false);
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "detect")
        {
            DotProbeTask.instance.StartCoroutine(DotProbeTask.instance.NextFloor());
            SoundManager.instance.PlaySound("Audio/reload");
            getBulletUI.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
  
