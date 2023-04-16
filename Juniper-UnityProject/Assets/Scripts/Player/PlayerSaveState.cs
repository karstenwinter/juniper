using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[Serializable]
public class PlayerSaveState
{
    public int health = 2, maxHealth = 2, healRate = 1;
    public int mana = 0, maxMana = 5;
    public int jumpCount = 1;
    public int amountNeededForHeal = 5;
    public float damageToEnemies = 1;
    public float damageToObstacles = 1;
    public float time;
    public Vector3 position = new Vector3(300f, -287.0262f, 0);
    public bool canProtection = false, canAttack = false, canDash = false, canMagic = false, canClimb = true, dashInvulnerable = false;
    public bool canShowMap = true;
    public bool minimapShowCamp = true, minimapShowShells, minimapShowBoss;
    public int percentage, shells, deaths;
    public string sceneName = "Scene", areaName = "UpperCave.PlayerStart", mode, checkpoint;

    public Difficulty difficulty = Difficulty.Normal;

    public string[] inventory = new string[0];
    public string[] defeated = new string[0];
    public string[] activated = new string[0];
    public string[] collected = new string[0];

    public string[] map = new string[0];

    public override string ToString()
    {
        if (time == 0)
            return "";

        var ts = TimeSpan.FromSeconds(time);
        return ts.ToString("m\\:ss\\.fff")
            + " • " + shells
            + " • " + percentage + "%";
    }
}
