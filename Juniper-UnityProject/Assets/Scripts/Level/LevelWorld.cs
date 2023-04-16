using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelWorld : MonoBehaviour
{
    public new Light light;
    public bool timeDebug;
    public float dayTime = 10.5f; // 10:30 
    public float timeSpeed = 1; // 1h per minute
    public float intensity0, intensity6, intensity12, intensity18;
    public Color color0, color6, color12, color18;
    public GameObject activeSection;
    public float zPosForCopies = 0.85f;
    string currentlyActive;
    public MusicInfo musicInfo;
    public string startSec;

    void Start()
    {
        LoadSection(startSec);
    }

    public void LoadSectionMusic(string newSectionName, bool transitionMusic)
    {
        foreach (var item in musicInfo.entries)
        {
            if (item.isTransition == transitionMusic && item.sectionCommaSep.Contains(newSectionName))
            {
                Global.soundManager.PlayMusic(item.musicTrack);
                break;
            }
        }
    }

    public void LoadSection(string newSectionName, bool ignoreSameName = true)
        {
            if (ignoreSameName && currentlyActive == newSectionName)
            return;

        Debug.Log("LoadSection " + newSectionName);
        var pref = Resources.Load<GameObject>("Sections/" + newSectionName);
        Debug.Log("LoadSection pref " + pref);
        if(pref != null)
        {
            currentlyActive = newSectionName;
            Destroy(activeSection);
            activeSection = null;
        }
        activeSection = Instantiate(pref, transform);
        Debug.Log("LoadSection pref inst " + activeSection);
        RefreshCopyLayer();
        LoadSectionMusic(newSectionName, false);
    }

    [ContextMenu("DestroyPrev")]
    public void DestroyPrev()
    { 
    }

    [ContextMenu("TEST")]
    public void RefreshCopyLayer()
    {
        var g = activeSection.transform.Find("Grounds").transform;
        foreach (Transform t in g)
        {
            if (t.name.Contains("Copy") || g.Find(t.name + "Copy") != null)
                continue;

            var copy = Instantiate(t.gameObject, g);
            copy.name += "Copy";
            copy.RemoveComponent<EdgeCollider2D>();
            var p = copy.transform.localPosition;
            p.z = zPosForCopies;
            copy.transform.localPosition = p;
        }
    }

    private void OnValidate()
    {
        UpdateLights();
    }

    void Update()
    {
        if(!timeDebug)
        { 
            var t = Global.playerController?.state?.time ?? Time.timeSinceLevelLoad;
            dayTime = timeSpeed * t;
            if (dayTime >= 24)
                dayTime -= 24;
        }

        UpdateLights();
    }

    private void UpdateLights()
    {
        var t = dayTime / 24f;

        light.intensity =
            Mathf.Lerp(
                Mathf.Lerp(
                    Mathf.Lerp(
                        Mathf.Lerp(intensity0, intensity6, t),
                        Mathf.Lerp(intensity6, intensity12, t), t),
                    Mathf.Lerp(intensity12, intensity18, t), t),
                Mathf.Lerp(intensity18, intensity0, t), t);

        light.color =
            Color.Lerp(
                Color.Lerp(
                    Color.Lerp(
                        Color.Lerp(color0, color6, t),
                        Color.Lerp(color6, color12, t), t),
                    Color.Lerp(color12, color18, t), t),
                Color.Lerp(color18, color0, t), t);
    }

    /*Color lerp3(Color c1, Color c2, Color c3, float t)
    {
        var position1 = Color.Lerp(c1, c2, t);
        var position2 = Color.Lerp(c2, c3, t);
        return Color.Lerp(position1, position2, t);
    }

    Color lerpMulti(Color c1, Color c2, Color c3, Color c4, float t)
    {

    }*/
}
