using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public ParticleSystem gotHitParticleSystem;
    public int health = 3;

    Vector3 startPos;
    public float speed = 2f;
    public float patrolWidth = 8f;
    public GameObject mark;

    bool headingRight;
    float dx;
    SpriteRenderer render;
    Animator animator;

    void Start()
    {
        startPos = transform.position;
        render = GetComponent<SpriteRenderer>();

        animator = GetComponent<Animator>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.tag == "Mark")
            {
                mark = child.gameObject;
                break;
            }
        }
    }

    void Update()
    {
        dx += (headingRight ? speed : -speed) * Time.deltaTime;
        transform.position = startPos + new Vector3(dx, 0, 0);

        if (headingRight && dx > patrolWidth)
        {
            headingRight = false;
        }

        if (!headingRight && dx < -patrolWidth)
        {
            headingRight = true;
        }
        render.flipX = headingRight;
        animator.SetBool("Walk", true);
    }

    public void KillEnemy() {
        if(gameObject.activeInHierarchy) {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            gameObject.SetActive(false);
        }
    }
    
    public void HitEnemy() {
        gotHitParticleSystem.Play();
        health--;
        if(health < 0) {
        }
    }
}
