using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour, IModal
{
    public bool sessionEnemiesLoaded, sessionEnvLoaded, inTransition;

    internal string startScene = "IntroScene";

    public GameObject mainMenuButtons, optionsMenu, buttonStartMain, buttonProfile, creditsMenu, buttonCreditsBack;
    public Text loadStateInfo, languageInfo, difficultyInfo, profileInfo, inputInfo, soundInfo, musicInfo, debugInfo, updateInfo;

    // for modal
    internal bool wasMainMenuButtonsActive, wasOptionsMenuActive;

    void Start()
    {
        RefreshInput();
        Translations.Apply(Global.settings.language);
        Global.RefreshSound(Global.settings.sound);

        BackInMainMenu();

        Global.soundManager.PlayMusic("MainTheme");
        Global.hud.UiFadeIn(() => { });
    }

    void updateCheck()
    {
        var cd = Global.contentDownloader;
        Global.LogDebug("contentDownloader: " + cd);
        cd.CheckForUpdatesAndDownload(info: x => updateInfo.text = x, modal: this);
    }

    void RefreshInput()
    {
    }

    void RefreshFromChangedOptions()
    {
        RefreshInput();
        Global.profile = SaveSystem.LoadPlayerProfile(Global.settings.profile, false) ?? new PlayerSaveState();

        loadStateInfo.text = Global.profile.ToString();
        SetBuildNum.done = false;

        languageInfo.text = Global.settings.language.MakeJoinedHtml();
        inputInfo.text = Global.settings.input.MakeJoinedHtmlForInput();
        difficultyInfo.text = Global.profile.difficulty.MakeJoinedHtml();
        profileInfo.text = Global.settings.profile.MakeJoinedHtml();
        musicInfo.text = Global.settings.music.MakeJoinedHtml();
        soundInfo.text = Global.settings.sound.MakeJoinedHtml();
        debugInfo.text = Global.settings.debug.MakeJoinedHtml();

        Global.hud.error.gameObject.SetActive(Global.isDebug);
        Global.hud.debug.gameObject.SetActive(Global.isDebug);
    }

    bool startClicked = false;
    public void OnStartGame()
    {
        if (startClicked)
            return;

        startClicked = true;

        Global.soundManager.StartCoroutine(Global.soundManager.FadeMusicOut());
        Global.soundManager.Play("select");

        var save = SaveSystem.LoadPlayerProfile(Global.settings.profile);
        var startSceneToLoad = save?.sceneName ?? startScene;

        Global.playerNeedsToLoadData = save != null;
        Global.hud.UiFadeOut(() => SceneManager.LoadScene(startSceneToLoad));
    }

    public void OnLanguage()
    {
        Global.soundManager.Play("select");

        var x = Global.settings.language;
        Global.settings.language = x.Next();

        Global.SaveSettings();

        Translations.Apply(Global.settings.language);

        RefreshFromChangedOptions();
    }

    public void OnInput()
    {
        Global.soundManager.Play("select");

        var x = Global.settings.input;
        Global.settings.input = x.NextForInput();

        RefreshInput();
        Global.SaveSettings();

        RefreshFromChangedOptions();
    }

    public void OnDifficulty()
    {
        Global.soundManager.Play("select");

        var x = Global.profile.difficulty;
        Global.profile.difficulty = x.Next();

        Global.SaveGame(targetPos: null, justProfile: true);

        RefreshFromChangedOptions();
    }

    public void OnMusic()
    {
        Global.soundManager.Play("select");

        var x = Global.settings.music;
        Global.settings.music = x.Next();

        Global.SaveSettings();

        RefreshFromChangedOptions();
    }

    public void OnSound()
    {
        var x = Global.settings.sound;
        Global.settings.sound = x.Next();

        Global.RefreshSound(Global.settings.sound);

        Global.SaveSettings();

        RefreshFromChangedOptions();

        Global.soundManager.Play("select");
    }

    public void OnProfile()
    {
        Global.soundManager.Play("select");

        var x = Global.settings.profile;
        Global.settings.profile = x.Next();

        Global.SaveSettings();

        RefreshFromChangedOptions();
    }

    public void OnDelete()
    {
        Global.soundManager.Play("select");

        OpenModal(
            "Delete Profile " + Global.settings.profile + "? This cannot be reversed.",
            onYes: () => {
                Global.soundManager.Play("select");
                SaveSystem.Delete(Global.settings.profile);
                Global.profile = new PlayerSaveState();
                Global.SaveSettings();
                RefreshFromChangedOptions();
            },
            onNo: () => { });
    }

    public void OnDebug()
    {
        Global.soundManager.Play("select");

        Global.settings.debug = Global.settings.debug.Next();
        Global.SaveSettings();
        RefreshFromChangedOptions();
    }

    public void OnOptionsBack()
    {
        Global.soundManager.Play("select");

        mainMenuButtons.SetActive(true);
        optionsMenu.SetActive(false);
        creditsMenu.SetActive(false);

        BackInMainMenu();
    }

    public void OnCredits()
    {
        Global.soundManager.Play("select");

        mainMenuButtons.SetActive(false);
        optionsMenu.SetActive(false);
        creditsMenu.SetActive(true);

        BackInMainMenu();

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonCreditsBack);
    }

    public void OnOptions()
    {
        Global.soundManager.Play("select");

        mainMenuButtons.SetActive(false);
        optionsMenu.SetActive(true);
        creditsMenu.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonProfile);
    }

    public void OnCommunity()
    {
        Global.soundManager.Play("select");

        Application.OpenURL(Global.discordInvite);
    }

    public void OnUpdateCheck()
    {
        Global.soundManager.Play("select");

        updateCheck();
    }

    public void OnQuit()
    {
        Global.hud.UiFadeOut(() => Application.Quit());
        Global.soundManager.Play("select");
    }

    public Action<bool> OnTestDone;
    public void OnTest()
    {
        Global.soundManager.Play("select");

        Action<bool> defaultCallback = (bool success) =>
        {
            Global.LogImportant("Test returned " + success, "Test");
        };
    }

    public void OnMainMenu()
    {
        Global.soundManager.Play("select");
        BackInMainMenu();
    }

    public void BackInMainMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonStartMain);
        RefreshFromChangedOptions();
    }

    public void OpenModal(string question, Action onYes, Action onNo)
    {
        wasMainMenuButtonsActive = mainMenuButtons.activeInHierarchy;
        wasOptionsMenuActive = optionsMenu.activeInHierarchy;

        optionsMenu.SetActive(false);
        mainMenuButtons.SetActive(false);

        Global.hud.OpenModal(
            question,
            onYes: () => {
                onYes();
                mainMenuButtons.SetActive(wasMainMenuButtonsActive);
                optionsMenu.SetActive(wasOptionsMenuActive);
            },
            onNo: () => {
                onNo();
                mainMenuButtons.SetActive(wasMainMenuButtonsActive);
                optionsMenu.SetActive(wasOptionsMenuActive);
            });
    }
}