using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CustomOnScreenButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public CustomOnScreenButtonPublic action1;
    public CustomOnScreenButtonPublic action2;
    public Image tintImage;
    public Image rotateImage;
    public Image dummyImage;
    public float rotationToApply;
    
    public static float buttonRotaAlpha = 0.4f;
    public static float buttonPressedAlpha = 0.3f;
    public static float buttonUnpressedAlpha = 0.1f;
    public static float buttonTextAlpha = 0.16f;
    new bool enabled = true;
    Text text;

    public void Start()
    {
        text = GetComponentInChildren<Text>();

        if (rotateImage != null)
        {
            var c = rotateImage.color;
            c.a = 0f;
            rotateImage.color = c;
        }

        if (dummyImage != null)
        {
            var c = dummyImage.color;
            c.a = orInv(c.a);
            dummyImage.color = c;
        }
        
        /*if (tintImage != null)
        {
            var c = tintImage.color;
            c.a = buttonUnpressedAlpha;
            tintImage.color = c;
        }*/
    }

    public void Update()
    {
        var p = Global.playerController;
        if (p != null)
        {
            if (gameObject.name == "DashBtn")
                enabled = p.state.canDash;
            else if (gameObject.name == "Attack")
                enabled = p.state.canAttack;
            else if (gameObject.name == "Map")
                enabled = p.state.canShowMap;
            else if (gameObject.name == "Cast")
                enabled = p.state.canMagic;

            if (tintImage != null)
            {
                var c = tintImage.color;
                c.a = enabled ? orInv(buttonUnpressedAlpha) : 0;
                tintImage.color = c;
            }
            if(text != null)
            {
                var c = text.color;
                c.a = enabled ? orInv(buttonTextAlpha) : 0;
                text.color = c;
            }
        }
    }

    public void TriggerButtonPressed()
    {
        if (!enabled)
            return;

        if (Global.isDebug) { 
            var text = "INPUT " + gameObject.name + " pressed doing " + action1?.name + " and also " + action2?.name;
            if (Global.playerController != null && Global.playerController.waitTillRespawn != null)
                Global.playerController.waitTillRespawn.text = text;
            Global.LogDebug(text);
        }
        action1.SendValueToControl2(1.0f);
        action2?.SendValueToControl2(1.0f);
        if(tintImage != null)
        { 
            var c = tintImage.color;
            c.a = orInv(buttonPressedAlpha);
            tintImage.color = c;
        }
        if (rotateImage != null)
        {
            var c = rotateImage.color;
            c.a = orInv(buttonRotaAlpha);
            rotateImage.color = c;
            rotateImage.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -rotationToApply));
        }
    }

    private float orInv(float x)
    {
        return Global.settings.input == InputType.TouchpadInvisible ? 0 : x;
    }

    public void ResetButtonPressed()
    {
        if (!enabled)
            return;

        action1.SendValueToControl2(0.0f);
        action2?.SendValueToControl2(0.0f);
        if (tintImage != null)
        {
            var c = tintImage.color;
            c.a = orInv(buttonUnpressedAlpha);
            tintImage.color = c;
        }
        if (rotateImage != null)
        {
            var c = rotateImage.color;
            c.a = 0f;
            rotateImage.color = c;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetButtonPressed();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        TriggerButtonPressed();
    }
    
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        TriggerButtonPressed();
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        ResetButtonPressed();
    }
}
