using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : InteractableObject
{
    public Sprite triggered;
    public GameObject obstacle;
    public GameObject trap;
    bool isTriggered;

    private Sprite origSprite;

    void Start()
    {
        origSprite = GetComponent<SpriteRenderer>().sprite;

    }

    public void turnOn()
    {
        isTriggered = true;

        Global.soundManager.Play("obstacle");
                    
        renderer.sprite = triggered;

        obstacle.GetComponent<Obstacle>().destroy();

        gameObject.layer = LayerMask.NameToLayer("Decoration");
    }

    public void toggle()
    {
        isTriggered = !isTriggered;
        Global.soundManager.Play("obstacle");
                    
        renderer.sprite = isTriggered ? triggered : origSprite;
    }
}
