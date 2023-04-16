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


public class GenPrefabs : MonoBehaviour
{
    public Transform player;

    const int w = 100;
    const int h = 100;
    List<GameObject>[,] grid;

    public Vector2Int lastIdx;

    public bool instantiateAllAsActiveForTest = false;

    public bool useTestPos = false;
    public int testX, testY;

    public int fineX = 25, fineY = 15;

    public GameObject blackProto;
    public Transform genBlackParent, images;
    public Tilemap level;
    public Text textProto;
    public Canvas textCanvas;

    public static void LoadAgain()
    {
        var gen = UnityEngine.Object.FindObjectOfType<GenPrefabs>();
        try
        {
            gen?.Load();
        }
        catch (Exception e)
        {
            Global.HandleError(e);
        }
    }

    void Start()
    {
        Load();
        Global.soundManager.PlayMusic("UpperCave");
    }

    [ContextMenu("EmptyCacheAndLoad")]
    public void EmptyCacheAndLoad()
    {
        Caches.Clear();
        Load();
    }

    void PutIntoGrid(GameObject gameObject)
    {
        var idx = getIndex(gameObject.transform.position);
        var listForPos = grid[idx.x, idx.y];
        if (listForPos == null)
        {
            listForPos = new List<GameObject>();
            grid[idx.x, idx.y] = listForPos;
        }
        listForPos.Add(gameObject);
    }

    public void Reload()
    {
        Load();
    }

    public void Load()
    {
        if (grid != null)
            DisableAll();

        grid = new List<GameObject>[w, h];

        LayersAndTags();

        ClearPrefabs();

        foreach (var item in Caches.Tilemap.blackAreas)
        {
            genBlackArea(item);
        }

        InstantiatePrefabs();

        if (player != null)
        {
            var pos = player.transform.position;
            var posIdx = getIndex(pos);
            setForPos(posIdx);
        }
    }

    [ContextMenu("Clear")]
    public void ClearPrefabs()
    {
        foreach (var item in transform.Cast<Transform>().ToArray())
        {
            DestroyImmediate(item.gameObject);
        }
        foreach (var item in genBlackParent.Cast<Transform>().ToArray())
        {
            DestroyImmediate(item.gameObject);
        }

        foreach (var item in textCanvas.transform.Cast<Transform>().ToArray())
        {
            DestroyImmediate(item.gameObject);
        }
    }

    private void InstantiatePrefabs()
    {
        Debug.Log("loading tilesets...");
        var t = Caches.Tileset;
        var objectsTileset = t;
        Debug.Log("done loading tilesets, found entries: " + objectsTileset.entries.Length);

        var sp = 4;
        var y = -1;
        foreach (var row in Caches.Tilemap.objects)
        {
            y++;
            var x = -1;
            foreach (var value in row)
            {
                x++;

                if (value == 0)
                    continue;

                string path = "";
                foreach (var item in t.entries)
                {
                    if (item.id == value)
                    {
                        path = "Prefabs/" + item.name;
                        break;
                    }
                }
                if (path == "")
                    continue;

                var o = Resources.Load<GameObject>(path);
                if (o == null)
                    Debug.LogError("not found: <" + path + "> (value was " + value + ")");

                else
                {
                    GameObject inst;
#if UNITY_EDITOR

                    inst = GameObject.Instantiate(o, transform);

#else
                    inst = GameObject.Instantiate(o, transform);
#endif


                    inst.transform.localPosition = new Vector3(x + 0.5f, -y + 2, 0);
                    inst.name = value + "@y" + -y + "x" + x + " " + path;
                    var rend = inst.GetComponentInChildren<SpriteRenderer>();
                    if (rend != null)
                    {
                        rend.sortingOrder = sp++;
                    }

                    if (!instantiateAllAsActiveForTest)
                        inst.SetActive(false);

                    PutIntoGrid(inst.gameObject);
                }
            }
        }
    }

    public static void SetPlayer(Transform transform)
    {
        var comp = UnityEngine.Object.FindObjectOfType<GenPrefabs>();
        if (comp != null)
            comp.player = transform;
    }

    public void Update()
    {
        if (player == null)
            return;

        var pos = player.transform.position;
        var posIdx = getIndex(pos);
        if (lastIdx.x != posIdx.x || lastIdx.y != posIdx.y)
        {
            if (Math.Abs(lastIdx.x - posIdx.x) > 1 || Math.Abs(lastIdx.y - posIdx.y) > 1)
            {
                DisableAll();
            }

            setForPos(posIdx);

            lastIdx = posIdx;
        }
    }

    Vector2Int getIndex(Vector3 pos)
    {
        var indX = (int)(pos.x / fineX);
        var indY = (int)(-pos.y / fineY);
        indX = Mathf.Clamp(indX, 0, w - 1);
        indY = Mathf.Clamp(indY, 0, h - 1);
        return new Vector2Int(indX, indY);
    }

    [ContextMenu("disableAll")]
    public void DisableAll()
    {
        setActiveAll(false);
    }


    [ContextMenu("EnableAll")]
    public void EnableAll()
    {
        setActiveAll(true);
    }

    void setActiveAll(bool value)
    {
        if (grid == null)
            return;

        for (int dy = 0; dy < h; dy++)
        {
            for (int dx = 0; dx < w; dx++)
            {
                var list = grid[dx, dy];
                if (list != null)
                    foreach (var item in list)
                    {
                        try
                        {
                            item.SetActive(false);
                        }
                        catch
                        {

                        }
                    }
            }
        }
    }

    void setForPos(Vector2Int indexToUpdate)
    {
        var foundEnemy = false;
        for (int dy = -2; dy <= 2; dy++)
        {
            var idxY = Mathf.Clamp(indexToUpdate.y + dy, 0, h - 1);
            for (int dx = -2; dx <= 2; dx++)
            {
                var idxX = Mathf.Clamp(indexToUpdate.x + dx, 0, w - 1);
                var listForPos = grid[idxX, idxY];
                if (listForPos == null)
                    continue;
                var act = Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1;
                foreach (var otherGameObject in listForPos)
                {

                    otherGameObject.gameObject.SetActive(act);
                    if (!foundEnemy && otherGameObject.name.Contains("Enemy"))
                    {
                        Global.soundManager.EnterFight();
                        foundEnemy = true;
                    }
                }
            }
        }
        if (!foundEnemy)
            Global.soundManager.ExitFight();
    }

    private void LayersAndTags()
    {
        if (images != null)
            foreach (var item in images?.Cast<Transform>()?.ToArray() ?? new Transform[0])
            {
                var b = item.GetComponent<BoxCollider2D>();
                if (b != null)
                    b.isTrigger = true;
            }

        var layer = LayerMask.NameToLayer("Platform");
        var n = 0;
        foreach (var edge in level.GetComponentsInChildren<EdgeCollider2D>())
        {
            edge.isTrigger = false;
            edge.tag = "Platform";
            edge.gameObject.layer = layer;
            n++;
        }
        Debug.Log("changed " + n + " edges to Platform (" + layer + ")");

        var old = level.transform.parent.Find("GEN Level");
        if (old != null)
            DestroyImmediate(old.gameObject);


        var lvlTilemap = Instantiate(level.gameObject, level.transform.parent);
        lvlTilemap.name = "GEN Level";
        lvlTilemap.transform.position += new Vector3(0, 0, -1f);
        lvlTilemap.GetComponent<TilemapRenderer>().sortingOrder = 2000;

        foreach (var item in lvlTilemap.transform.Cast<Transform>().ToArray())
        {
            DestroyImmediate(item.gameObject);
        }
        var t = lvlTilemap.GetComponent<Tilemap>();
        t.SetTilesBlock(level.cellBounds, level.GetTilesBlock(level.cellBounds));
    }

    void genBlackArea(TileFileArea blackArea)
    {
        var x = blackArea.x;
        var y = -blackArea.y;

        var width = blackArea.width;
        var height = blackArea.height;

        var blackGameObject = Instantiate(blackProto, genBlackParent);
        blackGameObject.SetActive(true);

        blackGameObject.transform.localPosition =
            new Vector3(x + width / 2f, y - height / 2f, blackGameObject.transform.position.z);

        var coll = blackGameObject.GetComponent<BoxCollider2D>();
        var sprite = blackGameObject.GetComponent<SpriteRenderer>();
        sprite.size = new Vector2(
            width + 0.5f, height + 0.5f
        );

        coll.size = new Vector2(
            width, height
        );

        var fadeComp = blackGameObject.GetComponent<ObjectFade>();
        fadeComp?.ResetState();
    }
}
