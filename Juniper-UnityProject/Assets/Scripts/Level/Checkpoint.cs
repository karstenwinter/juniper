using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : InteractableObject
{
    public ParticleSystem particles;
    public AnimatedObject animator;
    bool active;
    string myId;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<AnimatedObject>();
        }
        animator?.Load();

        var particleSprite = animator?.GetFirstOfState("particle");
        if (particleSprite != null && particles != null)
            particles.textureSheetAnimation.SetSprite(0, particleSprite);

        myId = "cx" + (int)transform.position.x + "y" + (int)transform.position.y;

        if (Global.playerController?.state?.checkpoint == myId)
        {
            SetActive();

        }
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
        if (active)
            return;

        if (col.tag == "Player")
        {
            particles.Play();
            SetActive();
            Global.allPlayers.ForEach(x => x.ActivateCheckpoint(transform.position, myId));
            Global.SaveGame();
        }
    }

    void SetInactive()
    {
        active = false;
        animator.Activate("idle");
    }

    void SetActive()
    {
        var others = UnityEngine.Object.FindObjectsOfType<Checkpoint>();
        foreach (var item in others)
        {
            if (item.myId != myId)
            {
                item.SetInactive();
            }
        }

        active = true;
        animator.Activate("active");
    }
}
