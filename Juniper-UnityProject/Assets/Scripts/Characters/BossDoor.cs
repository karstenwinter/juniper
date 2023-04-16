using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossDoor : InteractableObject
{
    public override bool GridBasedEnabled() { return false; }

    public SpriteRenderer spriteRenderer;
    public new BoxCollider2D collider;

    public void Start()
    {

    }

    public void lockDoor()
    {
        spriteRenderer.color = Color.white;
        collider.enabled = true;
    }

    public void unlockDoor()
    {
        spriteRenderer.color = new Color(0, 0, 0, 0);
        collider.enabled = false;
    }
}