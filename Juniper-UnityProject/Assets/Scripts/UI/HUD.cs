using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class HUD : MonoBehaviour, IModal
{
    public bool isFadeIn, isFadeOut;
    public GameObject pauseMenu, mobileInput, characterFader;
    public Image bgBlackFade;
    public ParticleSystem saveAnimation;
    public Button blurButton, aaButton, continueButton, yesButton, noButton, saveButton, loadButton, invButton;
    public GameObject[] mobileButtons = new GameObject[0];
    public Image shellImage;
    public Text shellText, error, debug, blurInfo, antialiasingInfo;
    public Image vignette, noise;
    public new Camera camera;

    Color fadeColor { get { return bgBlackFade == null ? default(Color) : bgBlackFade.color; } set { if (bgBlackFade != null) bgBlackFade.color = value; } }
    public bool isBlack { get { return bgBlackFade == null ? false : fadeColor.a != 0; } }
    bool ignoreFadeIn, ignoreFadeOut;
    public bool canPause = true;
    bool internalCanPause;
    float fadeSpeed = 0.2f; // 0.075f;
    float fadeWait = 0.03f; // 0.05f
    public float targetFadeOut = 1f;

    public float fadeFactor = 1f;
    public float campFadeFactor = 0.3f;
    public float fadedInScale = 4;
    public float fadedOutScale = 0.01f;
    public float targetFadeChar = 4;
    public float inactivePlayerDuringFadeTime = 0.5f;
    public float campInactivePlayerDuringFadeTime = 1.3f;

    int oldShells;
    public float timeScale = 1f;

    // for modal
    internal Action onYes, onNo;

    public GameObject modalMenu;
    public Text modalMenuText;

    //float shake = 0;
    //float shakeAmount = 0.7f;
    //float decreaseFactor = 1;
    public float shellShownAlpha = 0.5f;
    public float shellFadeAlpha = 0.01f;

    void Awake()
    {
        Global.hud = this;
    }

    void Start()
    {
        if (characterFader?.gameObject != null)
            characterFader.gameObject.SetActive(true);

        if (saveButton != null)
        {
            saveButton.gameObject.SetActive(Global.isDebug);
        }
        if (loadButton != null)
        {
            loadButton.gameObject.SetActive(Global.isDebug);
        }
        if (invButton != null)
        {
            invButton.gameObject.SetActive(Global.isDebug);
        }

        internalCanPause = canPause;

        bgBlackFade.gameObject.SetActive(true);

        var fadeC = fadeColor;
        fadeC.a = 1;
        fadeColor = fadeC;

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            Translations.Apply(Global.settings.language);
            Global.RefreshSound(Global.settings.sound);
            var invis = Global.settings.input == InputType.TouchpadInvisible;
            mobileInput.SetActive(Global.settings.input == InputType.Touchpad || invis);

            if (invis)
            {
                foreach (var item in mobileButtons)
                {
                    var img = item.GetComponent<Image>();
                    var c = img.color;
                    c.a = 0;
                    img.color = c;

                    var text = item.GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        c = text.color;
                        c.a = 0;
                        text.color = c;
                    }
                }
            }
        }
        else
        {
            mobileInput.SetActive(false);
        }

        ApplyVisualEffectsFromSettings();
    }

    public void ShakeCamera(float shakeAmount = 0, float shakeTime = 0.18f)
    {
        //this.shake = shakeTime;
        //this.shakeAmount = shakeAmount;
    }

    void ApplyVisualEffectsFromSettings()
    {
        var invis = Global.settings.input == InputType.TouchpadInvisible;
        mobileInput.SetActive(Global.settings.input == InputType.Touchpad || invis);
    }

    void Update()
    {
        /*if (shake > 0)
        {
            camera.transform.localPosition = UnityEngine.Random.insideUnitSphere * shakeAmount;
            shake -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            camera.transform.localPosition = Vector3.zero;
            shake = 0;
        }*/

        if (characterFader != null)
        {
            if (isFadeIn)
            {
                if (characterFader.transform.localScale.x < fadedInScale)
                {
                    characterFader.transform.localScale += Vector3.one * fadeSpeed * fadeFactor;
                    if (characterFader.transform.localScale.x > fadedInScale)
                        characterFader.transform.localScale = Vector3.one * fadedInScale;

                }
            }
            if (isFadeOut)
            {
                if (characterFader.transform.localScale.x > fadedOutScale)
                {
                    characterFader.transform.localScale -= Vector3.one * fadeSpeed * fadeFactor;
                    if (characterFader.transform.localScale.x < fadedOutScale)
                        characterFader.transform.localScale = Vector3.one * fadedOutScale;
                }
            }
        }

        if (Global.playerController == null)
            return;

        if (Global.isDebug)
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                PauseUnpause();
                OnSave();
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                PauseUnpause();
                OnLoadGame();
            }
        }

        if (internalCanPause && Global.playerController.Pause)
        {
            // PauseUnpause();
            var aParent = GameObject.Find("ActionButtonParent");
            var dParent = GameObject.Find("DirButtonParent");
            if (aParent && dParent)
            {
                var enabled = aParent.activeInHierarchy;
                aParent.SetActive(!enabled);
                dParent.SetActive(!enabled);
            }
            Global.playerController.Pause = false;
        }

        var shells = Global.playerController.state.shells;
        if (oldShells != shells)
        {
            oldShells = shells;
            shellText.text = shells.ToString();
            var c = shellText.color;
            c.a = shellShownAlpha;
            shellText.color = c;
            shellImage.color = c;
        }
        else if (shellText.color.a > 0)
        {
            var c = shellText.color;
            c.a -= shellFadeAlpha;
            shellText.color = c;
            shellImage.color = c;
        }
    }

    public void OnMainMenu()
    {
        Global.soundManager.Play("select");
        PauseUnpause();
        Global.TransitionToScene("MainMenu", null, false, false);
    }

    public void OnSaveAndExit()
    {
        Global.soundManager.Play("select");
        Global.SaveGame();
        PauseUnpause();

        Global.TransitionToScene("MainMenu", null, false, false);
    }

    void PauseUnpause()
    {
        pauseMenu.SetActive(!pauseMenu.activeInHierarchy);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(continueButton.gameObject);

        Time.timeScale = pauseMenu.activeInHierarchy ? 0 : timeScale;

        Global.isPaused = pauseMenu.activeInHierarchy;
    }

    void MenuOff()
    {
        Time.timeScale = timeScale;
        pauseMenu.SetActive(false);
    }

    public void OnContinue()
    {
        Global.soundManager.Play("select");
        PauseUnpause();
    }

    public void OnSave()
    {
        Global.soundManager.Play("select");
        Global.SaveGame(useCheckpoint: false);
        PauseUnpause();
    }

    public void OnLoadGame()
    {
        if (Global.isDebug)
        {
            Caches.Clear();
        }

        Global.soundManager.Play("select");

        Global.playerController.isInputEnabled = false;
        FadeOut(() =>
        {
            SaveSystem.Load(Global.settings.profile);
            FadeIn(() =>
            {
                Global.playerController.isInputEnabled = true;
            });
        });
        PauseUnpause();
    }

    public void OnBlurOption()
    {
        Global.soundManager.Play("select");
        Global.settings.input = Global.settings.input == InputType.Touchpad ? InputType.KeyboardGamepad : InputType.Touchpad;
        //Global.settings.blur = Global.settings.blur.Next();
        ApplyVisualEffectsFromSettings();
        Global.SaveSettings();
    }

    public void OnAntiAliasOption()
    {
        Global.soundManager.Play("select");
        Global.settings.antiAliasing = Global.settings.antiAliasing.Next();
        ApplyVisualEffectsFromSettings();
        Global.SaveSettings();
    }

    Action fadeOutThen;
    Action fadeInThen;

    public void FadeOut(Action then)
    {
        targetFadeChar = fadedOutScale;
        isFadeOut = true;
        isFadeIn = false;
        fadeInThen = then;
        StartCoroutine("fadeInThenInvoke");
    }

    public void FadeIn(Action then)
    {
        targetFadeChar = fadedInScale;
        isFadeOut = false;
        isFadeIn = true;
        fadeInThen = then;
        StartCoroutine("fadeInThenInvoke");
    }

    public void UiFadeIn(Action then)
    {
        fadeInThen = then;
        StopCoroutine("CFadeOut");
        StartCoroutine("CFadeIn");
    }

    public void UiFadeOut(Action then)
    {
        fadeOutThen = then;
        StartCoroutine("CFadeOut");
    }

    IEnumerator fadeInThenInvoke()
    {
        yield return new WaitForSecondsRealtime(inactivePlayerDuringFadeTime);
        if (isFadeOut)
            characterFader.transform.localScale = Vector3.one * targetFadeChar;

        if (fadeInThen != null)
            fadeInThen.Invoke();
    }

    IEnumerator CFadeOut()
    {
        Action then = fadeOutThen;
        fadeOutThen = null;

        var c = fadeColor;
        while (fadeColor.a < targetFadeOut)
        {
            c = fadeColor;
            c.a += fadeSpeed * fadeFactor;
            fadeColor = c;
            yield return new WaitForSecondsRealtime(fadeWait / fadeFactor);
        }
        c = fadeColor;
        c.a = targetFadeOut;
        fadeColor = c;

        yield return new WaitForSecondsRealtime(0.01f);

        then.Invoke();
    }

    IEnumerator CFadeIn()
    {
        Action then = fadeInThen;
        fadeInThen = null;

        var c = fadeColor;
        while (fadeColor.a > 0)
        {
            c = fadeColor;
            c.a -= fadeSpeed * fadeFactor;
            fadeColor = c;
            yield return new WaitForSecondsRealtime(fadeWait / fadeFactor);
        }
        c = fadeColor;
        c.a = 0;
        fadeColor = c;

        yield return new WaitForSecondsRealtime(0.01f);
        then.Invoke();

        internalCanPause = canPause;
    }

    public void OnLeft()
    {
    }

    public void OnLeftUp()
    {
    }

    public void OnUp()
    {
    }

    public void OnUpUp()
    {
    }

    public void OnDown()
    {
    }

    public void OnDownUp()
    {
    }

    public void OnAction()
    {
    }

    public void OnActionUp()
    {
    }

    public void OnAttack()
    {
        if (Global.settings.mobileAutoFire != OffOn.On)
            Global.playerController.attack();
    }

    public void OnAttackUp()
    {
    }

    public void OnJump()
    {
        if (Global.settings.mobileAutoFire != OffOn.On)
            Global.playerController.jumpOrClimbJump();
    }

    public void OnJumpUp()
    {
    }

    public void OnDash()
    {
        if (Global.settings.mobileAutoFire != OffOn.On)
            Global.playerController.dash();
    }

    public void OnDashUp()
    {
    }

    public void OnMagic()
    {
        if (Global.settings.mobileAutoFire != OffOn.On)
            Global.playerController.magic();
    }

    public void OnMagicUp()
    {
    }

    public void OnRight()
    {
    }

    public void OnRightUp()
    {
    }

    public void OnMenu()
    {
        if (internalCanPause)
        {
            Global.soundManager.Play("select");
            PauseUnpause();
        }
    }

    public void OnMap()
    {
    }

    public void SaveAnimation()
    {
    }

    public void OpenModal(string question, Action onYes, Action onNo)
    {
        modalMenuText.text = question;
        modalMenu.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(noButton.gameObject);

        this.onYes = onYes;
        this.onNo = onNo;
    }

    public void OpenModal(string question, Action onOk)
    {
        modalMenuText.text = question;
        modalMenu.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        
        EventSystem.current.SetSelectedGameObject(yesButton.gameObject);

        // on walk away just close, no ok button anmymore
        yesButton.GetComponentInChildren<Text>().text = ""; // Translations.For("OK");
        noButton.gameObject.SetActive(false);
        this.onYes = onOk;
        this.onNo = null;
    }

    public void OnYesClick()
    {
        onYes.Invoke();
        CloseModal();
    }

    public void OnInventoryTest()
    {
        SceneManager.LoadScene("Inventory");
    }

    public void CloseModal()
    {
        modalMenu.SetActive(false);

        onYes = null;
        onNo = null;
        modalMenuText.text = "";
        noButton.gameObject.SetActive(true);
        yesButton.GetComponentInChildren<Text>().text = Translations.For("Yes");
    }

    public void OnNoClick()
    {
        onNo.Invoke();
        CloseModal();
    }
}
