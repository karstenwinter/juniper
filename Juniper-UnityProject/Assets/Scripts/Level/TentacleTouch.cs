using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentacleTouch : MonoBehaviour
{
    public ParticleSystem sporeParticles;
    void OnTriggerEnter2D(Collider2D col)
    {
        sporeParticles.Play();
    }
}
