using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deco : InteractableObject
{
    public ParticleSystem particles, touchParticles;
    public AnimatedObject animator;
    public int dealsDamage = 0;
    public float stopAfterSec = 0.5f;
    Sprite deadSprite;
    bool dead;
    public bool touchAnimation = true;
    public bool destroyable = true;
    internal Action<float> onHurtCallback;
    public GameObject alsoDestroy;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<AnimatedObject>();
        }
        animator?.Load();

        deadSprite = animator?.GetFirstOfState("particle");
        if (deadSprite != null && particles != null)
            particles.textureSheetAnimation.SetSprite(0, deadSprite);
    }

    public override void InitFromName(string name)
    {
        base.InitFromName(name);
        if (animator == null)
        {
            animator = GetComponent<AnimatedObject>();
        }
        animator?.Load();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (dead)
            return;

        if (col.tag == "Player" || col.tag == "Enemy")
        {
            if (touchAnimation)
            {
                animator?.Activate("touch", true);
                Invoke("StopAnim", stopAfterSec);
                touchParticles.PlayIfNotPlaying();
            }

            if (dealsDamage > 0)
            {
                var player = col.GetComponent<PlayerController>();
                player?.hurt(dealsDamage);
                Debug.Log("deco hurts player " + player + ", amount: " + dealsDamage);
            }
        }
    }

    void StopAnim()
    {
        animator?.Activate("idle");
    }

    public override void hurt(float damage)
    {
        if (dead)
            return;

        if (onHurtCallback != null)
            onHurtCallback.Invoke(damage);

        if (destroyable)
        {
            if (particles != null)
                particles.Play();

            dead = true;
            animator?.Activate("dead");

            gameObject.RemoveComponent<BoxCollider2D>();
            gameObject.RemoveComponent<Deco>();

            if (alsoDestroy != null)
                Destroy(alsoDestroy);
        }
    }
}
