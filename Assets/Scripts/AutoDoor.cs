using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDoor : MonoBehaviour
{
    public Animator door;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && DotProbeTask.instance.isExperimentDone && DotProbeTask.instance.isDoorOpen == false)
        {
            DotProbeTask.instance.isDoorOpen = true;
             door.SetTrigger("Open");
        }
           
    }
     void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && DotProbeTask.instance.isExperimentDone && DotProbeTask.instance.isDoorOpen)
        {
            DotProbeTask.instance.isDoorOpen = false;
            door.SetTrigger("Close");
        }
        
    }
}
