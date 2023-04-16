using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LazyTilemap : MonoBehaviour
{
    [ContextMenu("Test")]
    void Test()
    {
        Caches.Clear();
        foreach (var item in sources)
        {
            Debug.Log(" TILEMAP " + item.name + " => " + item.size);
        }
        targets = Array.ConvertAll(sources, s => targetParent.transform.Find(s.name).GetComponent<Tilemap>());
        setForPos(testX, testY);
    }

    public Tilemap[] sources;
    public Grid targetParent;

    Tilemap[] targets;
    int lastPosX, lastPosY;
    public bool followsPlayer;

    public int facX = 1;
    public int facY = 1;
    public int w = 10;
    public int h = 10;
    public int testX = 111;
    public int testY = 222;
    public int z = 0;
    public int zSize = 1;
    public int bgFarOffsetX = -12;
    public int bgFarOffsetY = -12;
    public int playerPosOffsetX = -60;
    public int playerPosOffsetY = -60;
    void Start()
    {
        targets = Array.ConvertAll(sources, s => targetParent.transform.Find(s.name).GetComponent<Tilemap>());
    }

    public void Update()
    {
        if(!followsPlayer)
            return;

        var player = Global.playerController;
        if (player == null)
            return;

        var posX = (int)player.transform.position.x;
        var posY = (int)player.transform.position.y;
        if(lastPosX != posX || lastPosY != posY)
        {
            lastPosX = posX;
            lastPosY = posY;

            Debug.Log("Moved y" + posY + "x" + posX);

            setForPos(posX + playerPosOffsetX, posY + playerPosOffsetY);
        }
    }

     void setForPos(int posX, int posY)
    {
        for (int i = 0; i < targets.Length; i++)
        {
            var target = targets[i];
            var source = sources[i];
            if (source.name == "Level")
                continue;

            target.ClearAllTiles();
            //for (int dx = -w; dx < w; dx++)
            {
                //for (int dy = -h; dy < h; dy++)
                {
                    var x = posX;
                    var y = posY;
                    if(source.name == "BG Far")
                    {
                        x += bgFarOffsetX;
                        y += bgFarOffsetY;
                    }
                    var p = new BoundsInt(x - w/2, y - h/2, z, w, h, zSize);
                    target.SetTilesBlock(p, source.GetTilesBlock(p));
                    //var p2 = new Vector3Int(facX * posX + dx, facY * posY + dy, 0);
                    //target.SetTile(p2, source.GetTile(p2));
                }
            }
            target.RefreshAllTiles();
        }
    }

    public void ResetState() 
    {
    }
}
