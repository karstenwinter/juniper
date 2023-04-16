using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class Camp : InteractableObject
{
    public ParticleSystem particles;
    public TextFade text;

    private PlayerController player;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        player = collider.gameObject.GetComponent<PlayerController>();
        text.FadeIn();
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        player = null;
        text.FadeOut();
    }

    private void Update()
    {
        var tempPlayer = player;

        if (tempPlayer != null && tempPlayer.pressesUpOrDown && tempPlayer.isInputEnabled)
        {
            tempPlayer.isInputEnabled = false;
            tempPlayer.makeInvulnerable(changeColor: false);

            particles.PlayIfNotPlaying();

            var old = Global.hud.fadeFactor;
            Global.hud.fadeFactor = Global.hud.campFadeFactor;
            var oldT = Global.hud.inactivePlayerDuringFadeTime;
            Global.hud.inactivePlayerDuringFadeTime = Global.hud.campInactivePlayerDuringFadeTime;
            
            Global.FadeOutAndDo(() => {
                try
                {
                    Global.playerController.lastCheckpointPos = tempPlayer.transform.position;
                    tempPlayer.lastSafePosition = tempPlayer.transform.position;
                    tempPlayer.state.health = tempPlayer.state.maxHealth;
                    
                    Global.SaveGame();
                }
                catch (Exception e)
                {
                    Global.HandleError(e);
                }
                finally
                {
                    Global.FadeInAndDo(() => {
                        Global.hud.fadeFactor = old;
                        Global.hud.inactivePlayerDuringFadeTime = oldT;
                       //Global.hud.targetFadeOut = oldf;

                       tempPlayer.isInputEnabled = true;
                       tempPlayer.makeVulnerable(changeColor: false);
                    });
                }
            });
        } 
    }
}
