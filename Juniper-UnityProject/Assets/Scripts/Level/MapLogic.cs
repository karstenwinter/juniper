using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Tilemaps;

public class MapRepresentation {
	public string representation;
	public Edge edge = Edge.None;
    public MapRepresentation(string r, Edge e) { representation = r; edge = e; }
}

public static class MapLogic {
    public static MapRepresentation[] dictionary = new [] {
        new MapRepresentation("_", Edge.Bottom),
        new MapRepresentation("^", Edge.Top),
        new MapRepresentation("(", Edge.Left),
        new MapRepresentation(")", Edge.Right),
        new MapRepresentation("=", Edge.Bottom | Edge.Top),
        new MapRepresentation("H", Edge.Left | Edge.Right),
        new MapRepresentation("q", Edge.Right | Edge.Top),
        new MapRepresentation("y", Edge.Right | Edge.Bottom),
        new MapRepresentation("L", Edge.Left | Edge.Bottom),
        new MapRepresentation("p", Edge.Left | Edge.Top),
        new MapRepresentation("U", Edge.Left | Edge.Bottom | Edge.Right),
        new MapRepresentation("A", Edge.Left | Edge.Top | Edge.Right),
        new MapRepresentation("]", Edge.Top | Edge.Right | Edge.Bottom),
        new MapRepresentation("[", Edge.Top | Edge.Left | Edge.Bottom),
        new MapRepresentation("#", Edge.Top | Edge.Left | Edge.Bottom | Edge.Right)
    };

	public static string[] edgesToStrings(Edge[][] array) {
        if(array == null || array.Length == 0 || array[0] == null)
            return null;

		var startX = 0;
        var startY = 0;
        var h = array.Length;
        var w = array[0].Length;
		var res = new string[h];
        var sb = new StringBuilder();
        for(int y = startY; y < h; y++) {
            sb.Clear();
            var row = array[y];
            for(int x = startX; x < w; x++) {
                var e = row[x];
                if(e == Edge.None)
                    sb.Append(" ");
                else 
                    sb.Append(edgeToString(e));
            }
            res[y] = sb.ToString();
        }
		return res;
	}

	public static Edge[][] stringsToEdges(string[] array) {
        if(array == null || array.Length == 0 || array[0] == null)
            return null;
            
		var startX = 0;
        var startY = 0;
        var h = array.Length;
        var w = array[0].Length;
		var res = new Edge[h][];
        for(int y = startY; y < h; y++) {
            var row = new Edge[w];
            var rowToRead = array[y];
            for(int x = startX; x < w; x++) {
                var str = rowToRead[x].ToString();
                if(str == " ")
                    row[x] = Edge.None;
                else
                    row[x] = stringToEdge(str);
            }
            res[y] = row;
        }
		return res;
	}

    static string edgeToString(Edge e) {
        foreach (var item in dictionary)
        {
            if(item.edge == e)
                return item.representation;
        }
        throw new Exception("not found: " + e);
    }

    static Edge stringToEdge(string e) {
        foreach (var item in dictionary)
        {
            if(item.representation == e)
                return item.edge;
        }
        throw new Exception("not found: " + e);
    }

    public static Edge getEdgesFor(Vector3Int pos, Tilemap tilemap)
    {
        var x = pos.x;
        var ty = pos.y;
        var xy = tilemap.GetTile(new Vector3Int(x, ty, 0));
        var left = tilemap.GetTile(new Vector3Int(x - 1, ty, 0));
        var top = tilemap.GetTile(new Vector3Int(x, ty + 1, 0));
        var bottom = tilemap.GetTile(new Vector3Int(x, ty - 1, 0));
        var right = tilemap.GetTile(new Vector3Int(x + 1, ty, 0));
        var edges = Edge.None
            | ((xy && !left) ? Edge.Left : Edge.None)
            | ((xy && !right) ? Edge.Right : Edge.None)
            | ((xy && !bottom) ? Edge.Bottom : Edge.None)
            | ((xy && !top) ? Edge.Top : Edge.None);
        return edges;
    }

    public static Edge getEdgesFor(Vector3Int pos, long[][] tilemap)
    {
        var x = pos.x;
        var ty = pos.y;
        var xy = tilemap.GetTile(new Vector3Int(x, ty, 0)) != null;
        var left = tilemap.GetTile(new Vector3Int(x - 1, ty, 0)) != null;
        var top = tilemap.GetTile(new Vector3Int(x, ty + 1, 0)) != null;
        var bottom = tilemap.GetTile(new Vector3Int(x, ty - 1, 0)) != null;
        var right = tilemap.GetTile(new Vector3Int(x + 1, ty, 0)) != null;
        var edges = Edge.None
            | ((xy && !left) ? Edge.Left : Edge.None)
            | ((xy && !right) ? Edge.Right : Edge.None)
            | ((xy && !bottom) ? Edge.Bottom : Edge.None)
            | ((xy && !top) ? Edge.Top : Edge.None);
        return edges;
    }
}

static class TmxTilemap
{
    
}