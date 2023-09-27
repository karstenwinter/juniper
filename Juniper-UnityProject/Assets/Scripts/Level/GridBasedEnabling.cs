using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GridBasedEnabling : MonoBehaviour
{
    Transform player;

    const int w = 100;
    const int h = 100;
    List<GameObject>[,] grid = new List<GameObject>[w, h];

    public Vector2Int lastIdx;
    public bool test;
    public int testX, testY;
    public int fineX = 16, fineY = 16;

    public string status = "loading", status2 = "current cell";
    public void OnValidate()
    {
        if (test)
        {
            if (grid == null)
                Load();

            var p = new Vector3(testX, testY, 0);
            var old = player.transform.position;
            player.transform.position = p;
            transform.position = p;
            Update();
            player.transform.position = old;
        }
    }

    public void StartManually()
    {
        Load();
        DisableAll();

        var pos = GameObject.Find("GlobalPlayerStart").transform.position;
        var posIdx = getIndex(pos);
        setForPos(pos, posIdx);
    }

    [ContextMenu("Load")]
    public void Load()
    {
        DisableAll();

        int found = 0, lists = 0, dis = 0;
        grid = new List<GameObject>[w, h];
        foreach (var item in GameObject.FindObjectsOfTypeAll(typeof(InteractableObject)) as InteractableObject[])
        {
            // if(item.transform.parent?.name == "PrefabInstanceParent") // GEN Prefabs
            {
                var idx = getIndex(item.transform.position);
                var listForPos = grid[idx.x, idx.y];
                if (listForPos == null)
                {
                    listForPos = new List<GameObject>();
                    grid[idx.x, idx.y] = listForPos;
                    lists++;
                }
                found++;
                listForPos.Add(item.gameObject);
            }
            //else
            //{
            //    dis++;
            //}
        }
        status = "Found: " + found + ", lists: " + lists + ", disabled: " + dis;
    }

    public void Update()
    {
        player = Global.playerController?.transform;
        if (!player)
            return;

        var pos = player.transform.position;
        var posIdx = getIndex(pos);
        if (lastIdx.x != posIdx.x || lastIdx.y != posIdx.y)
        {
            if (Math.Abs(lastIdx.x - posIdx.x) > 1 || Math.Abs(lastIdx.y - posIdx.y) > 1)
            {
                DisableAll();
            }

            setForPos(pos, posIdx);

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

    void setForPos(Vector3 playerPos, Vector2Int indexToUpdate)
    {
        var activeInCell = 0;
        var inactiveInCell = 0;
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
                    if (act)
                        activeInCell++;
                    else
                        inactiveInCell++;
                }
            }
        }
        status2 = "last idx " + indexToUpdate + " act " + activeInCell + " inactive " + inactiveInCell + " for player pos" + playerPos;
    }
}
