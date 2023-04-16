using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType 
{
    AnyDir, FromLeft, FromRight, Invincible
}

public class Obstacle : InteractableObject
{
    public float hitPoints = 1f;
    public ParticleSystem system;
    public ObstacleType obstacleType = ObstacleType.AnyDir;
    public string myId;
    public GameObject toDestroy;

    public void Start() 
    {
        myId = "oy" + (int)transform.position.y + "x" + (int)transform.position.x;

        string[] arr = Global.playerController?.state?.activated;
        if(arr != null && Array.IndexOf(arr, myId) >= 0)
        {
            destroy();
        }
    }

    public void destroy()
    {
        //Destroy(gameObject);
        renderer.gameObject.SetActive(false);
        gameObject.RemoveComponent<Obstacle>();
        gameObject.RemoveComponent<Deadly>();
        gameObject.RemoveComponent<BoxCollider2D>();

        if(toDestroy != null)
            Destroy(toDestroy.gameObject);
    }

    public override void hurt(float damage)
    {
        if (playerController == null)
            return;

        bool conditionMet = obstacleType == ObstacleType.AnyDir;
        
        var playerX = playerController.transform.position.x;

        if(obstacleType == ObstacleType.FromLeft)
            conditionMet = playerX <= transform.position.x;

        if(obstacleType == ObstacleType.FromRight)     
           conditionMet = transform.position.x <= playerX;


        if(Global.isDebug)
            Global.LogDebug("playerX " + playerX + ", selfX" + transform.position.x + ", " + obstacleType);

        if(conditionMet) 
        {
            Global.soundManager.Play("obstacle");
            system.PlayIfNotPlaying();
            hitPoints -= damage;
            if(hitPoints <= 0)
            {
                Global.playerController?.AddActivated(myId);
                destroy();
            }
        }
    }
}
