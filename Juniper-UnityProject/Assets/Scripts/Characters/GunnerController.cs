using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerController : EnemyController
{
    public float shootInterval;
    public GameObject projectilePrefab;
    public float projectileSpeed = 0;
    public string projectileTone = "";

    public override void Start()
    {
        base.Start();
        Global.hud.StartCoroutine(Behaviour());
    }

    public Vector3 GetNearestPlayerPos()
    {
        var d = 1000000f;
        if (Global.playerController == null)
            return new Vector3();

        var nearestPlayer = Global.playerController.transform.position;
        
        Global.allPlayers.ForEach(p => {
            var distForCurrentPlayer = (transform.position - p.transform.position).magnitude;
            if (distForCurrentPlayer < d)
            {
                nearestPlayer = p.transform.position;
                d = distForCurrentPlayer;
            }
        });
        return nearestPlayer;
    }

    public override void Update()
    {
        base.Update();

        var deltaX = GetNearestPlayerPos().x - transform.position.x;
        int direction = Math.Sign(deltaX);

        if (direction != 0 && health > 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x = direction * scale;
            transform.localScale = newScale;
        }
    }

    public IEnumerator Behaviour()
    {
        while (health > 0)
        {
            yield return new WaitForSeconds(0.1f);
            Activate("idle");
            var delta = GetNearestPlayerPos() - transform.position;

            if (delta.sqrMagnitude < detectDistance * detectDistance)
            {
                var direction = delta.normalized;

                Quaternion rotation = transform.rotation;
                GameObject projectileObj = Instantiate(projectilePrefab, transform.position, rotation);
                projectileObj.transform.parent = transform.parent;
                Projectile projectile = projectileObj.GetComponent<Projectile>();
                projectile.direction = direction;
                var angle = Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x);
                projectileObj.transform.rotation = Quaternion.Euler(0, 0, angle);
                if (projectileSpeed != 0)
                {
                    projectile.movingSpeed = projectileSpeed;
                }

                Color parsed;
                if (ColorUtility.TryParseHtmlString(projectileTone, out parsed))
                {
                    var sp = projectile.GetComponent<SpriteRenderer>();
                    if (sp != null)
                        sp.color = parsed;
                }

                projectile.name = "Projectile " + direction + " speed " + projectileSpeed;
              
                projectile.trigger();
                Activate("attack");
                yield return new WaitForSeconds(shootInterval / 2);

                if (health > 0)
                {
                    Activate("idle");
                    yield return new WaitForSeconds(shootInterval / 2);
                }
            }
        }

        damageToPlayer = 0;

        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
        Activate("dead");
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        GetComponent<BoxCollider2D>().isTrigger = false;
    }
}
