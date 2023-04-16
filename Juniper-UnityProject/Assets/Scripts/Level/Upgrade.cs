using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class Upgrade : InteractableObject
{
    public TextFade textInfo;
    public ParticleSystem particles;
    internal bool collected = false;
    public UpgradeType upgradeType;

    public string myId;
    bool checkedCollected;
    
    void Start() {
        myId = "ux" + (int)transform.position.x + "y" + (int)transform.position.y;
    }

    public override void InitFromData(TableData data)
    {
        base.InitFromData(data);
        upgradeType = data.name.As<UpgradeType>();
    }

    void Update()
    {
        if(!checkedCollected && Global.playerController != null && Global.playerController.state.time != 0)
        {
            checkedCollected = true;
            if(Array.IndexOf(Global.playerController.state.collected, myId) >= 0)
            {
                collected = true;
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;
        //only exectue OnPlayerEnter if the player collides with this token.
        var player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
        	collected = true;
            particles.PlayIfNotPlaying();
            // give it player 1 anyway
            Global.playerController.ReceiveUpgrade(upgradeType, myId);
            renderer.gameObject.SetActive(false);
            if(textInfo != null && textInfo.gameObject) {
                textInfo.gameObject.SetActive(true);
                textInfo.FadeInForever();
            }
        }
    }
}