using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TouchpadConverter : MonoBehaviour
{
    public bool touchHandling;
    bool toggle1;
    bool toggle2 = true;
    public Camera cameraMain;

    RaycastHit[] results = new RaycastHit[10];
    ButtonHack[] allBtns = new ButtonHack[0];

    void Start()
    {
        allBtns = Resources.FindObjectsOfTypeAll(typeof(ButtonHack)) as ButtonHack[];
    }

    void Update()
    {
        release2();

        var c = EventSystem.current?.currentInputModule;
        Touch t;
        if (c != null && c.input.touchCount > 0 && (t = c.input.GetTouch(0)).position.x > 0)
        {
            if (t.phase != UnityEngine.TouchPhase.Ended)
            {
                var touchPos = t.position;
                handleTouch(touchPos); //, releaseOtherwise: true);
            }
            // else { release2(); }

            var t2 = c.input.touchCount > 1 ? c.input.GetTouch(1) : default(Touch);
            if (t2.position.x > 0 && t2.phase != UnityEngine.TouchPhase.Ended)
            {
                var touchPos = t2.position;
                handleTouch(touchPos);
            }
            //else if (c.input.touchCount > 1) { release2(); }*/
            //  return;
        }
        else // if (toggle2)
        {
            var m = Mouse.current.position.ReadValue();
            // Debug.Log("Mouse." + m);
            // Debug.Log("m" + Mouse.current + "p" + Mouse.current?.position + "" + m);
            x = m.x;
            y = m.y;
            handleTouch(new Vector2(x, y)); // , releaseOtherwise: true);
            // return;
        }

        // handleTouch(pos);
    }

    Vector2 pos
    {
        get
        {
            var size = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            return new Vector2(x, y) * size;
        }
    }
    void release2()
    {
        foreach (var item in allBtns)
        {
            item.ResetButtonPressed();
        }
    }

    void handleTouch(Vector2 pos, bool releaseOtherwise = false)
    {
        var info = Global.isDebug ? GameObject.Find("InfoText") : null;
        if (info)
        {
            info.transform.position = pos;
            info.GetComponentInChildren<Text>().text = pos + "\n...";
        }

        Ray ray = cameraMain.ScreenPointToRay(pos);
        //Debug.DrawRay(ray.origin + 4 * ray.direction, ray.origin + 4 * ray.direction);
        Debug.DrawRay(ray.origin, ray.direction);

        //GameObject.Find("Start").transform.position = ray.origin;
        //GameObject.Find("Dir").transform.position = ray.origin + 3 * ray.direction;
        // Construct a ray from the current touch coordinates
        int len = Physics.RaycastNonAlloc(ray, results, 10000, LayerMask.NameToLayer("UI"));
        if (len > 0)
        {
            for (int i = 0; i < len; i++)
            {
                results[i].transform.GetComponent<ButtonHack>()?.TriggerButtonPressed();
            }
            if (Global.isDebug)
            {
                var a = "r," + len + string.Join(",", Array.ConvertAll(results, x => x.transform?.gameObject?.name));
                Debug.Log(a);

                GameObject.Find("InfoText").GetComponentInChildren<Text>().text = pos + "\n" + a;
            }
        }
    }

    float x, y;
    bool clicked;

    public void handleX(System.Single value)
    {
        x = value;
        GameObject.Find("InfoText2").GetComponentInChildren<Text>().text = "P" + pos;
        //Debug.Log("handleX" + value);
    }

    public void handleY(System.Single value)
    {
        y = value;
        GameObject.Find("InfoText2").GetComponentInChildren<Text>().text = "P" + pos;
        //Debug.Log("handleY" + value);
    }

    public void click()
    {
        clicked = true;
        Debug.Log("clicked" + clicked);
    }

    public void release()
    {
        clicked = false;
        Debug.Log("clicked" + clicked);
    }

    public void handleToggle1(System.Boolean value)
    {
        toggle1 = value;
    }

    public void handleToggle2(System.Boolean value)
    {
        toggle2 = value;
    }
}
