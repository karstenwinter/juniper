using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class SimpleSpriteTile : TileBase
{
    public Sprite sprite;
 
    // Docs: https://docs.unity3d.com/ScriptReference/Tilemaps.TileBase.GetTileData.html
 
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.flags = TileFlags.None;
    }
}

public class TilemapLoader : MonoBehaviour
{ }