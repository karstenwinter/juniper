using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ManaDisplay : MonoBehaviour
{
    public PlayerController player;
    public GameObject[] leafSteps;
    private Image[] leafStepsImages;

    private int lastMana, lastMax;

    void Start()
    {
        leafStepsImages = Array.ConvertAll(leafSteps, x => x.GetComponent<Image>());
        for (int i = 0; i < leafStepsImages.Length; ++i)
        {
            if(leafStepsImages[i] != null && leafStepsImages[i].gameObject != null) {
                leafStepsImages[i].gameObject.SetActive(false);
            }
        }
        if(player.playerIndex != 0)
        {
            transform.position += new Vector3(500, 0, 0);
        }
    }

    void Update()
    {
        var state = player.state;
        if(lastMax != state.maxMana || lastMana != state.mana) {
            lastMax = state.maxMana;
            lastMana = state.mana;

            int healthRemain = state == null ? 0 : state.health;
            
            for (int i = 0; i < leafStepsImages.Length; ++i)
            {
                if(leafStepsImages[i] != null && leafStepsImages[i].gameObject != null) {
                    leafStepsImages[i].gameObject.SetActive(lastMana == i + 1);
                }
            }
        }
    }
}
