using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextFade : MonoBehaviour
{
    public static readonly float DefaultFadeDist = 6.4f;
    
    Text text;
    float fadeInDist = DefaultFadeDist;
    float fadeWait = 0.03f;
    float fadeSpeed = 0.1f;
    bool ignoreFadeOut, ignoreFadeIn;
    Color fadeColor { get { return text.color; } set { text.color = value; } }
    public bool canChange = true;
    // string start = "";
    
    void Start() {
        text = GetComponent<Text>();
        if(text == null) {
            Debug.LogError("No component Text in " + this);
        }
        var c = fadeColor;
        c.a = 0;
        fadeColor = c;
        
    } //start = text.text; }

    public void Update() {
        if(canChange) {
            var t = Global.playerController.transform;
            var dist = (transform.position - t.position).sqrMagnitude;
            var sq = fadeInDist * fadeInDist;
            var fadeIn = dist < sq;
            //text.text = start + "@" + dist + " / " + sq + " => " + fadeIn;
            if(fadeIn) {
                if(fadeColor.a != 1) {
                    StopCoroutine("FadeOutC");
                    StartCoroutine("FadeInC");
                }
            } else if(fadeColor.a != 0) {
                StopCoroutine("FadeInC");
                StartCoroutine("FadeOutC");
            }
        }
    }

    public void FadeInForever() {
        canChange = false;
        StartCoroutine(FadeInC());
    }

    IEnumerator FadeInC() {
        var c = fadeColor;
        while(fadeColor.a < 1)  {
            c.a += fadeSpeed;
            fadeColor = c;
            yield return new WaitForSeconds(fadeWait);
        }
    }

    IEnumerator FadeOutC() {
        var c = fadeColor;
        while(fadeColor.a > 0)  {
            c.a -= fadeSpeed;
            fadeColor = c;
            yield return new WaitForSeconds(fadeWait);
        }
    }

    public void FadeOut()
    {
        StopCoroutine("FadeInC");
        StartCoroutine("FadeOutC");
    } 

    public void FadeIn()
    {
        StopCoroutine("FadeOutC");
        StartCoroutine("FadeInC");
    } 
}
