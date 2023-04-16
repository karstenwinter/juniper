using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deadly : InteractableObject
{
    public bool instantKill = false;

    // private void OnCollisionEnter2D(Collision2D collision)
    private void OnTriggerEnter2D(Collider2D collider)
    {
        string layerName = LayerMask.LayerToName(collider.gameObject.layer);

        if (layerName == "Player" || layerName == "PlayerInvulnerable")
        {
            PlayerController playerController = collider.GetComponent<PlayerController>();
            if(playerController != null)
            { 
                if(layerName == "Player") {
                    playerController.hurt(instantKill ? playerController.state.health : 1);
                }
                if (playerController.state.health > 0)
                {
                    playerController.teleportToSafety();
                }
            }
        }
        else if (layerName == "Enemy")
        {
            EnemyController enemyController = collider.GetComponent<EnemyController>();
            enemyController?.hurt(enemyController.health);
        }
    }

    [ContextMenu("Set Collider size from Sprite")]
    void RefreshCollider() {
        var coll = GetComponent<BoxCollider2D>();
        var sprite = GetComponent<SpriteRenderer>();
        coll.size = sprite.size;
    }
    
    public override void hurt(float damage)
    {
        Global.soundManager.Play("obstacle");
    }
}
