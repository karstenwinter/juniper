using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolController : EnemyController
{
    public float walkSpeed = 2f;
    public float waitTillCheck = 0.3f;
    public float edgeSafeDistance = 0.99f;
    public float checkOffsetY = -0.64f;
    public float checkRadius = 0.27f;
    
    public bool reachEdge, reachWall;
    
    private float origScale = 1f;

    public override void Start()
    {
        base.Start();

        origScale = transform.localScale.x;
        StartCoroutine(EnemyBehaviour());
    }

    public IEnumerator EnemyBehaviour() 
    {
        var move = random.Next(2) == 0 ? -1 : 1;

        while(health > 0)
        {
            // flip sprite
            reachEdge = !checkCollision(transform.position + new Vector3(
                edgeSafeDistance * Math.Sign(transform.localScale.x),
                checkOffsetY,
                0
            ), checkRadius);

            reachWall = checkCollision(transform.position + new Vector3(
                edgeSafeDistance * Math.Sign(transform.localScale.x),
                0,
                0
            ), checkRadius);

         
            if(reachWall || reachEdge)
                move = -move;

            if (move != 0)
            {
                Vector3 newScale = transform.localScale;
                newScale.x = move * origScale;
                transform.localScale = newScale;
            }

            // set velocity
            Vector2 newVelocity = _rigidbody.velocity;
            newVelocity.x = move * walkSpeed;
            _rigidbody.velocity = newVelocity;

            // animation
            Activate(move == 0 ? "idle" : "run");

            if(move != 0) {
                CreateDust();
            }

            yield return new WaitForSeconds(waitTillCheck);
        }
    }

    bool checkCollision(Vector2 pos, float radius)
    {
       LayerMask layerMask = LayerMask.GetMask("Platform") | LayerMask.GetMask("Interactable");
       RaycastHit2D hitRec = Physics2D.CircleCast(pos, radius, new Vector2(), 0, layerMask);
       return hitRec.collider != null;
    }
}
