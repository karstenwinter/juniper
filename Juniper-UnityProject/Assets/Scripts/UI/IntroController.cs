using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class IntroController : MonoBehaviour
{
    public void OnJump()
    {
        speed = 0;
    }
    public GameObject mobileControls;

    public AudioSource endSound;
    public SpriteRenderer fade;
    public Text text;
    private string[] lines2 =
        @"I remember the Giants born of yore,
who bred me up long ago.
I remember nine Worlds, nine Sibyls,
a glorious Judge beneath the earth.

- Voluspa (Prophecy of the seeress)".Split('\n');
    public float speed = 0.08f;
    public float fadeShowsBg = 0.4f;
    public float fadeHideBg = 0.9f;
    public int letterFadeStep = 10;

    public void Start()
    {
        Global.RefreshSound(Global.settings.sound);
        Load();
        mobileControls.SetActive(Global.settings.input == InputType.Touchpad);
    }

    [ContextMenu("Load")]
    public void Load()
    {
        var x = Translations.For("IntroText");
        if(x != "IntroText")
            lines2 = x.Split('\n');
        
        text.text = "";
        var c = fade.color;
        c.a = fadeHideBg;
        fade.color = c;

        StartCoroutine(Trigger());
    }
    
    public IEnumerator Trigger()
    {
        yield return new WaitForSeconds(0.8f);
        //endSound.Play();

        while (fade.color.a > fadeShowsBg)
        {
            var c = fade.color;
            c.a -= 0.1f;
            fade.color = c;
            yield return new WaitForSeconds(speed);
        }
        yield return new WaitForSeconds(8 * speed);

        string before = "";
        foreach (var line in lines2)
        {
            for (int i = 0; i < line.Length; i++)
            {
                //Debug.Log("line: " + line + ", i: " + i + ", len " + line.Length);
                var textAfter = line.Substring(i + 1, line.Length - i - 1);
                var textAfterFaded = "";

                int alpha = 255;
                foreach(var letter in textAfter)
                {
                    alpha -= letterFadeStep;
                    textAfterFaded += "<color=#ffffff"+(alpha < 10 ? "00" : alpha.ToString("X"))+">" + letter + "</color>";
                }
                text.text =
                    before +
                        line.Substring(0, i + 1)
                         + textAfterFaded;
                
                yield return new WaitForSeconds(speed);
            }
            before += line + "\n";
            yield return new WaitForSeconds(8 * speed);
        }
        endSound.Play();

        yield return new WaitForSeconds(8 * speed);

        while (fade.color.a < fadeHideBg)
        {
            var c = fade.color;
            c.a += 0.1f;
            fade.color = c;
            yield return new WaitForSeconds(speed);
        }

        SceneManager.LoadScene("Midgard");
    }
}
