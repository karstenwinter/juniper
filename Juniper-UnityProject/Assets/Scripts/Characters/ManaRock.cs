using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaRock : InteractableObject
{
    public ParticleSystem particles;
    void Start()
    {
        canBounceOff = false;
        grantsManaOnHit = true;
    }

    public override void hurt(float damage)
    {
        Global.soundManager.Play("manaRock");
        particles?.Play(); 
    }
}
