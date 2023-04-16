using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : InteractableObject
{
    public float curDist = 0;
    public float distToInteract = 1f;
    public GameObject mark;
    public string text = "";
    public Action onYes, onNo;

    [ContextMenu("Load")]
    protected virtual void Start()
    {
        canBounceOff = false;
        if (data == null)
            data = Array.Find(Caches.TableData, x => x.name == gameObject.name);

        Debug.Log("Start for " + gameObject.name + ", data: " + data?.ToJson());
        text = data?.data != null ? Translations.For(data?.data) : "MISSING, DEFAULT: Hi, I'm " + gameObject.name + ".";
    }

    void Update()
    {
        var tempPlayer = Global.playerController;
        var playerPos = tempPlayer?.transform?.position ?? new Vector3();
        var showDist = distToInteract * distToInteract;
        curDist = (transform.position - playerPos).sqrMagnitude;
        if (curDist < showDist)
        {
            mark.SetActive(true);
            if (tempPlayer != null && tempPlayer.pressesUpOrDown && tempPlayer.isInputEnabled)
            {
                OnInteraction();
            }
        }
        else
        {
            if (mark.activeInHierarchy)
            {
                mark.SetActive(false);
                Global.hud.CloseModal();
            }
        }
    }

    protected virtual void OnInteraction()
    {
        Global.hud.OpenModal(
            text,
            onOk: () =>
            {

            }
        );
    }
}
