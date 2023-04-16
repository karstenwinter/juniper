using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TestBehavior : BossBehaviourBase
{
    public float otherAttackRate;
}

public enum ControlTwinAttack
{
    UseRandom = -1,
    SprintHit = 0,
    SprintHit0 = 1,
    SprintHit1 = 2,
    FallDown = 3,
    FallDown0 = 4,
    HangLeft = 5,
    HangRight = 6,
    Length = 7
}

[Serializable]
public class LizardTwin : BossBehaviourBase
{
    static Color transparent = new Color(a: 0, r: 1, g: 1, b: 1);

    public float walkSpeed;

    public GameObject projectilePrefab;

    public float projectileSpeed = 3f;
    public float spawnSprintSideDistX = 3f;
    public float spawnSideDistX = 6.2f;
    public float spawnSideDistY = 4f;
    public float dist = 5f;
    public float speed = 2;
    public float waitBetweenAttacks = 1f;
    public float waitAfterAttack = 1f;
    public float waitBeforeAttack = 1f;
    public float waitStart = 2f;
    internal float waitBetweenSteps = 0.01f;

    public ControlTwinAttack controlAttack = ControlTwinAttack.UseRandom;
    private ControlTwinAttack randomAttack;

    public TextFade challengeText;

    public bool startedFight;

    public PlayerController player;
    private Vector3 start, away;
    int playerHits;

    public override void Start()
    {
        base.Start();
        start = bossController.transform.position;
        away = start + new Vector3(0, 16, 0);
    }

    public override void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "Player")
        {
            player = collider.gameObject.GetComponent<PlayerController>();
            if (startedFight)
            {
                player.hurt(bossController.damageToPlayer);
                playerHits++;
            }
            else
            {
                challengeText.FadeIn();
            }
        }
    }

    public override void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.tag == "Player")
        {
            player = null;
            challengeText.FadeOut();
        }
    }

    public override void Update()
    {
        var tempPlayer = player;

        if (tempPlayer != null && tempPlayer.pressesUpOrDown && tempPlayer.isInputEnabled && !bossController.inFight)
        {
            bossController.StartBossFight();

            bossController.gameObject.tag = "Enemy";
            bossController.gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
    }

    public void drawInfo(string s)
    {
        bossController.drawInfo("attack: " + randomAttack + " " + s);
    }

    public override IEnumerator Behaviour()
    {
        if (!bossController.inFight)
        {
            yield break;
        }
        else
        {
            var oldMusic = Global.soundManager.EnterSequence("LizardTwin");
            lockDoors();

            bossController.transform.position = away;

            var player = Global.playerController;
            bossController.renderer.color = transparent;

            yield return new WaitForSeconds(waitStart);
            var grav = bossController._rigidbody.gravityScale;
            bossController._rigidbody.gravityScale = 0;

            while (bossController.health > 0)
            {
                if (Global.isPaused)
                {
                    yield return new WaitForSeconds(0.3f);
                    continue;
                }

                var playerX = player.transform.position.x;

                randomAttack = controlAttack;
                if (this.controlAttack == ControlTwinAttack.UseRandom)
                    randomAttack = (ControlTwinAttack)bossController.random.Next((int)ControlTwinAttack.Length);

                var sprintFactorX = bossController.random.Next(2) == 0 ? 1 : -1;

                Vector3? projectileSpawn = null;
                IEnumerator enumerator;

                var idle = "idle";

                if (player.IsClimbing)
                    randomAttack = playerX > bossController.transform.position.x ? ControlTwinAttack.HangRight : ControlTwinAttack.HangLeft;

                switch (randomAttack)
                {
                    default:
                    case ControlTwinAttack.SprintHit:
                    case ControlTwinAttack.SprintHit0:
                    case ControlTwinAttack.SprintHit1:
                        bossController.Activate("dash");
                        bossController.setScaleX(sprintFactorX);
                        var startSprint = start + Vector3.right * sprintFactorX * spawnSprintSideDistX;
                        var endSprint = start + Vector3.right * -sprintFactorX * spawnSprintSideDistX;
                        enumerator = Move(startSprint, endSprint, speed, waitBetweenSteps);
                        break;
                    case ControlTwinAttack.FallDown:
                    case ControlTwinAttack.FallDown0:
                        bossController.setScaleX(1);
                        bossController.Activate("air");
                        var startFall = start;
                        startFall.x = Mathf.Clamp(start.x - spawnSprintSideDistX, playerX, start.x + spawnSprintSideDistX);
                        enumerator = Move(startFall + new Vector3(0, dist, 0), startFall, speed, waitBetweenSteps);
                        break;
                    case ControlTwinAttack.HangLeft:
                        bossController.setScaleX(1);
                        projectileSpawn = start + new Vector3(-spawnSideDistX, spawnSideDistY, 0);
                        bossController.Activate("climb");
                        enumerator = Move(projectileSpawn.Value, projectileSpawn.Value, speed, waitBetweenSteps);
                        break;
                    case ControlTwinAttack.HangRight:
                        bossController.setScaleX(-1);
                        projectileSpawn = start + new Vector3(spawnSideDistX, spawnSideDistY, 0);
                        bossController.Activate("climb");
                        enumerator = Move(projectileSpawn.Value, projectileSpawn.Value, speed, waitBetweenSteps);
                        break;
                }

                bossController.renderer.color = Color.white;
                bossController.transform.position = start;

                drawInfo("1 wait before");

                if (enumerator.MoveNext())
                    yield return enumerator.Current;

                yield return new WaitForSeconds(waitBeforeAttack);

                if (projectileSpawn != null)
                {
                    bossController.Activate("climb up");
                    GameObject projectileObj = GameObject.Instantiate(projectilePrefab, projectileSpawn.Value, bossController.transform.rotation);
                    projectileObj.transform.parent = bossController.transform.parent;
                    Projectile projectile = projectileObj.GetComponent<Projectile>();
                    var direction = (player.transform.position - projectileSpawn.Value).normalized;
                    var angle = Mathf.Rad2Deg * (Mathf.Atan2(direction.y, direction.x));
                    projectileObj.transform.rotation = Quaternion.Euler(0, 0, angle);

                    projectile.direction = direction;
                    projectile.movingSpeed = projectileSpeed;

                    projectile.trigger();

                    yield return new WaitForSeconds(0.5f);

                    idle = "climb";
                    bossController.Activate(idle);
                }

                drawInfo("2 in progress");
                while (enumerator.MoveNext())
                    yield return enumerator.Current;

                bossController.Activate(idle);

                drawInfo("3 wait after");
                yield return new WaitForSeconds(waitAfterAttack);

                bossController.transform.position = away;

                drawInfo("4 wait between");
                yield return new WaitForSeconds(waitBetweenAttacks);
            }

            Global.LogGameplay("Boss defeated: LizardTwin " + bossController.fightTime.FormatPlayTime() + ", player took hits " + playerHits);
            bossController.Activate("dead");
            bossController._rigidbody.gravityScale = grav;
            OnDefeat();
            Global.soundManager.Restore(oldMusic);
            unlockDoors();

        }
    }
}