using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

[Serializable]
public class MinimapInfo
{
    public string edgeCombination = "";
}

public struct TileFileAreaWithColor
{
    public TileFileArea area;
    public Color color;
}

public class MinimapUI : MonoBehaviour
{
    public bool setPosEveryFrame;
    public Tilemap mutableTilemap;
    public Vector3 playerPosScale = new Vector3(-0.1f, -0.1f, 1f);
    public Vector3 playerPosOffset = new Vector3(50f, -6f, 0);
    public SpriteRenderer playerSprite;
    public SpriteRenderer black;

    internal string[] onLoad;
    public int unveilDelta = 3;

    internal int oldX, oldY;
    public MinimapInfo[] tiles = new MinimapInfo[0];
    Dictionary<Edge, TileBase> tilesDict = new Dictionary<Edge, TileBase>();
    static int w = 512;
    static int h = 512;
    Dictionary<Vector2Int, TileFileAreaWithColor> areasForPos = new Dictionary<Vector2Int, TileFileAreaWithColor>();
    Dictionary<string, TileBase> icons = new Dictionary<string, TileBase>();

    internal Edge[][] edgesArray = new Edge[0][];

    TileBase getIcon(string name)
    {
        TileBase tile;
        if (icons.TryGetValue(name, out tile))
            return tile;

        var sprite = Resources.Load<Sprite>("Minimap/Icons/" + name);
        tile = tileForSprite(sprite);
        icons[name] = tile;
        return tile;
    }

    SimpleSpriteTile tileForSprite(Sprite sp)
    {
        var simpleSpriteTile = (SimpleSpriteTile)ScriptableObject.CreateInstance(typeof(SimpleSpriteTile));
        simpleSpriteTile.sprite = sp;
        return simpleSpriteTile;
    }

    // Before start, important!
    void Awake()
    {
        Global.minimapUI = this;

        edgesArray = new Edge[h][];
        for (int i = 0; i < h; i++)
            edgesArray[i] = new Edge[w];

        mutableTilemap.gameObject.SetActive(false);
        foreach (var item in tiles)
        {
            var edge = parseEdges(item.edgeCombination);
            var sprite = Resources.Load<Sprite>("Minimap/Borders/" + item.edgeCombination);
            tilesDict[edge] = tileForSprite(sprite);
        }

        /*return;

        var tmx = Caches.Tilemap;
        var areas = Caches.AreaData;
        foreach (var item in tmx.areas)
        {
            var biome = item.name.GetBiome();
            var areaData = Array.Find(areas, x => x.name == biome);
            var tone = areaData?.tone ?? "";
            Color parsedColor;
            if (!ColorUtility.TryParseHtmlString(tone, out parsedColor))
            {
                parsedColor = new Color(1, 1, 1);
                if (tone != "")
                    Global.HandleError("Cannot parse color: " + tone + " for " + biome);
            }
            float h, s, v;
            Color.RGBToHSV(parsedColor, out h, out s, out v);
            if (v < 0.8f)
                v = 0.8f;
            var minimapColor = Color.HSVToRGB(h, s, v);
            Global.LogDebug("AREA " + item.name + " => biome: " + biome + " => tone: " + tone + " => parse " + parsedColor + " minimap Color: " + minimapColor + " => areaData: " + (areaData == null ? "none" : "found"));

            for (int dx = 0; dx < (int)item.width; dx++)
            {
                for (int dy = 0; dy < (int)item.height; dy++)
                {
                    areasForPos[new Vector2Int((int)item.x + dx, (int)item.y + dy)] = new TileFileAreaWithColor { area = item, color = minimapColor };
                }
            }
        }*/
    }

    Edge parseEdges(string s)
    {
        var res = Edge.None;
        foreach (var p in s.Split(' '))
        {
            res |= p.As<Edge>();
        }
        return res;
    }

    void Update()
    {
        if (Global.playerController != null)
        {
            if (Global.playerController.state.canShowMap)
            {
                var oldState = mutableTilemap.gameObject.activeInHierarchy;
                var newState = Global.playerController.Map;
                if (oldState != newState)
                    ToggleMap(newState);
            }
            var player = Global.playerController.transform;
            check(player.position);

            if (setPosEveryFrame)
                SetPos(player.position);
        }
    }

    public void ToggleMinimap()
    {
        if (Global.playerController?.state.canShowMap ?? false)
        {
            ToggleMap(!mutableTilemap.gameObject.activeInHierarchy);
        }
    }

    public void ToggleMap(bool visible)
    {
        Debug.Log("toggleMap" + visible);
        mutableTilemap.gameObject.SetActive(visible);
        if (visible)
        {
            var c = black.color;
            c.a = 0.3f;
            black.color = c;
        }
        else
        {
            var c = black.color;
            c.a = 0f;
            black.color = c;
        }
    }

    void Stop()
    {
        mutableTilemap.gameObject.SetActive(false);
    }

    public void refreshFromNewArray(Edge[][] newEdges)
    {
        if (newEdges == null || (mutableTilemap?.gameObject?.activeInHierarchy ?? false))
            return;

        var h = newEdges.Length;
        var w = newEdges[0].Length;
        for (int y = 0; y < h; y++)
        {
            var row = newEdges[y];
            for (int x = 0; x < w; x++)
            {
                var pos = new Vector3Int(x, -y, 0);
                mutableTilemap.SetTile(pos, getTileFor(row[x]));

                colorPos(pos, x, -y, false);
            }
        }
        edgesArray = newEdges;
    }

    public void SetPos(Vector3 newMapPos)
    {
        var v = new Vector3(newMapPos.x * playerPosScale.x, newMapPos.y * playerPosScale.y, 0) + playerPosOffset;
        v.z = transform.localPosition.z;
        transform.localPosition = v;
    }

    public void check(Vector3 playerPosition)
    {
        var y = (int)playerPosition.y;
        var x = (int)playerPosition.x;

        if (oldX != x || oldY != y)
        {
            setForPos(oldX, oldY, false);
            oldX = x;
            oldY = y;

            for (int dy = -unveilDelta; dy <= unveilDelta; dy++)
            {
                for (int dx = -unveilDelta; dx < unveilDelta; dx++)
                {
                    if ((dx != -unveilDelta && dy != -unveilDelta)
                        || (dx != unveilDelta - 1 && dy != unveilDelta - 1)
                        || (dx != unveilDelta - 1 && dy != -unveilDelta)
                        || (dx != -unveilDelta && dy != unveilDelta - 1)) // circle
                    {
                        var tx = x + dx;
                        var ty = y + dy;
                        setForPos(tx, ty, dx == 0 && dy == 0);
                    }
                }
            }
        }
    }

    void setForPos(int tx, int ty, bool selfTile)
    {
        var pos = new Vector3Int(tx, ty, 0);
        var arrayOk = tx >= 0 && tx < edgesArray.Length && -ty >= 0 && -ty < edgesArray.Length;

        var tilemapCached = Caches.Tilemap;
        if (tilemapCached == null)
            return;

        var obj = tilemapCached.objects.GetTile(pos);

        var edges = selfTile ? Edge.None
                    : MapLogic.getEdgesFor(pos, tilemapCached.level);
        if (arrayOk)
            edgesArray[-ty][tx] = edges;

        var playerSkill = Global.playerController.state;
        var isCampTile = playerSkill.minimapShowCamp && Array.IndexOf(Caches.NumbersForCamp, obj) >= 0;
        var isShellTile = playerSkill.minimapShowShells && Array.IndexOf(Caches.NumbersForShells, obj) >= 0;
        var isBossTile = playerSkill.minimapShowBoss && Array.IndexOf(Caches.NumbersForBoss, obj) >= 0;

        var tileForPos =
        selfTile ? getIcon("Gecko")
        : isCampTile ? getIcon("Camp")
        : isShellTile ? getIcon("Shells")
        : isBossTile ? getIcon("Boss")
        : edges != Edge.None
        ? getTileFor(edges)
        : null;

        mutableTilemap.SetTile(pos, tileForPos);

        colorPos(pos, tx, ty, isCampTile || isBossTile || isShellTile || selfTile);
    }

    void colorPos(Vector3Int pos, int tx, int ty, bool white)
    {
        Color color = new Color(1, 1, 1);

        TileFileAreaWithColor area;
        if (!white && areasForPos.TryGetValue(new Vector2Int(tx, -ty), out area))
        {
            color = area.color;
        }

        mutableTilemap.SetTileFlags(pos, TileFlags.None);
        mutableTilemap.SetColor(pos, color);
        mutableTilemap.RefreshTile(pos);
    }

    TileBase getTileFor(Edge edge)
    {
        return edge == Edge.None || !tilesDict.ContainsKey(edge) ? null : tilesDict[edge];
    }
}
