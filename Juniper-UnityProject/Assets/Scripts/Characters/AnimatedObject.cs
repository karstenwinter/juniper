using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEngine;

public class AnimatedObject : MonoBehaviour
{
    public GameObject takeNameFrom;

    SpriteRenderer render;
    Sprite[] current;
    public bool isCharacter;

    public string state = "idle";
    public int startFrame = 0;
    public int imgWidth = 10;

    internal float counter;
    internal int i;

    public float speed = 6f;
    public float pauseAfterCycle = 0f;

    public float waitCylces = 0f;
    string lastState;
    bool playToEnd;

    public float unit = 240f;
    public float pixelsPerUnit = 120f;
    public Vector2 pos = new Vector2(0.5f, 0.5f);

    public string[] status;
    Dictionary<string, Sprite[]> states = new Dictionary<string, Sprite[]>();

    void Start()
    {
        Load();
    }

    [ContextMenu("Load")]
    public void Load()
    {
        render = GetComponent<SpriteRenderer>();
        var name = (takeNameFrom != null ? takeNameFrom : gameObject).name;
        var found = new List<AnimatedBase>();
        foreach (var item in Caches.CharacterData)
        {
            if (item.name == name)
                found.Add(item);
        }
        foreach (var item in Caches.AnimatedData)
        {
            if (item.name == name)
                found.Add(item);
        }

        var y = -1;
        foreach (var item in found)
        {
            float parsed;
            if (float.TryParse(item.data, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
            {
                unit = parsed;
            }

            y++;
            var list = new List<Sprite>();
            for (int x = 0; x < item.frames; x++)
            {
                var sp =
                    ExternalDataLoader.LoadSpriteForCharacterOrAnimated(isCharacter, name, x, y, imgWidth, unit, pixelsPerUnit, pos);
                Global.LogDebug("sprite at y" + y + "x" + x + " of " + name + " is " + item.state + " => " + sp);
                list.Add(sp);
            }

            if (list.Count > 0 || !states.ContainsKey(item.state))
                states[item.state] = list.ToArray();
        }

        Activate(state);
        if (states.TryGetValue(state, out current))
        {
            render.sprite = current[startFrame];
        }

        setStatus("Found " + found.Count + " for " + name, "states: " + String.Join(", ", states.Keys), "current is " + state + " with " + current?.Length + "frames", "current at 0: " + render.sprite);
    }

    void setStatus(params string[] arr)
    {
        status = arr;
    }

    public string nextUp;

    public void Activate(string name, bool playToEnd = false)
    {
        if (this.playToEnd)
        {
            nextUp = name;
            return;
        }

        if (state != name)
        {
            this.playToEnd = playToEnd;
            state = name;
            if (states.TryGetValue(state, out current))
            {
                counter = startFrame;
                render.sprite = current[startFrame];
            }
        }
    }

    void Update()
    {
        if (state != lastState)
        {
            Activate(state);
        }

        if (current != null)
        {
            counter += speed * Time.deltaTime;

            i = (int)(counter % current.Length);
            var index = Math.Max(0, i);
            if(index < current.Length)
                render.sprite = current[index];

            if ((int)(counter % (current.Length + 1)) > current.Length - 1)
            {
                counter = -pauseAfterCycle;

                if (playToEnd) { 
                    playToEnd = false;
                    if(nextUp != null)
                    { 
                        Activate(nextUp);
                        nextUp = null;
                    }
                }
            }

        }
        lastState = state;
    }

    public Sprite GetFirstOfState(string s)
    {
        Sprite[] res;
        if (states.TryGetValue(s, out res) && res.Length > 0)
        {
            return res[0];
        }
        return null;
    }
}
