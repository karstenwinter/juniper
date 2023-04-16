using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SetCinemachineBounds : MonoBehaviour
{
    public bool done;
    public string status;
    void Update()
    {
        if(done)
            return;
        //var gc = GlobalController.instance;
        //var camObj = .camera.GetComponent<CinemachineVirtualCamera>();
       
        //var cam = Camera.main.GetComponent<CinemachineVirtualCamera>();
        if(Camera.main != null) {
            var confiner = Camera.main.GetComponent<CinemachineConfiner>();
            if(confiner != null) {
                confiner.m_BoundingShape2D = GetComponent<PolygonCollider2D>();
                status = "confiner: " + confiner + " with "+confiner.m_BoundingShape2D;
                done = true;
            }
        } else {
            status = "nocam";
        }
    }
}
