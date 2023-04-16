using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapAreaLoader : MonoBehaviour
{
    public string onLeftLoad, onRightLoad;

    void Start()
    {
        
    }

    void Update()
    {

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("entering a load area, l: " + onLeftLoad + ", r: " + onRightLoad);
        
        LoadSection(onLeftLoad ?? onRightLoad, transitionMusicOnly: true);

    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        var p = collision.GetComponent<PlayerController>();
        if (p == null)
            return;

        if (p.transform.position.x < transform.position.x)
        {
            Debug.Log("exit to the left, l: " + onLeftLoad + ", r: " + onRightLoad);
            LoadSection(onLeftLoad, transitionMusicOnly: false);
        }

        if (transform.position.x < p.transform.position.x)
        {
            Debug.Log("exit to the right, l: " + onLeftLoad + ", r: " + onRightLoad);
            LoadSection(onRightLoad, transitionMusicOnly: false);
        }
    }

    private void LoadSection(string sectionName, bool transitionMusicOnly)
    {
        var w = FindObjectOfType<LevelWorld>();
        if (transitionMusicOnly)
            w.LoadSectionMusic(sectionName, true);
        else
            w.LoadSection(sectionName);
    }
}
