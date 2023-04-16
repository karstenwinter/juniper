using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class Sound
{
    public string name = "";
    public AudioClip clip;
    public float volume = 1.0f;
    public float pitch = 1.0f;
    public bool loop = false;
    internal SoundManager manager;
    internal AudioSource audioSource;
}

public class SoundManager : MonoBehaviour
{
    protected string lastMusic = "MainTheme";
    protected string gameMusic = "MossyAlcoves88";

    public Sound[] clips = new Sound[0];
    internal Dictionary<string, Sound> soundMap = new Dictionary<string, Sound>();

    public Sound[] musicClips = new Sound[0];
    internal Dictionary<string, Sound> musicMap = new Dictionary<string, Sound>();

    AudioSource fightAudioSource, activeAudioSource, oldActiveAudioSource;
    public float fadeDuration = 4f;

    public static int numPlaying;

    void Awake()
    {
        Global.soundManager = this;
        soundMap.Clear();
        musicMap.Clear();

        foreach (var item in clips)
        {
            InitItem(item);
            soundMap[item.name] = item;
        }
        foreach (var item in musicClips)
        {
            InitItem(item);
            musicMap[item.name] = item;

            if (item.name == "Fight")
                fightAudioSource = item.audioSource;
        }
    }

    void Start()
    {
        PlayMusic(gameMusic);
    }

    void InitItem(Sound item)
    {
        item.manager = this;
        item.audioSource = gameObject.AddComponent<AudioSource>();
        item.audioSource.playOnAwake = false;
        item.audioSource.clip = item.clip;
        item.audioSource.volume = item.volume;
        item.audioSource.pitch = item.pitch;
        item.audioSource.loop = item.loop;
    }

    public void Play(string name)
    {
        // Debug.Log("Play sound " + name + "/" + soundMap.Count);
        if (soundMap.Count == 0)
            return;

        Sound sound;
        if (soundMap.TryGetValue(name, out sound))
        {
            sound.audioSource.Play();
        }
        else
        {
            Debug.LogError("Sound not found: " + name);
        }
    }

    public void EnterFight()
    {
        if (musicMap.Count == 0)
            return;

        StopCoroutine("FadeOldMusicOut");
        StartCoroutine("FadeOldMusicOut");

        StopCoroutine("FadeMusicOut");
        StopCoroutine("FadeMusicIn");
        StartCoroutine("FadeMusicOut");

        StopCoroutine("FadeFightOut");
        StopCoroutine("FadeFightIn");
        StartCoroutine("FadeFightIn");
    }

    public void ExitFight()
    {
        if (musicMap.Count == 0)
            return;

        StopCoroutine("FadeOldMusicOut");
        StartCoroutine("FadeOldMusicOut");

        StopCoroutine("FadeMusicOut");
        StopCoroutine("FadeMusicIn");
        StartCoroutine("FadeMusicIn");

        StopCoroutine("FadeFightOut");
        StopCoroutine("FadeFightIn");
        StartCoroutine("FadeFightOut");
    }

    public IEnumerator FadeFightOut()
    {
        float currentTime = 0;
        PlaySilent(fightAudioSource);
        float start = fightAudioSource.volume;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            fightAudioSource.volume = Mathf.Lerp(start, 0, currentTime / fadeDuration);
            yield return new WaitForSecondsRealtime(0.001f);
        }
        StopSilent(fightAudioSource);
    }

    public IEnumerator FadeFightIn()
    {
        float currentTime = 0;
        PlaySilent(fightAudioSource);
        float start = fightAudioSource.volume;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            fightAudioSource.volume = Mathf.Lerp(start, 1, currentTime / fadeDuration);
            yield return new WaitForSecondsRealtime(0.001f);
        }
    }

    public IEnumerator FadeMusicOut()
    {
        if (activeAudioSource == null)
            yield break;

        float currentTime = 0;
        PlaySilent(activeAudioSource);
        float start = activeAudioSource.volume;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            activeAudioSource.volume = Mathf.Lerp(start, 0, currentTime / fadeDuration);
            yield return new WaitForSecondsRealtime(0.001f);
        }
        StopSilent(activeAudioSource);
    }

    public IEnumerator FadeMusicIn()
    {
        if (activeAudioSource == null)
            yield break;

        float currentTime = 0;
        PlaySilent(activeAudioSource);
        float start = activeAudioSource.volume;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            activeAudioSource.volume = Mathf.Lerp(start, 1, currentTime / fadeDuration);
            yield return new WaitForSecondsRealtime(0.001f);
        }
        StopSilent(activeAudioSource);
    }

    public IEnumerator FadeOldMusicOut()
    {
        if (oldActiveAudioSource == null)
            yield break;

        float currentTime = 0;
        PlaySilent(oldActiveAudioSource);
        float start = oldActiveAudioSource.volume;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            oldActiveAudioSource.volume = Mathf.Lerp(start, 0, currentTime / fadeDuration);
            yield return new WaitForSecondsRealtime(0.001f);
        }
        StopSilent(oldActiveAudioSource);
    }

    void PlaySilent(AudioSource a)
    {
        if (!a.isPlaying)
        {
            if (a.volume == 1)
                a.volume = 0;
            a.Play();
        }
    }

    void StopSilent(AudioSource a)
    { }

    public AudioSource EnterSequence(string newMusic)
    {
        var active = activeAudioSource;
        PlayMusic(newMusic);
        return active;
    }

    public void Restore(AudioSource restore)
    {
        oldActiveAudioSource = activeAudioSource;
        activeAudioSource = restore;
        ExitFight();
    }
    public void PlayMusic(string name)
    {
        if (musicMap.Count == 0)
            return;

        Sound sound;
        if (musicMap.TryGetValue(name, out sound))
        {
            lastMusic = name;

            oldActiveAudioSource = activeAudioSource;
            activeAudioSource = sound.audioSource;

            ExitFight();
        }
    }
}
