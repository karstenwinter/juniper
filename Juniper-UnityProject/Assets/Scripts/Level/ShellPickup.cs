using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellPickup : InteractableObject
{
    public ParticleSystem collectParticles;
    public Rigidbody2D rb;
    
    bool collected;
    float rota;
    
    public float partEjectSpeed = 0.1f;
    public float partHitSpeed = 0.3f;
    public float rotaSpeed = 0.4f;
    public float dampen = 0.9f;


    public void Eject(Vector3 dir, Sprite sprite)
    {
        rb.velocity = dir * partEjectSpeed;
        GetComponent<SpriteRenderer>().sprite = sprite;
    }

    void Update()
    {
        var s = rb == null ? 0 : rb.velocity.sqrMagnitude;
        if(s > 0.01f)
        {
            rota += s * rotaSpeed;
            transform.rotation = Quaternion.AngleAxis(rota, Vector3.forward);
            rb.velocity *= dampen;
        }
    }
    
    public override void hurt(float damage)
    {
        var playerT = Global.playerController.transform;
        var playerPos = playerT.position + playerT.localScale.x * Vector3.one + playerT.localScale.y * Vector3.up;
        var delta = transform.position - playerPos;
        rb.AddForce(delta.normalized * partHitSpeed, ForceMode2D.Impulse);
        StopCoroutine("StopMovement");
        StartCoroutine("StopMovement");
    }

    IEnumerator StopMovement() 
    {
        yield return new WaitForSeconds(0.6f);
        for(int i = 0; i < 4; i++) 
        {
            yield return new WaitForSeconds(0.4f);

            if(rb == null)
                yield break;
            
            var vel = rb.velocity;
            vel.x *= 0.7f;
            rb.velocity = vel;
        }

        rb.velocity = new Vector2();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        var player = other.collider.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            if (collected)
            {
                return;
            }
            else
            {
                collected = true;
                Global.playerController.modifyShells(1); // add to Player 1's shells :)
                Global.soundManager.Play("shell");
                collectParticles.PlayIfNotPlaying();
                gameObject.RemoveComponent<Rigidbody2D>();
                gameObject.RemoveComponent<Collider2D>();
                gameObject.RemoveComponent<SpriteRenderer>();

                StartCoroutine("DestroySelf");
                StopCoroutine("StopMovement");
            }
        }
    }

    IEnumerator DestroySelf() 
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
