using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class MadSpider : BossBehaviourBase
{
    public float max = 13f, max2 = 77f;
    public float facLeg = -1f;
    public float facAttackLeg = 1f;
    public float rotaXAttack = 0f;

    public float r, r2;
    public GameObject[] left, right, attacking;
    public float waitBetweenSteps = 0.08f;
    int playerHits;

    public override void Start()
    {
        base.Start();
        bossController.StartBossFight();
    }

    public override void OnTriggerEnter2D(Collider2D collider)
    {
        var player = collider.gameObject.GetComponent<PlayerController>();
        if (!dead && player != null)
        {
            player.hurt(bossController.damageToPlayer);
            playerHits++;
        }
    }

    public override IEnumerator Behaviour()
    {
        while (tooFarAway())
        {
            if (Global.playerController != null)
            {
                var dist2 =
                    Global.isDebug ?
                    (Global.playerController.transform.position - bossController.transform.position).magnitude
                    : -1;
                bossController.drawString("tooFarAway D " + dist2 + "," + Time.realtimeSinceStartup.FormatPlayTime());
            }
            else
                bossController.drawString("tooFarAway D null");

            yield return new WaitForSeconds(1f);
        }

        var oldMusic = Global.soundManager.EnterSequence("MadSpider");
        lockDoors();

        var dist =
            Global.isDebug ?
            (Global.playerController.transform.position - bossController.transform.position).magnitude
            : -1;
        bossController.drawString("started d " + dist);

        var start = bossController.transform.position;
        while (bossController.health > 0) //  && !hasFled()
        {
            r += facLeg;

            if (r >= max || r <= -max)
            {
                facLeg = -facLeg;
            }

            r2 += facAttackLeg;

            if (r2 >= max2 || r2 <= -max2)
            {
                facAttackLeg = -facAttackLeg;
                bossController.drawString("facAttackLeg flip " + facAttackLeg);
            }

            foreach (var item in left)
            {
                item.transform.rotation = Quaternion.Euler(0, 0, r);
            }
            foreach (var item in right)
            {
                item.transform.rotation = Quaternion.Euler(0, 0, -r);
            }

            attacking[0].transform.rotation = Quaternion.Euler(0, 0, r2);
            attacking[1].transform.rotation = Quaternion.Euler(0, 0, -r2);

            yield return new WaitForSeconds(waitBetweenSteps);
        }

        bossController.drawString("dead " + bossController.fightTime.FormatPlayTime());
        Global.LogGameplay("Boss defeated: MadSpider " + bossController.fightTime.FormatPlayTime() + ", player took hits " + playerHits);
        dead = true;
        foreach (var item in left)
        {
            item.SetActive(false);
        }
        foreach (var item in right)
        {
            item.SetActive(false);
        }
        foreach (var item in attacking)
        {
            item.SetActive(false);
        }

        bossController.damageToPlayer = 0;
        bossController.drawInfo("dead (coroutine)");

        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
        bossController.Activate("dead");
        bossController._rigidbody.bodyType = RigidbodyType2D.Dynamic;
        bossController.GetComponent<BoxCollider2D>().isTrigger = false;
        OnDefeat();
        Global.soundManager.Restore(oldMusic);
        unlockDoors();
    }
}
