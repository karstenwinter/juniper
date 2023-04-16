using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimatedTestObject : MonoBehaviour
{
    SpriteRenderer render;
    Sprite current;
    public bool isCharacter;

    public string state = "idle";
    float counter;
    public float speed = 1f;
    public float unit = 240f;
    public float pixlesPerUnit = 120f;
    public Vector2 pos = new Vector2(0.5f, 0.5f);
    public Vector2Int posInSheet;

    public string dummy;
    string lastDummy;

    void Start()
    {
        Load();
    }

    [ContextMenu("Load")]
    public void Load()
    {
        render = GetComponent<SpriteRenderer>();

        var w = 10;
        var y = posInSheet.y;

        var x = posInSheet.x;
        var sp =
            ExternalDataLoader.LoadSpriteForCharacterOrAnimated(isCharacter, name, x, y, w, unit, pixlesPerUnit, pos);

        render.sprite = sp;
    }

    void Update()
    {
        dummy = posInSheet + " " + unit + " " + pixlesPerUnit;
        if (dummy != lastDummy)
        {
            Load();
            lastDummy = dummy;
        }
    }
}
