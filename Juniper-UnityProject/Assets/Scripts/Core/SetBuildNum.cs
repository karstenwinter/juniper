using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetBuildNum : MonoBehaviour
{
    public static bool done;
    void Start() {
        done = false;
    }

    void Update()
    {
        if(done)
            return;

        var t = GetComponent<Text>();
        if(t != null) {
            var mobile = "";
            #if UNITY_ANDROID
            mobile = "-m";
            #endif
            t.text = Global.buildNum + mobile + " / " + Global.contentId;
            if(Global.isDebug)
                t.text += " / " + Global.GetPlayerIdOrName();

            done = true;
        }
    }
}
