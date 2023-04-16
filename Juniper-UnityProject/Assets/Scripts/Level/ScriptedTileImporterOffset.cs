using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class RectPreset
{
    public string name;
    public RectInt rect;
}

[Serializable]
[CreateAssetMenu(fileName = "ScriptedTileImporterOffset", menuName = "ScriptedTileImporterOffset")]
public class ScriptedTileImporterOffset : ScriptableObject
{
    public string preset = "ALL";
    [InspectorName("Preview")]
    public RectInt value;
    public RectPreset[] customPresets =
    {
        new RectPreset { name = "ALL", rect = new RectInt(0, 0, 512, 512) },
        new RectPreset { name = "Spider", rect = new RectInt(150, 230, 50, 60) },
        new RectPreset { name = "Lizard", rect = new RectInt(290, 290, 50, 60) },
        new RectPreset { name = "Snake", rect = new RectInt(411, 280, 50, 60) },
    };

    public void OnValidate()
    {
        var rect = Array.Find(customPresets, x => x.name == preset)?.rect ?? new RectInt(0, 0, 512, 512);
        value = rect;
        Debug.Log("rect " + rect);
        /*try
        {
              SuperTiled2Unity.SuperTileLayer.minCX = rect.min.x;
              SuperTiled2Unity.SuperTileLayer.maxCX = rect.max.x;
              SuperTiled2Unity.SuperTileLayer.minCY = rect.min.y;
              SuperTiled2Unity.SuperTileLayer.maxCY = rect.max.y;
          
        }
        catch (Exception e)
        {

        }*/
    }
}