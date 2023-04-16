using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public enum BossBehaviourType
{
    LizardTwin = 0, MadSpider = 1, TwistedSnake = 2, TestBehavior = 3
}

[Serializable]
public abstract class BossBehaviourBase
{
    [HideInInspector]
    public GameObject gameObject;

    [HideInInspector]
    public BossController bossController;

    protected bool dead;
    public int bossDoors = 1;

    protected void OnDefeat()
    {
        var id = GetType().Name;
        var p1 = Global.playerController;
        if (Array.IndexOf(p1.state.defeated, id) == -1)
        {
            p1.AddDefeated(id);
        }
    }

    public virtual void Start()
    {
        unlockDoors();
    }

    public void lockDoors()
    {
        foreach (var door in UnityEngine.Object.FindObjectsOfType<BossDoor>())
        {
            door.lockDoor();
        }
    }

    public void unlockDoors()
    {
        foreach (var door in UnityEngine.Object.FindObjectsOfType<BossDoor>())
        {
            door.unlockDoor();
        }
    }

    /*protected bool hasFled(float? distOverride = null)
    {
        if (Global.playerController == null)
            return false;
        var dist = (Global.playerController.transform.position - bossController.transform.position).sqrMagnitude;
        Debug.Log("Dist current to " + this + " is " + Math.Sqrt(dist));
        return distOverride  == null ? dist > 1000f : dist > (distOverride * distOverride);
    }*/

    public float farAwayDist = 10f;

    protected bool tooFarAway()
    {
        return Global.playerController == null || (Global.playerController.transform.position - bossController.transform.position).sqrMagnitude > farAwayDist * farAwayDist;
    }

    public virtual IEnumerator Behaviour()
    {
        yield break;
    }

    public virtual IEnumerator Move(Vector3 start, Vector3 target, float speed, float waitBetweenSteps)
    {
        bossController.transform.position = start;
        var dist = (target - start).magnitude;

        var steps = (int)(Math.Abs(dist / speed == 0 ? 0.1f : speed) * 10);
        foreach (var i in Enumerable.Range(1, Math.Max(1, steps)))
        {
            var perc = (float)i / steps;
            bossController.transform.position = new Vector3(
                Mathf.Lerp(start.x, target.x, perc),
                Mathf.Lerp(start.y, target.y, perc),
                Mathf.Lerp(start.z, target.z, perc)
            );
            yield return new WaitForSeconds(waitBetweenSteps);
        }
    }

    public virtual void hurt(float damage)
    {

    }

    public virtual void OnTriggerExit2D(Collider2D collider)
    {

    }

    public virtual void OnTriggerEnter2D(Collider2D collider)
    {

    }

    public virtual void Update()
    {

    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (dead)
            return;

        var playerController = collision.collider.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.hurt(bossController.damageToPlayer);
        }
    }

    public virtual void die()
    {

    }
}

public class BossController : EnemyController
{
    protected BossBehaviourBase bossBehaviour;

    public LizardTwin _LizardTwin = new LizardTwin();
 
    public MadSpider _MadSpider = new MadSpider();

    public TwistedSnake _TwistedSnake = new TwistedSnake();
    [HideInInspector]
    public TestBehavior _TestBehavior = new TestBehavior();

    internal float fightTime;
    internal bool inFight;

    public BossBehaviourType bossBehaviourType = BossBehaviourType.LizardTwin;

    static Color transparent = new Color(a: 0, r: 1, g: 1, b: 1);

    internal float initialHealth;

    float origScale = 1f;
    public Text text;
    
    public void StartBossFight()
    {
        inFight = true;
        if(bossBehaviour == null)
        {
            ReloadBehaviour();
        }

        Global.hud.StartCoroutine(bossBehaviour.Behaviour());
    }

    public override void Start()
    {
        base.Start();
        initialHealth = health;

        ReloadBehaviour();

        changesMusic = false;

        random = new System.Random(name.Length + (int)Global.settings.profile * 100);

        origScale = transform.localScale.x;

        StartCoroutine(bossBehaviour.Behaviour());
    }

    void ReloadBehaviour()
    {
        bossBehaviour = _TestBehavior;
        try
        {
            var t = GetType().GetField("_" + bossBehaviourType.ToString());
            Global.LogDebug("Field for " + bossBehaviourType + " is " + t);
            var val = t.GetValue(this);
            Global.LogDebug("Field val " + val);
            bossBehaviour = (BossBehaviourBase)val;
            bossBehaviour.bossController = this;
            bossBehaviour.gameObject = gameObject;
            bossBehaviour.Start();
        }
        catch (Exception e)
        {
            Global.HandleError(e);
        }
    }

    public override void hurt(float damage)
    {
        if (health <= 0)
        {
            return;
        }
        base.hurt(damage);
        bossBehaviour.hurt(damage);
    }
    
    public new void Update()
    {
        if (Global.isPaused)
            return;

        bossBehaviour.Update();

        if (inFight)
            fightTime += Time.deltaTime;

        base.Update();
    }

    public void drawString(string textToDraw)
    {
        if (text != null && Global.isDebug)
            text.text = textToDraw;
    }

    public void setScaleX(float scaleX)
    {
        var vec = transform.localScale;
        vec.x = scaleX * scale;
        transform.localScale = vec;
    }

    public void drawInfo(string s)
    {
        if (Global.isDebug)
            drawString("anim " + currentAnimState + ", " + s);
    }

    public new void OnCollisionEnter2D(Collision2D collision)
    {
        bossBehaviour.OnCollisionEnter2D(collision);
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        bossBehaviour.OnTriggerExit2D(collider);
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        bossBehaviour.OnTriggerEnter2D(collider);
    }

    protected override void die()
    {
        bossBehaviour.die();
    }
}