using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class AngleTest : MonoBehaviour
{    
    public GameObject other, player, target;
    public Text info;
    public float speed = 0.1f;
    public float dist = 1.4f;
    
    public void Update()
    {
        player.transform.position += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0) * speed;

        var oPos = other.transform.position;
        var pPos = player.transform.position;
        var angle = Mathf.Rad2Deg * (Mathf.Atan2(oPos.y - pPos.y, oPos.x - pPos.x));
        target.transform.rotation = Quaternion.Euler(0, 0, angle);
        target.transform.position = oPos + (pPos - oPos).normalized * dist;
        info.text = "angle between " + oPos + " and " + pPos + " is " + angle;
    }
}