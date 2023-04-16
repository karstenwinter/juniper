using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectFade : MonoBehaviour
{
    public SpriteRenderer sprite;
    public new BoxCollider2D collider;
    public float fadeInDist = 10.4f;
    public float fadeWait = 0.03f;
    public float fadeSpeed = 0.1f;
    public string myId;
    Color fadeColor { get { return sprite.color; } set { sprite.color = value; } }
    bool triggered;
    
    void Start() {
        myId = this.GetId("bl");

        string[] arr = Global.playerController?.state?.activated;
        if (arr != null && Array.IndexOf(arr, myId) >= 0)
        {
            triggered = true;
            var c = fadeColor;
            c.a = 0;
            fadeColor = c;
        }
        else
        {
            var c = fadeColor;
            c.a = 1;
            fadeColor = c;

        }
    }

    public void ResetState() 
    {
        myId = null;
        triggered = false;
        var c = fadeColor;
        c.a = 1;
        fadeColor = c;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(triggered)
            return;

        var player = collider.gameObject.GetComponent<PlayerController>();
        if(player != null)
        {
            triggered = true;
            Global.soundManager.Play("reveal");
            StartCoroutine(FadeOut());
            if(myId != null)
            {
                Global.playerController?.AddActivated(myId);
            }
        }
    }

    [ContextMenu("SetColliderFromSprite")]
    public void SetColliderFromSprite()
    {
        collider.size = sprite.size;
    }

    IEnumerator FadeOut() {
        var c = fadeColor;
        while(fadeColor.a > 0)  {
            c.a -= fadeSpeed;
            fadeColor = c;
            yield return new WaitForSeconds(fadeWait);
        }
    }
}
