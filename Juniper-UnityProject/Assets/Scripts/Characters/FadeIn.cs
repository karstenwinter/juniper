using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    public float targetAlpha = 0.5f;
    public float speed = 0.01f;
    public SpriteRenderer spriteRenderer;
    public virtual void Start()
    {
        if (spriteRenderer != null)
            StartCoroutine("FadeInC");
    }

    public IEnumerator FadeInC()
    {
        var c = spriteRenderer.color;
        c.a = 0;
        spriteRenderer.color = c;
        while (c.a < targetAlpha)
        {
            c.a += speed;
            spriteRenderer.color = c;
            yield return new WaitForSeconds(0.01f);
        }
        c.a = targetAlpha;
        spriteRenderer.color = c;
    }

}