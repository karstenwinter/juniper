using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Conditional : MonoBehaviour
{
    public string destroyIfActivated = "";

    bool checkedCollected;
    
    void Update() {
        if(!checkedCollected && Global.playerController.state.time != 0)
        {
            checkedCollected = true;
            if(Array.IndexOf(Global.playerController.state.activated, destroyIfActivated) >= 0)
            {
            	Debug.Log("destroyed " + this + " because player activated " + destroyIfActivated);
            	
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
    }
}
