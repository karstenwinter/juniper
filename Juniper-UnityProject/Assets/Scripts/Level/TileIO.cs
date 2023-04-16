using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class TileFileArea
{
    public long x, y, width, height;
    public string name = "", id = "", biome = "UpperCave", biomeObjects = "Object";
}
public class TileFileTileset
{
    public long firstGid;
    public string name = "", source = "";
    public TileFileTilesetEntry[] entries;
}
public class TileFileTilesetEntry
{
    public long id;
    public string name = "";
}

public class TileFileLayer
{
    public string name = "", csv = "";
    public float parallaxx, parallaxy;
    public long[][] data;
}
public class TileFile
{
    public static int size = 512;

    public long[][] bgFar, bgMid, bgNear, objects, level, fgBeforePlayer, fgNearCamera;
    //public long[][] old_objects;


    public TileFileArea[] areas = new TileFileArea[0];
    public TileFileArea[] blackAreas = new TileFileArea[0];

    public TileFileLayer[] rawLayersCsv = new TileFileLayer[0];
    public TileFileTileset[] tilesets;
}

public class TileIO : MonoBehaviour
{
    static int tileSize = 120;

    public static TileFile ParseTMX(string tmxFileContent, string tsxFileContent)
    {
            var res = new TileFile();
        //try {
        var doc = XDocument.Parse(tmxFileContent);
        var layers = doc.Element("map").Elements("layer");
        var i = 0;
        var objectgroups = doc.Element("map").Element("objectgroup")?.Elements("object")?.Select(x =>
        {
            i++;
            var name = x.Attribute("name")?.Value ?? "name" + i;
            return new TileFileArea
            {
                id = x.Attribute("id")?.Value ?? "id" + i,
                name = name,
                x = long.Parse(x.Attribute("x")?.Value ?? "0") / tileSize,
                y = long.Parse(x.Attribute("y")?.Value ?? "0") / tileSize,
                width = long.Parse(x.Attribute("width")?.Value ?? "160") / tileSize,
                height = long.Parse(x.Attribute("height")?.Value ?? "160") / tileSize
            };
        }).ToArray();

        res.tilesets = doc.Element("map").Elements("tileset").Where(x => x.Attribute("source").Value.Contains("Prefabs")).Select(x =>
        {
            var firstGid = (long)x.Attribute("firstgid").Value.ToFloat();
            Debug.Log("tsxFileContent: " + tsxFileContent + ", firstGid: " + firstGid);
            var doc2 = XDocument.Parse(tsxFileContent);
            var ch = doc2.Element("tileset").Elements("tile");
            return new TileFileTileset
            {
                name = "Prefabs",
                firstGid = firstGid,
                entries = ch.Select(t => Tuple.Create(t, t?.Element("image"))).Where(t => t.Item2 != null).Select(tuple =>
                {
                    var f = (long)tuple.Item1.Attribute("id")?.Value.ToFloat();

                    return new TileFileTilesetEntry
                    {
                        id = firstGid + f,
                        name = getPrefabName(tuple.Item2.Attribute("source")?.Value)
                    };
                }
                ).ToArray()
            };
        }).ToArray();

        res.areas = objectgroups.Where(x => x.name != "black").ToArray();
        res.blackAreas = objectgroups.Where(x => x.name == "black").ToArray();
        res.rawLayersCsv = layers.Select(x =>
           new TileFileLayer
           {
               name = x.Attribute("name").Value,
               csv = x.Element("data").FirstNode.ToString(),
               parallaxx = (x.Attribute("parallaxx")?.Value).ToFloat(),
               parallaxy = (x.Attribute("parallaxy")?.Value).ToFloat()
           }
        ).ToArray();

        foreach (var layer in new[] {  "Level", "Objects" })
        {
            var layerEntry = Array.Find(res.rawLayersCsv, x => x.name == layer);
            var textRaw = layerEntry?.csv ?? "";

            var rowsSplit = textRaw
                .Replace("\r", "")
                .Replace("\t", "")
                .Replace(" ", "")
                .Split('\n');

            long[][] current = null;

            if (layer == "Level")
                current = res.level = new long[rowsSplit.Length][];
            if (layer == "Objects")
                current = res.objects = new long[rowsSplit.Length][];
            layerEntry.data = current;

            var y = -1;

            foreach (string row in rowsSplit)
            {
                y++;
                var split = row.Split(',');

                current[y] = new long[split.Length];
                int x = -1;
                foreach (string col in split)
                {
                    x++;
                    if (col != "" && col != "0")
                    {
                        long value;
                        if (long.TryParse(col, out value))
                        {
                            current[y][x] = value;
                        }
                    }
                }
            }
        }
        return res;
    }

    private static string getPrefabName(string value)
    {
        return value.Split('_')[1].Split('.')[0];
    }
}
