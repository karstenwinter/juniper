using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : NPC
{
    public ParticleSystem particles;
    bool collected = false;

    protected override void Start()
    {
        base.Start();

        text = Translations.For("Received") + ": " + Translations.For(gameObject.name);

        if(Array.IndexOf(Global.playerController.state.collected, data?.name) >= 0)
        {
            SetCollected();   
        }
    }

    void SetCollected()
    {
        gameObject.SetActive(false);
        collected = true;
    }

    protected override void OnInteraction() 
    {
        if(collected)
            return;
        
        SetCollected();
        particles.PlayIfNotPlaying();
        
        Global.hud.OpenModal(
            text,
            onOk: () => {
                // onYes?.Invoke();

                // if(tempPlayer != null)
                //     tempPlayer.isInputEnabled = true;
            }
        );
        Global.playerController.state.collected = new List<string>(Global.playerController.state.collected) { data?.name }.ToArray();
        // TODO: place map differently
        Global.playerController.ReceiveUpgrade(UpgradeType.Map);
    }
}
