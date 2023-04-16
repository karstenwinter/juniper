using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Shells : InteractableObject
{
    public ParticleSystem collectParticles, destroyParticles;
    public AnimatedObject animator;
    public GameObject shellProto;
    public float collectedAnimDuration = 1f;
    internal bool collected = false;
    internal string myId;
    bool checkedCollected;

    public int partsPerDepot = 10;
    public float partEjectSpeed = 0.4f;
    public float partHitSpeed = 0.3f;
    public float dampen = 0.9f;

    void Start()
    {
        myId = "px" + (int)transform.position.x + "y" + (int)transform.position.y;
    }

    void Update()
    {
        if (Global.playerController == null)
            return;

        if (!checkedCollected && Global.playerController.state.time != 0)
        {
            checkedCollected = true;
            if (Array.IndexOf(Global.playerController.state.collected, myId) >= 0)
            {
                SetCollected();

                animator.Activate("dead");
            }
        }
    }

    void SetCollected()
    {
        collected = true;
        gameObject.RemoveComponent<Shells>();
    }

    public override void hurt(float damage)
    {
        Debug.Log("hurt pickup");
        Global.soundManager.Play("shell");
        destroyParticles.PlayIfNotPlaying();
        collectParticles.PlayIfNotPlaying();
        var len = partsPerDepot;

        for (var i = 0; i < len; i++)
        {
            var inst = Instantiate(shellProto);
            inst.transform.parent = transform;
            inst.transform.localPosition = new Vector3();
            inst.gameObject.SetActive(true);
            var pickup = inst.GetComponent<ShellPickup>();
            pickup.dampen = dampen;
            pickup.partEjectSpeed = partEjectSpeed;
            pickup.partHitSpeed = partHitSpeed;
            var dir = new Vector2(Mathf.Sin(2f * i * Mathf.PI / len), -Mathf.Cos(2f * i * Mathf.PI / len));
            pickup.Eject(dir, animator.GetFirstOfState("pickup"));
        }

        animator.Activate("dead");
        SetCollected();

        Global.playerController.AddCollected(myId);
    }
}
