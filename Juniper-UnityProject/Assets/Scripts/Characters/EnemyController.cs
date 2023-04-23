using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyController : InteractableObject
{
    public bool changesMusic = true;

    public ParticleSystem gotHitParticleSystem, dustParticleSystem, hitParticleSystem, dieParticleSystem;
    public float health;
    public float detectDistance;
    public int damageToPlayer;
    public AnimatedObject animator;
    public Rigidbody2D _rigidbody;
    public bool dead;

    public Vector2 hurtRecoil;
    public float hurtRecoilTime;
    public Vector2 deathForce;
    public float destroyDelay;

    public bool respawns = true;
    bool setFromSaveState;
    internal string myId;
    protected internal string currentAnimState;

    public float scale = 1f;
    public string tone = "";

    public bool gameEndsOnDefeat;
    internal System.Random random;

    public override void InitFromData(TableData data)
    {
        base.InitFromData(data);

        health = data.hp;
        damageToPlayer = data.damage;

        Global.LogDebug("enemy init from table " + data.location + " => hp " + health + ", damage: " + damageToPlayer);
    }

    public virtual void Start()
    {
        random = new System.Random((int)Global.settings.profile + 100 * (int)transform.position.x + 10000 * (int)transform.position.y);

        grantsManaOnHit = true;

        var vec = transform.localScale;
        vec.x = scale;
        vec.y = scale;
        transform.localScale = vec;

        Color parsed;
        if (ColorUtility.TryParseHtmlString(tone, out parsed) && renderer != null)
        {
            renderer.color = parsed;
        }

        myId = "ex" + (int)transform.position.x + "y" + (int)transform.position.y;

        animator?.Load();
    }

    public void Activate(string state, bool playToEnd = false)
    {
        currentAnimState = state;
        animator.Activate(state, playToEnd);
    }

    public virtual void Update()
    {
        if (Global.isPaused)
            return;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("collision enemy to " + collision + "- " + collision?.collider?.gameObject);
        if (dead)
            return;

        string layerName = LayerMask.LayerToName(collision.collider.gameObject.layer);

        if (layerName == "Player" && damageToPlayer > 0)
        {
            PlayerController playerController = collision.collider.GetComponent<PlayerController>();
            playerController.hurt(damageToPlayer);

            hitParticleSystem.PlayIfNotPlaying();
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("Tr enemy to " + collider + "- " + collider?.gameObject);
        if (dead)
            return;

        string layerName = LayerMask.LayerToName(collider.gameObject.layer);

        if (layerName == "Player" && damageToPlayer > 0)
        {
            PlayerController playerController = collider.GetComponent<PlayerController>();
            playerController.hurt(damageToPlayer);

            hitParticleSystem.PlayIfNotPlaying();
        }
    }

    protected virtual IEnumerator fadeCoroutine()
    {
        while (destroyDelay > 0)
        {
            destroyDelay -= Time.deltaTime;
            yield return new WaitForSeconds(0.08f);
            _rigidbody.velocity *= 0.5f;
        }
        _rigidbody.velocity = new Vector2();
    }

    protected virtual void CreateDust()
    {
        dustParticleSystem.PlayIfNotPlaying();
    }
    public virtual float behaveInterval()
    {
        return 1f;
    }

    public override void hurt(float damage)
    {
        if (health <= 0)
        {
            return;
        }

        Global.soundManager.Play("enemy");
        gotHitParticleSystem.PlayIfNotPlaying();
        onHurtEnemy();

        health = Math.Max(health - damage, 0);

        if (health <= 0)
        {
            die();
            return;
        }

        if (hurtRecoil != Vector2.zero)
        {
            Vector2 newVelocity = hurtRecoil;
            newVelocity.x *= Math.Sign(transform.localScale.x);

            _rigidbody.velocity = newVelocity;

            StopCoroutine("hurtCoroutine");
            StartCoroutine("hurtCoroutine");
        }
    }

    protected void onHurtEnemy() { }
    protected void onHurtEnemyEnd() { }

    protected virtual IEnumerator hurtCoroutine()
    {
        yield return new WaitForSeconds(hurtRecoilTime);

        Vector2 newVelocity;
        newVelocity.x = 0;
        newVelocity.y = 0;
        _rigidbody.velocity = newVelocity;

        onHurtEnemyEnd();
    }

    protected virtual void die()
    {
        Global.soundManager.Play("enemyDead");
        dieParticleSystem.PlayIfNotPlaying();
        Activate("dead");

        _rigidbody.velocity = new Vector2();

        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");

        var newForce = new Vector2(
            transform.localScale.x * deathForce.x,
            deathForce.y);
        _rigidbody.AddForce(newForce, ForceMode2D.Impulse);

        StopCoroutine("fadeCoroutine");
        StartCoroutine("fadeCoroutine");

        if (gameEndsOnDefeat)
        {
            Global.isPaused = true;
            var totalShells = 0;
            Global.allPlayers.ForEach(p =>
            {
                p.isInputEnabled = false;
                p._rigidbody.velocity = new Vector2();
            });

            long[] chkNumber = Caches.NumbersForShells;
            if (false && chkNumber.Length > 0)
            {
                var tilemap = Caches.Tilemap;
                for (int y = 0; y < tilemap.objects.Length; y++)
                {
                    var temp = tilemap.objects[y];
                    for (int x = 0; x < temp.Length; x++)
                    {
                        long objectTile = temp[x];
                        if (Array.IndexOf(chkNumber, objectTile) >= 0)
                        {
                            totalShells++;
                        }
                    }
                }
            }

            if (totalShells == 0)
                totalShells = 42;

            var time = Global.playerController.state.time.FormatPlayTime();
            var collected = Global.playerController.state.shells + " / " + totalShells;
            var perc = Math.Round(Global.playerController.state.shells / ((float)totalShells) * 100f, 2);

            Global.hud.OpenModal(
                Translations.For("YouDidIt", time, collected, perc),
                onYes: () =>
                {
                    // var discordChannel = "https://discord.com/channels/711611331840835594/711611332398546957";
                    Application.OpenURL(Global.discordInvite);

                    Global.TransitionToScene("MainMenu", null, false, false);
                },
                onNo: () =>
                {
                    Global.TransitionToScene("MainMenu", null, false, false);
                }
            );

            var currentArea = "?";

            Global.LogGameplay("Defeated Boss " + name + " in " + currentArea
                                + " after " + time + ", collected: " + collected + " (" + perc + "%)");
        }
    }
}