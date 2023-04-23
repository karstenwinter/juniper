using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class Ext
{
    public static bool ReadButton(this InputAction inputAction)
    {
        return inputAction.ReadValue<float>() != 0f;
    }

    public static float ReadAxis(this float value)
    {
        if (Math.Abs(value) < Global.settings.inputThreshold.ToFloat())
        {
            return 0;
        }
        return Mathf.Sign(value);
    }
}

// Props to https://github.com/DanielDFY/Hollow-Knight-Imitation
// Helped a lot in the beginning

public class PlayerController : MonoBehaviour
{
    // New Input System
    internal float Horizontal, Vertical;
    internal bool Jump, Attack, AttackMelee, Map, Magic, Pause, Dash;
    internal bool BtnScaleUp;

    public bool hasContactLeft;
    public bool hasContactRight;
    public bool hasContactBottom = true;

    public void OnMap(InputAction.CallbackContext context)
    {
        Map = context.action.ReadButton();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump = context.action.ReadButton();
        if (Global.isDebug && text != null)
            text.text = "J " + Jump.ToString();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log("OnAttack");
        Attack = context.action.ReadButton();
    }

    public void OnMagic(InputAction.CallbackContext context)
    {
        Magic = context.action.ReadButton();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        Dash = context.action.ReadButton();
    }

    public void OnBtnScaleUp(InputAction.CallbackContext context)
    {
        BtnScaleUp = context.action.ReadButton();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
            Pause = true;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var v = context.action.ReadValue<Vector2>();
        Horizontal = v.x.ReadAxis();
        Vertical = v.y.ReadAxis();

        if (Global.isDebug && text != null)
            text.text = "M " + v;
    }
    // END New Input System

    public ParticleSystem jumpParticles, landParticles, protectParticleSystem;
    public ParticleSystem dustParticleSystem, dustClimbParticleSystem, hitParticleSystem, gotHitParticleSystem, sporeParticles, healParticles, healParticles2, dashParticles;

    public Text text;
    public Canvas textCanvas;
    public Image textImage;

    public PlayerSaveState state = new PlayerSaveState();

    public PlayerMovementNumbers values;
    public int jumpLeft;
    public Color invulnerableColor;
    public float verticalDirection;

    public GameObject attackUpEffect, attackForwardEffect, attackDownEffect, focusEffect, lastSaveSpot;
    public GameObject takeWithMe;

    public bool isClimb, isDashPossible, isDashReset,
                isAttackable, isFocussing, isMagicReset,
                // currently doing:
                isDashing, isJumping, isAttacking, isProtectable, isProtecting;

    RaycastHit2D[] hitRecList = new RaycastHit2D[16];

    public bool IsClimbing { get { return isClimb; } }
    float verticalVelocity { get { return _rigidbody.velocity.y; } }
    float horizontalVelocity { get { return _rigidbody.velocity.x; } }

    public bool isInputEnabled, pressesUpOrDown;

    public float climbToStandDuration = 0.5f;
    public float minEnemyDistance = 2f;
    public float detectRadiusGround = 0.1f;
    public float detectDistanceGround = 0.5f;
    public float walkPlayFreq = 0.2f;
    public float detectDistance = 0.4f;
    public float maxVelocity = 22f;

    public AnimatedObject _animator;
    public Rigidbody2D _rigidbody;
    public SpriteRenderer _spriteRenderer;

    public Vector3 lastCheckpointPos;
    public Vector3 lastSafePosition { get { return lastSaveSpot.transform.position; } set { lastSaveSpot.transform.position = value; } }
    protected float walkTimer;
    protected bool walkPlayLeft;
    protected float textCanvasOrigXScale;

    public int playerIndex = 0;
    static float multiplayerWaitToRespawn = 5f;

    public Text waitTillRespawn;

    void OnDisable()
    {
        Global.LogDebug("Clean up player with index " + playerIndex);
        Global.allPlayers.Remove(this);

        if (playerIndex == 0)
            Global.playerController = null;
    }

    void SetFollow(Transform followTransform)
    {
        var c = GameObject.Find("MainCamera");
        Debug.Log("MainCamera: " + c);
        if (c != null)
        {
            var vc = c.GetComponent<CinemachineVirtualCamera>();
            if (vc != null)
            {
                vc.m_Follow = followTransform;
                Debug.Log("MainCamera VC: " + vc);
            }
        }
    }

    void Start()
    {
        if (values == null)
        {
            values = Resources.Load<PlayerMovementNumbers>("PlayerMovementNumbers");
        }

        var globalPlayerPos = GameObject.Find("GlobalPlayerStart");
        if (globalPlayerPos != null)
            transform.position = globalPlayerPos.transform.position;

        playerIndex = GetComponent<PlayerInput>().playerIndex;
        if (playerIndex != 0)
        {
            gameObject.name = "Gecko";
        }
        else
        {
            gameObject.name = "GeckoP2";
            Global.playerController = this;

            if (Global.playerNeedsToLoadData && !Global.inTransition)
            {
                SaveSystem.Load(Global.settings.profile, withMapAndRefresh: false);
            }

            GenPrefabs.SetPlayer(transform);

            SetFollow(transform);
            var fader = transform.Find("Fader");
            if (fader != null)
            {
                fader.gameObject.SetActive(true);

                if (Global.hud.characterFader.gameObject != null)
                    Global.hud.characterFader.gameObject.SetActive(false);

                Global.hud.characterFader = fader.gameObject;

                Global.hud.FadeIn(() => { });
            }
        }

        if (!Global.allPlayers.Contains(this))
            Global.allPlayers.Add(this);

        if (Global.hud.isBlack)
            Global.hud.UiFadeIn(() =>
            {
                var any = GameObject.Find("AnyKey");
                if (any != null)
                    any.SetActive(false);
            });

        lastCheckpointPos = transform.position;

        isDashReset = true;
        isMagicReset = true;
        isAttackable = true;
        isProtectable = true;

        _rigidbody.gravityScale = values.rigidbodyGravityScale;

        textCanvasOrigXScale = textCanvas.transform.localScale.x;

        if (Global.playerNeedsToLoadData && !Global.inTransition)
        {
            SaveSystem.Load(Global.settings.profile);
            Global.playerNeedsToLoadData = false;
            if (playerIndex != 0)
            {
                transform.position = Global.playerController.transform.position;
            }
        }
        lastSafePosition = transform.position;
    }

    void Update()
    {
        // Debug.Log("Update A" + Attack);
        takeWithMe.transform.position = transform.position;

        updatePlayerState();
        if (isInputEnabled && !Global.isPaused)
        {
            move();

            if (Jump)
                jumpOrClimbJump();

            if (Dash)
                dash();

            if (Attack)
                protection();

            if (AttackMelee)
                attack();

            if (Magic)
                magic();

            state.time += Time.deltaTime;
        }
        if (Global.settings.autoFireButtons == OffOn.Off)
        {
            Jump = false;
            Attack = false;
            Dash = false;
        }
    }

    private void protection()
    {
        if (!(!Global.isPaused && state.canProtection && !isClimb && isProtectable))
            return;

        verticalDirection = Vertical;
        if (verticalDirection > 0)
            protectUp();
        else if (verticalDirection < 0 && !hasContactBottom)
            protectDown();
        else
        {
            protectForward();
        }
    }

    public void jumpOrClimbJump()
    {
        if (Global.isPaused)
            return;

        if (isClimb && !hasContactBottom)
            climbJump();
        else if (jumpLeft > 0 && hasContactBottom && !isClimb)
            jump();
    }

    void drawString(string textToDraw)
    {
        var scale = textCanvas.transform.localScale;
        scale.x = transform.localScale.x * textCanvasOrigXScale;
        textCanvas.transform.localScale = scale;
        text.text = textToDraw;
        textImage.enabled = textToDraw != "";
    }

    IEnumerator enableInputAgain()
    {
        yield return new WaitForSeconds(climbToStandDuration);
        isInputEnabled = true;

        if (!isJumping)
        {
            _rigidbody.velocity =
                new Vector3(
                    -transform.localScale.x * values.climbExitForce.x,
                    0,
                    0);
        }
    }

    void refreshAnimation()
    {
        const float almostZero = 0.01f;
        if (!isDashing && !isAttacking && !isProtecting && isInputEnabled)
        {
            var st = isClimb ?
                    verticalVelocity > almostZero ? "climb up"
                    : verticalVelocity < -almostZero ? "climb down"
                    : "climb"
                : hasContactBottom ?
                    Math.Abs(horizontalVelocity) < almostZero ? "idle" : "run"
                : verticalVelocity > almostZero ? "jump"
                : verticalVelocity < -almostZero ? "air"
                : "idle";
            _animator.Activate(st);
        }
    }
    bool isWallNear()
    {
        return transform.localScale.x > 0 ? hasContactLeft : hasContactRight;
    }

    public void makeInvulnerable(bool changeColor = true)
    {
        if (changeColor)
            _spriteRenderer.color = invulnerableColor;

        gameObject.layer = LayerMask.NameToLayer("PlayerInvulnerable");
    }

    public void makeVulnerable(bool changeColor = true)
    {
        if (changeColor)
            _spriteRenderer.color = Color.white;

        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    public void hurt(int damage)
    {
        gotHitParticleSystem.Play();

        makeInvulnerable();

        state.health = Math.Max(state.health - damage, 0);

        rumbleGamepadGeckoHit();

        if (state.health <= 0)
        {
            die(true);
            return;
        }

        // enter invulnerable state
        _animator.Activate("land");

        // stop player movement
        var newVelocity = new Vector2(0, 0);
        _rigidbody.velocity = newVelocity;


        var newForce = new Vector2(
            -transform.localScale.x * values.hurtRecoil.x,
            values.hurtRecoil.y);
        _rigidbody.AddForce(newForce, ForceMode2D.Impulse);

        isInputEnabled = false;

        StopCoroutine("recoverFromHurtCoroutine");
        StartCoroutine("recoverFromHurtCoroutine");
    }

    protected IEnumerator recoverFromHurtCoroutine()
    {
        var old = Global.hud.timeScale;

        yield return new WaitForSecondsRealtime(values.hurtTime);

        isInputEnabled = true;
        yield return new WaitForSeconds(values.hurtRecoverTime);

        makeVulnerable();
    }

    public bool noDamagingEntitiesNear()
    {
        var sqDist = minEnemyDistance * minEnemyDistance;
        var enemies = GameObject.FindObjectsOfType<Enemy>();
        if (Array.Exists(enemies, en => (en.transform.position - transform.position).sqrMagnitude < sqDist))
            return false;

        var traps = GameObject.FindObjectsOfType<Deadly>();
        if (Array.Exists(traps, trap => (trap.transform.position - transform.position).sqrMagnitude < sqDist))
            return false;

        return true;
    }

    public float frictionOnIdle = 4f;
    protected void updatePlayerState()
    {
        var lastGrounded = hasContactBottom;

        var lastIsClimb = isClimb;
        isClimb = isWallNear();

        if (isClimb && isJumping)
            isJumping = false;

        var groundedAndNoInput = (hasContactBottom && Horizontal == 0 && !Dash && !Jump && !isJumping);
        if (isClimb || isDashing || groundedAndNoInput)
        {
            if (groundedAndNoInput)
                _rigidbody.velocity = new Vector2();

            _rigidbody.gravityScale = 0; // dont move on slopes when no input is done
        }
        else
        {
            _rigidbody.gravityScale = values.rigidbodyGravityScale;
        }

        if (_rigidbody.gravityScale == 0 && isJumping)
        {
            _rigidbody.gravityScale = values.rigidbodyGravityScale;
        }

        if (!lastGrounded && hasContactBottom)
        {
            if (noDamagingEntitiesNear())
            {
                lastSafePosition = transform.position;
            }
            Global.soundManager.Play("land");

            landParticles.PlayIfNotPlaying();
            _animator.Activate("land", true);
        }
        refreshAnimation();

        if (hasContactBottom)
        {
            jumpLeft = state.jumpCount;

            isDashPossible = true;
        }
        else
        {
            if (isClimb)
            {
                isDashPossible = true;
                // one remaining jump chance after climbing
                jumpLeft = state.jumpCount - 1;
            }
            else if (state.jumpCount == 1)
            {
                jumpLeft = 0; // no coyote
            }
        }

        var velocity = _rigidbody.velocity;
        if (Math.Abs(velocity.y) > maxVelocity)
        {
            velocity.y = Math.Sign(_rigidbody.velocity.y) * maxVelocity;
            _rigidbody.velocity = velocity;
        }
    }

    protected void move()
    {
        var hor = Horizontal;
        var vert = Vertical;

        pressesUpOrDown = vert != 0;

        // calculate movement
        float horizontalMovement = hor * values.moveSpeed;

        var oldHor = _rigidbody.velocity.x;
        // set velocity

        if (isClimb && isWallNear())
        {
            if (state.canClimb)
            {
                if ((transform.localScale.x < 0 && hasContactRight && hor > 0)
                    || (transform.localScale.x > 0 && hasContactLeft && hor < 0))
                {
                    vert = Mathf.Clamp(vert + Math.Abs(hor), -1, 1);
                    horizontalMovement = 0;
                }

                if (vert != 0)
                    CreateDust();

                var newVelocity = new Vector2(
                    horizontalMovement,
                    vert * values.climbSpeed);
                _rigidbody.velocity = newVelocity;
            }
        }
        else
        {
            var newVelocity = new Vector2(
                horizontalMovement,
                _rigidbody.velocity.y);
            _rigidbody.velocity = newVelocity;
            // the sprite itself is inversed 
            float moveDirection = -transform.localScale.x * horizontalMovement;

            if (moveDirection < 0)
            {
                // flip player sprite
                var newScale = new Vector3(
                    horizontalMovement < 0 ? 1 : -1,
                    1,
                    1);

                transform.localScale = newScale;

                if (hasContactBottom)
                {
                    // turn back animation
                    _animator.Activate("turn");
                }
            }

            if (hasContactBottom && moveDirection != 0)
            {
                walkTimer += Time.deltaTime;
                if (walkTimer > walkPlayFreq)
                {
                    walkTimer = 0;
                    walkPlayLeft = !walkPlayLeft;

                    Global.soundManager.Play(
                        walkPlayLeft ? "walkl" : "walkr"
                    );
                }
            }
        }


        if (hor != 0 && hasContactBottom)
        {
            CreateDust();
        }
    }

    public void teleportToSafety()
    {
        Debug.Log("playerController.teleportToSafety");
        StartCoroutine(dieAndRestorePosTo(false, () => lastSafePosition));
    }

    protected void die(bool incDeaths)
    {
        Global.hud.ShakeCamera(0.09f, values.deathDelay / 2f);
        _animator.Activate("air");

        isInputEnabled = false;

        _rigidbody.velocity = new Vector2();

        _spriteRenderer.color = invulnerableColor;

        StartCoroutine(deathCoroutine(incDeaths && state.health <= 0));
    }

    protected IEnumerator deathCoroutine(bool incDeaths)
    {
        if (Global.allPlayers.Count < 2)
        {
            reallyDie(incDeaths);
        }
        else if (playerIndex != 0)
        {
            return dieAndRestorePosTo(incDeaths, () => Global.playerController.lastSafePosition);
        }
        else
        {
            foreach (var item in Global.allPlayers)
            {
                if (item != this && item.state.health > 0)
                {
                    var fader = transform.Find("Fader");
                    if (fader != null)
                    {
                        fader.gameObject.SetActive(false);
                    }
                    SetFollow(item.transform);
                    return dieAndRestorePosTo(incDeaths, () => item.lastSafePosition, () =>
                    {
                        SetFollow(transform);
                        if (fader != null)
                        {
                            fader.gameObject.SetActive(true);
                        }
                    });
                }
            }
            reallyDie(incDeaths);
        }
        return new string[0].GetEnumerator();
    }


    void reallyDie(bool incDeaths)
    {
        //Global.hud.UiFadeOut(() =>
        //{
        try
        {
            if (incDeaths)
            {
                SaveSystem.Load(Global.settings.profile, incDeaths: incDeaths);
            }
            else
            {
                _animator.Activate("idle");
                transform.position = lastSafePosition;
            }
        }
        catch (Exception e)
        {
            Global.HandleError(e);
        }
        finally
        {
            //Global.hud.UiFadeIn(() =>
            //{
            makeVulnerable();
            isInputEnabled = true;
            // });
        }
        //});
    }

    bool allDead()
    {
        return Global.allPlayers.All(x => x.state.health <= 0);
    }

    IEnumerator dieAndRestorePosTo(bool wait, Func<Vector3> pos, Action a = null)
    {
        var c = _spriteRenderer.color;
        makeInvulnerable();
        isInputEnabled = false;
        c.a = 0;
        _spriteRenderer.color = c;
        bool dead = false;
        if (wait)
        {
            var timer = multiplayerWaitToRespawn;
            waitTillRespawn.text = timer.ToString("0.00") + "s";
            var delta = multiplayerWaitToRespawn / 20;
            for (int i = 0; i < 20; i++)
            {
                timer -= delta;
                waitTillRespawn.text = timer.ToString("0.00") + "s";
                if (dead = allDead())
                {
                    break;
                }
                yield return new WaitForSeconds(delta);
            }
            waitTillRespawn.text = "";
        }
        state.health = Math.Max(state.health, 1);

        transform.position = pos();

        c.a = 1;
        _spriteRenderer.color = c;

        makeVulnerable();
        isInputEnabled = true;

        if (a != null)
            a();

        if (dead)
            reallyDie(true);
    }
    protected void jump()
    {
        isJumping = true;
        Global.soundManager.Play("jump");

        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, values.jumpSpeed);

        _animator.Activate("jump");
        if (hasContactBottom)
        {
            jumpParticles.PlayIfNotPlaying();
        }
        StartCoroutine(climbJumpCoroutine(values.climbJumpDelay));
    }

    protected void climbJump()
    {
        transform.localScale = new Vector3(
            -transform.localScale.x,
            1,
            1
        );

        isJumping = true;
        Global.soundManager.Play("jump");

        _rigidbody.velocity = new Vector2(
            values.climbJumpForce.x * transform.localScale.x,
            values.climbJumpForce.y);

        isInputEnabled = false;
        _animator.Activate("climb to jump", true);
        StartCoroutine(climbJumpCoroutine(values.climbJumpDelay));
    }

    protected IEnumerator climbJumpCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        isJumping = false;
        isInputEnabled = true;
    }

    protected void fall()
    {
        if (_rigidbody.velocity.y > 0)
        {
            var newVelocity = new Vector2(
                _rigidbody.velocity.x,
                _rigidbody.velocity.y / 2);
            _rigidbody.velocity = newVelocity;
        }
    }

    public void dash()
    {
        if (!(!Global.isPaused && state.canDash && isDashPossible && isDashReset))
            return;

        Global.soundManager.Play("dash");
        var sc = dashParticles.transform.localScale;
        sc.x = transform.localScale.x;
        dashParticles.transform.localScale = sc;
        dashParticles.PlayIfNotPlaying();

        // reject input during sprinting
        isInputEnabled = false;
        isDashPossible = false;
        isDashReset = false;
        isDashing = true;

        var newVelocity = new Vector2(
            transform.localScale.x * (isClimb ? values.sprintSpeed : -values.sprintSpeed),
            0);
        _rigidbody.velocity = newVelocity;

        if (isClimb)
        {
            // sprint to the opposite direction
            var newScale = new Vector3(
                -transform.localScale.x,
                1,
                1);

            transform.localScale = newScale;
        }

        StartCoroutine(sprintCoroutine(values.sprintTime, values.sprintInterval));
    }

    protected IEnumerator sprintCoroutine(float sprintDelay, float sprintInterval)
    {
        _animator.Activate("dash", true);
        if (state.dashInvulnerable)
            makeInvulnerable();

        _rigidbody.gravityScale = 0;
        yield return new WaitForSeconds(sprintDelay);

        if (state.dashInvulnerable)
            makeVulnerable();

        if (!isClimb && _rigidbody.gravityScale == 0)
            _rigidbody.gravityScale = values.rigidbodyGravityScale;

        isInputEnabled = true;
        isDashing = false;
        yield return new WaitForSeconds(0.001f);

        yield return new WaitForSeconds(sprintInterval);
        isDashReset = true;
    }

    void CreateDust()
    {
        if (IsClimbing)
            dustClimbParticleSystem.PlayIfNotPlaying();
        else
            dustParticleSystem.PlayIfNotPlaying();
    }

    void PlayGotHit()
    {
        gotHitParticleSystem.PlayIfNotPlaying();
    }

    protected IEnumerator focusCoroutine()
    {
        isFocussing = true;
        state.mana -= state.amountNeededForHeal;
        isMagicReset = false;
        healParticles.PlayIfNotPlaying();
        yield return new WaitForSeconds(values.focusDelay);
        isInputEnabled = true;
        state.health = Math.Min(state.maxHealth, Math.Max(state.health + state.healRate, 0));
        healParticles2.PlayIfNotPlaying();

        yield return new WaitForSeconds(values.focusInterval);
        isMagicReset = true;
        isFocussing = false;
    }

    public void attack()
    {
        Debug.Log("attack: " + Global.isPaused + "" + state.canAttack + "" + !isClimb + "" + isAttackable);

        if (!(!Global.isPaused && state.canAttack && !isClimb && isAttackable))
            return;

        verticalDirection = Vertical;
        if (verticalDirection > 0)
            attackUp();
        else if (verticalDirection < 0 && !hasContactBottom)
            attackDown();
        else
        {
            hitParticleSystem.PlayIfNotPlaying();
            attackForward();
        }

    }

    public void magic()
    {
        if (!(!Global.isPaused && state.canMagic && !isClimb && isMagicReset && !isFocussing))
            return;

        if (hasContactBottom && state.mana >= state.amountNeededForHeal)
        {
            Global.soundManager.Play("magic");
            StartCoroutine(focusCoroutine());
        }
    }

    protected void protectUp()
    {
        Debug.Log("protectUp");
        protectParticleSystem.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
        protectParticleSystem.PlayIfNotPlaying();

        var detectDirection = new Vector2(0, 1);

        _animator.Activate("hit up", true);
        StartCoroutine(protectCoroutine(detectDirection, values.attackUpRecoil));
    }

    protected void protectForward()
    {
        Debug.Log("protectForward");
        protectParticleSystem.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Math.Sign(transform.localScale.x) == -1 ? 180 : 0));
        protectParticleSystem.PlayIfNotPlaying();

        var detectDirection = new Vector2(-transform.localScale.x, 0);

        var recoil = new Vector2(
            transform.localScale.x > 0 ? -values.attackForwardRecoil.x : values.attackForwardRecoil.x,
            values.attackForwardRecoil.y);

        _animator.Activate("hit", true);
        StartCoroutine(protectCoroutine(detectDirection, recoil));
    }

    protected void protectDown()
    {
        Debug.Log("protectDown");
        protectParticleSystem.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
        protectParticleSystem.PlayIfNotPlaying();

        var detectDirection = new Vector2(0, -1);

        _animator.Activate("hit down", true);
        StartCoroutine(protectCoroutine(detectDirection, values.protectDownRecoil));
    }

    protected IEnumerator protectCoroutine(Vector2 detectDirection, Vector2 attackRecoil)
    {
        // attack cool down
        isProtectable = false;
        isProtecting = true;
        Vector2 origin = transform.position;
        origin += detectDirection * 0.1f; // no radius behind player pos 
        LayerMask layerMask =
            LayerMask.GetMask("Enemy")
        | LayerMask.GetMask("Interactable")
        | LayerMask.GetMask("Platform")
        | LayerMask.GetMask("Deco");

        int len = Physics2D.CircleCastNonAlloc(origin, values.protectDetectRadius, detectDirection, hitRecList, values.protectDetectDistance, layerMask);

        if (Global.isDebug)
            Debug.Log("protectCoroutine " + detectDirection + "," + attackRecoil + "" + String.Join(", ", Array.ConvertAll(hitRecList, x => x.transform.gameObject.name)));

        var canBounceOff = hitRecList.Length > 0;

        float shake = 0;
        shake = 0.05f;

        Global.soundManager.Play("protect");

        for (var i = 0; i < len; i++)
        {
            RaycastHit2D hitRec = hitRecList[i];
            GameObject obj = hitRec.collider.gameObject;

            string layerName = LayerMask.LayerToName(obj.layer);

            var controller = obj.GetComponent<InteractableObject>();
            if (controller != null)
            {
                if (!controller.canBounceOff)
                {
                    canBounceOff = false;
                    shake = 0f;
                }

                controller.playerController = this;
                controller.protectionSpell(layerName == "Enemy" ? state.damageToEnemies : state.damageToObstacles);
            }
            else if (layerName == "Platform")
            {
                Global.soundManager.Play("hitPlatform");
            }
        }

        if (len > 0 && shake > 0)
            Global.hud.ShakeCamera(shake);

        if (canBounceOff)
        {
            _rigidbody.velocity = attackRecoil;
        }

        yield return new WaitForSeconds(values.protectEffectLifeTime);

        isProtecting = false;
        yield return new WaitForSeconds(values.protectInterval);
        isProtectable = true;
    }

    protected void attackUp()
    {
        attackUpEffect.SetActive(true);

        var detectDirection = new Vector2(0, 1);

        _animator.Activate("hit up", true);
        Debug.Log("attackUp");
        StartCoroutine(attackCoroutine(attackUpEffect, detectDirection, values.attackUpRecoil));
    }

    protected void attackForward()
    {
        attackForwardEffect.SetActive(true);

        var detectDirection = new Vector2(-transform.localScale.x, 0);

        var recoil = new Vector2(
            transform.localScale.x > 0 ? -values.attackForwardRecoil.x : values.attackForwardRecoil.x,
            values.attackForwardRecoil.y);

        _animator.Activate("hit", true);
        Debug.Log("attackForward");
        StartCoroutine(attackCoroutine(attackForwardEffect, detectDirection, recoil));
    }

    protected void attackDown()
    {
        attackDownEffect.SetActive(true);

        var detectDirection = new Vector2(0, -1);

        _animator.Activate("hit down", true);
        Debug.Log("attackDown");
        StartCoroutine(attackCoroutine(attackDownEffect, detectDirection, values.attackDownRecoil));
    }

    protected IEnumerator attackCoroutine(GameObject attackEffect, Vector2 detectDirection, Vector2 attackRecoil)
    {
        Debug.Log("attackCoroutine" + attackEffect + "," + detectDirection + "," + attackRecoil);
        // attack cool down
        isAttackable = false;
        isAttacking = true;
        Vector2 origin = transform.position;
        origin += detectDirection * 0.1f; // no radius behind player pos 
        LayerMask layerMask = LayerMask.GetMask("Enemy")
        | LayerMask.GetMask("Interactable")
        | LayerMask.GetMask("Platform") | LayerMask.GetMask("Deco");


        int len = Physics2D.CircleCastNonAlloc(origin, values.attackDetectRadius, detectDirection, hitRecList, values.attackDetectDistance, layerMask);

        if (Global.isDebug)
            Debug.Log("hitRecList " + String.Join(", ", hitRecList));

        var canBounceOff = hitRecList.Length > 0;

        float shake = 0;
        shake = 0.05f;

        var amountManaFromHit = 0;

        Global.soundManager.Play("attack");

        for (var i = 0; i < len; i++)
        {
            RaycastHit2D hitRec = hitRecList[i];
            GameObject obj = hitRec.collider.gameObject;

            string layerName = LayerMask.LayerToName(obj.layer);

            var controller = obj.GetComponent<InteractableObject>();
            if (controller != null)
            {
                if (!controller.canBounceOff)
                {
                    canBounceOff = false;
                    shake = 0f;
                }

                if (controller.grantsManaOnHit)
                    amountManaFromHit++;
                controller.playerController = this;

                controller.hurt(layerName == "Enemy" ? state.damageToEnemies : state.damageToObstacles);
            }
            else if (layerName == "Platform")
            {
                Global.soundManager.Play("hitPlatform");
            }
        }

        if (hitRecList.Length > 0 && shake > 0)
            Global.hud.ShakeCamera(shake);

        gainMana(amountManaFromHit);

        if (canBounceOff)
        {
            _rigidbody.velocity = attackRecoil;
        }

        if (hitRecList.Length > 0)
            rumbleGamepadGeckoAttacking();

        yield return new WaitForSeconds(values.attackEffectLifeTime);

        attackEffect.SetActive(false);

        isAttacking = false;
        yield return new WaitForSeconds(values.attackInterval);
        isAttackable = true;
    }

    public void rumbleGamepadGeckoAttacking()
    {
        StopCoroutine("StartRumbleAttack");
        StartCoroutine("StartRumbleAttack");

    }

    public void rumbleGamepadGeckoHit()
    {
        StopCoroutine("StartRumbleHit");
        StartCoroutine("StartRumbleHit");
    }

    public IEnumerator StartRumbleAttack()
    {
        GetGamepad()?.SetMotorSpeed(0.1f, 0.1f);
        yield return new WaitForSecondsRealtime(0.1f);
        GetGamepad()?.SetMotorSpeed(0, 0);
    }

    public IEnumerator StartRumbleHit()
    {
        GetGamepad()?.SetMotorSpeed(0.2f, 0.2f);
        yield return new WaitForSecondsRealtime(0.2f);
        GetGamepad()?.SetMotorSpeed(0, 0);
    }

    public interface Gamepad2
    {
        void SetMotorSpeed(float a, float b);
    }

    public Gamepad2 GetGamepad()
    {
        return null;
    }

    public void spores()
    {
        sporeParticles.Play();
        gainMana(1);
    }

    public void gainMana(int amount)
    {
        state.mana = Math.Min(state.mana + amount, state.maxMana);
    }

    public void modifyShells(int delta, string id = null)
    {
        state.shells += delta;
        AddCollected(id);
    }

    public void AddCollected(string id)
    {
        if (id != null)
        {
            var lst = new List<string>(state.collected);
            lst.Add(id);
            state.collected = lst.ToArray();
        }
    }

    public void AddActivated(string id)
    {
        if (id != null)
        {
            var lst = new List<string>(state.activated);
            lst.Add(id);
            state.activated = lst.ToArray();
        }
    }

    public void AddDefeated(string id)
    {
        if (id != null)
        {
            var lst = new List<string>(state.defeated);
            lst.Add(id);
            state.defeated = lst.ToArray();

            state.percentage = CalcPercentage();
        }
    }

    public static int numberOfBosses = 4;
    public static int numberOfUpgrades = 8;

    int CalcPercentage()
    {
        return
            (int)(
            50f * state.defeated.Length / numberOfBosses
            +
            50f * (
                  // upgrades
                  (state.canDash ? 1 : 0)
                + (state.canAttack ? 1 : 0)
                + (state.canMagic ? 1 : 0)
                + (state.canProtection ? 1 : 0)
                + (state.dashInvulnerable ? 1 : 0)
                // map with upgrades
                + (state.canShowMap ? 1 : 0)
                + (state.minimapShowBoss ? 1 : 0)
                + (state.minimapShowShells ? 1 : 0)
                ) / numberOfUpgrades
            );
    }

    public void ReceiveUpgrade(UpgradeType upgrade, string id = null)
    {
        Global.LogImportant("Received: " + upgrade + " after " + state.time.FormatPlayTime(), "GameplayInfo");

        state.health = state.maxHealth;

        if (upgrade == UpgradeType.Needle)
        {
            state.canAttack = true;
        }
        if (upgrade == UpgradeType.Dash)
        {
            state.canDash = true;
        }
        if (upgrade == UpgradeType.Heal)
        {
            state.canMagic = true;
        }
        if (upgrade == UpgradeType.Map)
        {
            state.canShowMap = true;
        }

        AddCollected(id);

        lastCheckpointPos = transform.position;
        Global.SaveGame();
        lastSafePosition = transform.position;
    }

    public PlayerSaveState GetStateWithMap(bool useCheckpoint = false)
    {
        state.position = useCheckpoint ? lastCheckpointPos : transform.position;
        state.sceneName = SceneManager.GetActiveScene().name;
        //state.areaName = Global.tileLevel?.currentArea;
        var map2 = MapLogic.edgesToStrings(Global.minimapUI?.edgesArray);
        if (map2 != null)
            state.map = map2;

        Global.profile = state;
        return state;
    }

    public void LoadFromState(PlayerSaveState data, bool incDeaths = false)
    {
        if (playerIndex == 0)
            Global.profile = state;
        else
            data = SaveSystem.Clone(data);

        isInputEnabled = true;
        makeVulnerable();
        state = data;
        if (incDeaths)
            state.deaths++;

        state.health = Math.Min(1, state.health);

        TeleportToPos(data.position, overrideGrounded: false);
    }

    public void TeleportToPos(Vector3 pos, bool overrideGrounded = true)
    {
        lastSafePosition = pos;
        transform.position = pos;
        _rigidbody.velocity = Vector2.zero;
        if (overrideGrounded)
        {
            hasContactBottom = true;
            isClimb = false;
            isJumping = false;
            _animator.Activate("idle");
        }
    }
}
