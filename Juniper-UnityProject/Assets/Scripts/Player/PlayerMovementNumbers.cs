using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[Serializable]
[CreateAssetMenu(fileName = "PlayerMovementNumbers", menuName = "PlayerMovementNumbers")]
public class PlayerMovementNumbers : ScriptableObject
{
    public float moveSpeed = 4, jumpSpeed = 6, climbSpeed = 3;
    public Vector2 climbJumpForce = new Vector2(9, 2);
    public float climbVelocity = -1;
    public float climbGravity = 0;
    public Vector2 climbExitForce = new Vector2();
    public float fallSpeed = 1, sprintSpeed = 10, sprintTime = 0.37f, sprintInterval = 0.6f, focusDelay = 1, focusInterval = 1;
    public Vector2 hurtRecoil = new Vector2(-4, 3);
    public float hurtTime = 0.4f, hurtRecoverTime = 1;
    public Vector2 deathRecoil = new Vector2(-4, 3);
    public float deathDelay = 0.3f;

    public Vector2 attackUpRecoil = new Vector2(0, -2f), attackForwardRecoil = new Vector2(-2, 0), attackDownRecoil = new Vector2(0, 7);
    public float attackDetectRadius = 0.6f;
    public float attackDetectDistance = 1.6f;
    public float attackEffectLifeTime = 0.05f;
    public float attackInterval = 0.34f;

    public Vector2 protectUpRecoil = new Vector2(0, -2f), protectForwardRecoil = new Vector2(-2, 0), protectDownRecoil = new Vector2(0, 7);
    public float protectDetectRadius = 0.6f;
    public float protectDetectDistance = 1.6f;
    public float protectEffectLifeTime = 0.05f;
    public float protectInterval = 0.34f;

    public float climbJumpDelay = 0.2f;
    public float rigidbodyGravityScale = 1.6f;
}
