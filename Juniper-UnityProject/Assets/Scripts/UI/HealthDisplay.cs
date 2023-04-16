using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    public PlayerController player;
    public GameObject[] hearts;
    public ParticleSystem healParticles, damageParticles, maxHpUpParticles;

    public Sprite healthFull;
    public Sprite healthEmpty;
    public Text respawnText;

    private Image[] _hearts;
    int lastHealth, lastMax;

    void Start()
    {
        _hearts = Array.ConvertAll(hearts, x => x.GetComponent<Image>());

        for (int i = 0; i < _hearts.Length; ++i)
        {
            if (_hearts[i] != null && _hearts[i].gameObject != null)
            {
                _hearts[i].gameObject.SetActive(false);
            }
        }
        if (player.playerIndex != 0)
        {
            transform.position += new Vector3(500, 0, 0);
        }
    }

    void Update()
    {
        var state = player.state;
        if (lastMax != state.maxHealth || lastHealth != state.health)
        {
            var damage = state.health < lastHealth;
            var heal = state.health > lastHealth;
            var maxHpChanged = state.maxHealth != lastMax;

            lastMax = state.maxHealth;
            lastHealth = state.health;

            int healthRemain = state == null ? 0 : state.health;

            for (int i = 0; i < _hearts.Length; ++i)
            {
                if (_hearts[i] != null && _hearts[i].gameObject != null)
                {
                    _hearts[i].gameObject.SetActive(i < state.maxHealth);
                    _hearts[i].sprite = i < healthRemain ? healthFull : healthEmpty;
                }
            }

            if (maxHpChanged)
            {
                maxHpUpParticles.PlayIfNotPlaying();
            }

            if (damage)
            {
                damageParticles.PlayIfNotPlaying();
            }

            if (heal)
            {
                healParticles.PlayIfNotPlaying();
            }
        }
    }
}
