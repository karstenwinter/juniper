using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
Ich werde jetzt die Bilder von den Nummern erklären
2. Skizze
6. Boss mit der Arena (3 platformen oben, 3 unten, 2 längere in der Mitte)
7. Attacke 1- Schießt gift direkt auf den Spieler 
4. Attacke 2- Geht von einer Seite auf die andere und attackiert gleichzeitig die unteren Platformen
5. Attacke 3- Schießt gift nach oben, fällt danach auf 3 random Platformen und bleibt da 3-5 Sekunden
1. Attacke 4- Springt von einer Seite zur anderen und attackiert gleichzeitig alle Platformen bis auf oben links, oben rechts und unten Mitte
3- Übergang zur Rage-Mode, zerstört die Mittleren Platformen und macht die zwei langen Platformen kürzer
Bei Fragen einfach fragen lol
 */
public enum ControlTwistedSnake
{
    Appear = -2,
    UseRandom = -1,
    ShootsPoisonToPlayer = 0,
    DashLower = 1,
    ShootPoisonUp = 2,
    DashMiddle = 3,
    RageMode = 4,
    Length = 5
}

[Serializable]
public class Phase
{
    public ControlTwistedSnake control;
    public string animationState;
    public PolygonCollider2D bossCollider;
    public EdgeCollider2D path;
    public bool showTail;

    public Vector2[] GetPathPoints()
    {
        return path == null ? null : Array.ConvertAll(path.points, p => (p + path.offset) * path.transform.localScale);
    }

    public Vector2[] GetColliderPoints()
    {
        var sc = 1; //  bossCollider.transform.localScale;
        return bossCollider == null ? null : Array.ConvertAll(bossCollider.points, p => (p + bossCollider.offset) * sc);
    }
}

[Serializable]
public class TwistedSnake : BossBehaviourBase
{
    static Color transparent = new Color(a: 0, r: 1, g: 1, b: 1);

    public Phase[] phases = new Phase[0];

    public GameObject projectilePrefab;

    public float projectileSpeed = 3f;
    public float dist = 5f;
    public float speed = 2;
    public int pathInterpolateSmoothness = 10;
    public float waitBetweenAttacks = 1f;
    public float waitAfterAttack = 1f;
    public float waitBeforeAttack = 1f;
    public float waitStart = 2f;
    public float waitBetweenSteps = 0.01f;

    public ControlTwistedSnake controlAttack = ControlTwistedSnake.Appear;
    private ControlTwistedSnake randomAttack;

    public TextFade challengeText;

    private PlayerController player;
    private Vector3 start, away;
    public Vector3 spawnShootOffset;
    public int phaseNum = 1;
    int playerHits;

    public int segmentDist;
    public GameObject[] segments = new GameObject[0];

    public GameObject[] destroyPlatformsPhase2 = new GameObject[0];

    public GameObject[] destroyPlatformsPhase3 = new GameObject[0];

    public GameObject[] shootUpTargets = new GameObject[0];
    public Vector3 shootUpOffset;
    public float shootUpSpeed = 4f;

    public float diffFactorForWait = 2f;

    public override void Start()
    {
        base.Start();
        start = bossController.transform.position;
        away = start + new Vector3(0, 32, 0);

        Action<float> onHurtCallback = (float damage) => bossController.hurt(damage);
        foreach (var item in segments)
        {
            item.GetComponent<Deco>().onHurtCallback = onHurtCallback;
        }
        bossController.renderer.color = transparent;
    }

    public override void OnTriggerEnter2D(Collider2D collider)
    {
        player = collider.gameObject.GetComponent<PlayerController>();
        if (bossController.inFight)
        {
            player.hurt(bossController.damageToPlayer);
            playerHits++;
        }
        else
        {
            challengeText.FadeIn();
        }
    }

    public override void OnTriggerExit2D(Collider2D collider)
    {
        player = null;
        challengeText.FadeOut();
    }

    public override void Update()
    {
        var tempPlayer = player;
        if (tempPlayer != null && tempPlayer.pressesUpOrDown && tempPlayer.isInputEnabled && !bossController.inFight)
        {
            bossController.StartBossFight();
        }
    }

    public IEnumerator MoveAlong(Vector2[] path)
    {
        if (speed == 0)
            yield break;

        var diff = Global.profile.difficulty.ToFloat();

        var tempSpeed = speed * diff;

        var beforeRota = bossController.transform.rotation;

        var interp = new List<Vector2>();
        for (int i = 1; i < path.Length; i++)
        {
            var p1 = path[i - 1];
            var p2 = path[i];
            var dist = (p1 - p2).magnitude;
            var newLen = dist * pathInterpolateSmoothness;
            for (int j = 0; j < newLen; j++)
            {
                interp.Add(new Vector2(
                    Mathf.Lerp(p1.x, p2.x, j / newLen),
                    Mathf.Lerp(p1.y, p2.y, j / newLen)
                ));
            }
        }

        for (int i = 0; i < interp.Count; i++)
        {
            var deltaPosBefore = interp[Math.Max(0, i - 1)];
            var deltaPos = interp[i];
            var pos = start + new Vector3(deltaPos.x, deltaPos.y, 0);

            for (int j = 0; j < segments.Length; j++)
            {
                var delta2Before = interp[Math.Max(0, i - (j + 1) * segmentDist)];
                var delta2 = interp[Math.Max(0, i - j * segmentDist)];
                var pos2 = start + new Vector3(delta2.x, delta2.y, 0);

                var angle = Mathf.Rad2Deg * (Mathf.Atan2(delta2.y - delta2Before.y, delta2.x - delta2Before.x));
                segments[j].transform.rotation = Quaternion.Euler(0, 0, angle);
                segments[j].transform.position = pos2;
            }

            if ((bossController.currentAnimState == "head" || bossController.currentAnimState == "up") && deltaPosBefore != deltaPos)
            {
                var angle = Mathf.Rad2Deg * (Mathf.Atan2(deltaPos.y - deltaPosBefore.y, deltaPos.x - deltaPosBefore.x));
                bossController.transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            bossController.transform.position = pos;

            if (i % (int)(diffFactorForWait * diff) == 0)
                yield return new WaitForSeconds(waitBetweenSteps);
        }
        bossController.transform.rotation = beforeRota;

        yield break;
    }

    public void drawInfo(string s)
    {
        bossController.drawInfo("attack: " + randomAttack + " " + s);
    }

    public override IEnumerator Behaviour()
    {
        while (Global.playerController == null || (Global.playerController.transform.position - bossController.transform.position).sqrMagnitude > 100f)
        {
            yield return new WaitForSeconds(1f);
        }
        var poly = bossController.GetComponent<PolygonCollider2D>();
        var oldPoints = poly.points;

        if (!bossController.inFight)
        {
            yield break;
        }
        else
        {
            bossController.transform.position = away;
            var player = Global.playerController;
            var oldMusic = Global.soundManager.EnterSequence("TwistedSnake");

            bossController.renderer.color = transparent;

            yield return new WaitForSeconds(waitStart);
            var grav = bossController._rigidbody.gravityScale;
            bossController._rigidbody.gravityScale = 0;
            // var distOverride = 66;
            while (bossController.health > 0) //  && !hasFled(distOverride)
            {

                if (Global.isPaused)
                {
                    yield return new WaitForSeconds(0.3f);
                    continue;
                }

                var playerX = player.transform.position.x;

                randomAttack = controlAttack;
                if (controlAttack == ControlTwistedSnake.UseRandom)
                    randomAttack = (ControlTwistedSnake)bossController.random.Next((int)ControlTwistedSnake.Length);

                if (bossController.health <= bossController.initialHealth * 0.66f && phaseNum == 1)
                {
                    phaseNum = 2;
                    Global.LogDebug("Boss snake phase2 " + bossController.health + "/" + bossController.initialHealth);
                    foreach (var item in destroyPlatformsPhase2)
                    {
                        item.SetActive(false);
                    }
                }

                if (bossController.health <= bossController.initialHealth * 0.33f && phaseNum == 2)
                {
                    Global.LogDebug("Boss snake phase3 " + bossController.health + "/" + bossController.initialHealth);
                    phaseNum = 3;
                    foreach (var item in destroyPlatformsPhase3)
                    {
                        item.SetActive(false);
                    }
                }

                Vector3? projectileSpawn = null;
                Vector3[] targets = new Vector3[0];

                IEnumerator enumerator;

                if (player.IsClimbing)
                    randomAttack = ControlTwistedSnake.ShootsPoisonToPlayer;

                bossController.setScaleX(1);

                switch (randomAttack)
                {
                    default:
                    case ControlTwistedSnake.DashLower:
                        break;
                    case ControlTwistedSnake.Appear:
                        break;
                    case ControlTwistedSnake.RageMode:
                        break;
                    case ControlTwistedSnake.DashMiddle:
                        break;
                    case ControlTwistedSnake.ShootPoisonUp:
                    case ControlTwistedSnake.ShootsPoisonToPlayer:
                        bossController.setScaleX(-1);
                        projectileSpawn = start + spawnShootOffset;
                        targets = new[] { player.transform.position };
                        break;
                }

                enumerator = Move(start, start, speed, waitBetweenSteps);

                var phase = Array.Find(phases, x => x.control == randomAttack);

                if (phase == null)
                    Global.LogDebug("found no phase for " + randomAttack);

                if (phase != null)
                {
                    foreach (var item in segments)
                    {
                        item.gameObject.SetActive(phase.showTail);
                    }

                    bossController.Activate(phase.animationState);

                    poly.points = phase.GetColliderPoints() ?? oldPoints;
                    var p = phase.GetPathPoints();
                    if (p != null)
                        enumerator = MoveAlong(p);
                }

                bossController.renderer.color = Color.white;
                bossController.transform.position = start;

                drawInfo("1 wait before");

                if (enumerator.MoveNext())
                    yield return enumerator.Current;

                yield return new WaitForSeconds(waitBeforeAttack);

                if (projectileSpawn != null)
                {
                    var currentProjSpeed = projectileSpeed;

                    if (randomAttack == ControlTwistedSnake.ShootPoisonUp)
                    {
                        currentProjSpeed = shootUpSpeed;
                        targets = Array.ConvertAll(shootUpTargets, x => x.transform.position);
                    }

                    foreach (var targetPos in targets)
                    {
                        GameObject projectileObj = GameObject.Instantiate(projectilePrefab, bossController.transform.position, bossController.transform.rotation);
                        projectileObj.transform.parent = bossController.transform.parent;

                        var delta = targetPos - start;
                        var angle = Mathf.Rad2Deg * (Mathf.Atan2(delta.y, delta.x));
                        projectileObj.transform.rotation = Quaternion.Euler(0, 0, angle);

                        Projectile projectile = projectileObj.GetComponentInChildren<Projectile>();
                        var direction = (targetPos - projectileSpawn.Value).normalized;
                        projectile.direction = direction;
                        projectile.movingSpeed = currentProjSpeed;

                        projectile.trigger();
                        yield return new WaitForSeconds(0.1f);
                    }

                    yield return new WaitForSeconds(0.2f);
                }

                if (controlAttack == ControlTwistedSnake.Appear)
                    controlAttack = ControlTwistedSnake.UseRandom;

                drawInfo("2 in progress");
                while (enumerator.MoveNext())
                    yield return enumerator.Current;

                drawInfo("3 wait between");
                yield return new WaitForSeconds(waitBetweenAttacks);

            }
            drawInfo("dead (coroutine)");
            foreach (var item in segments)
            {
                item.gameObject.SetActive(false);
            }

            bossController.renderer.color = Color.white;

            bossController._rigidbody.gravityScale = grav;

            bossController.damageToPlayer = 0;

            bossController.gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
            bossController.Activate("dead");
            bossController._rigidbody.bodyType = RigidbodyType2D.Dynamic;
            bossController.GetComponent<BoxCollider2D>().isTrigger = false;

            bossController.transform.position = player.transform.position;
            Global.LogGameplay("Boss defeated: TwistedSnake " + bossController.fightTime.FormatPlayTime() + ", player took hits " + playerHits);

            OnDefeat();
            Global.soundManager.Restore(oldMusic);
            unlockDoors();
        }
    }
}
