using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class ColorTest : MonoBehaviour
{
    public Tilemap mutableTilemap;
    public int detectX, detectY;
    public Sprite tileS, tile2S;
    public Color color = new Color(1, 0, 0);
    public int h = 12;
    public int w = 12;

    public void Start()
    {
        Load();
    }

    SimpleSpriteTile tileForSprite(Sprite sp)
    { 
        var simpleSpriteTile = (SimpleSpriteTile)ScriptableObject.CreateInstance(typeof(SimpleSpriteTile));
        simpleSpriteTile.sprite = sp;
        return simpleSpriteTile;
    }

    [ContextMenu("Test")]
    public void Load()
    {
        TileBase tile = tileForSprite(tileS);
        TileBase tile2 = tileForSprite(tile2S);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var pos = new Vector3Int(x, -y, 0);
                mutableTilemap.SetTile(pos, tile);

                if(x > detectX && y > detectY)
                { 
                    mutableTilemap.SetTile(pos, tile2);
                    mutableTilemap.SetTileFlags(pos, TileFlags.None);
                    mutableTilemap.SetColor(pos, color);
                    mutableTilemap.RefreshTile(pos);
                }

            }
        }
    }
}
