using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Extensions
{
    public static string GetId(this MonoBehaviour behaviour, string pref)
    {
        return pref + "x" + (int)behaviour.transform.position.x + "y" + (int)behaviour.transform.position.y;
    }
    
    public static float ToFloat(this string s)
    {

        float value = 0;
        if(s != null)
            float.TryParse(s, out value);
        return value;
    }
    
    public static T As<T>(this string s, bool logErrorOnFail = true)
    {
        try
        {
            return (T)Enum.Parse(typeof(T), s);
        }
        catch (Exception e)
        {
            if (logErrorOnFail)
                Debug.LogError("string as enum " + typeof(T).Name + ": " + s + " => " + e);
            return default(T);
        }
    }

    public static T Next<T>(this T src) where T : struct
    {
        var arr = (T[])Enum.GetValues(typeof(T));
        var j = Array.IndexOf<T>(arr, src) + 1;
        return arr.Length == j ? arr[0] : arr[j];
    }

    public static InputType NextForInput(this InputType src)
    {
        InputType[] arr = new[] {
                InputType.KeyboardGamepad, InputType.Touchpad, InputType.TouchpadInvisible
            //#if UNITY_ANDROID
              //  InputType.Keyboard, InputType.DualshockAndroid, InputType.XBox, InputType.Touchpad, InputType.TouchpadInvisble, InputType.Custom
            //#else
              //  InputType.Keyboard, InputType.DualshockPC, InputType.XBox, InputType.Custom
             //#endif
        };

        var j = Array.IndexOf(arr, src) + 1;
        return arr.Length == j ? arr[0] : arr[j];
    }

    public static float ToFloat(this Difficulty x)
    {
        switch (x)
        {
            case Difficulty.Easy: return 0.75f;
            default:
            case Difficulty.Normal: return 1f;
            case Difficulty.Hard: return 1.5f;
        }
    }

    public static float ToFloat(this AudioVolume x)
    {
        switch (x)
        {
            case AudioVolume.Low: return 0.33f;
            case AudioVolume.Mid: return 0.66f;
            case AudioVolume.High: return 1.0f;
            default: return 0f;
        }
    }

    public static int GetDeltaX(this Edge x)
    {
        switch (x)
        {
            case Edge.Left: return -1;
            case Edge.Right: return 1;
            default: return 0;
        }
    }

    public static int GetDeltaY(this Edge x)
    {
        switch (x)
        {
            case Edge.Top: return -1;
            case Edge.Bottom: return 1;
            default: return 0;
        }
    }

    public static float ToFloat(this InputThreshold x)
    {
        switch (x)
        {
            case InputThreshold.VeryLow: return 0.05f;
            case InputThreshold.Low: return 0.1f;
            case InputThreshold.Mid: return 0.2f;
            case InputThreshold.High: return 0.3f;
            default: return 0f;
        }
    }

    public static string MakeJoinedHtml<T>(this T selected) where T : Enum
    {
        var values = Enum.GetValues(typeof(T)) as T[];
        return MakeJoinedHtmlFromArray(values, selected);
    }

    static string MakeJoinedHtmlFromArray<T>(T[] values, T selected)
    {
        return String.Join(" | ",
            Array.ConvertAll(
                Array.FindAll(values, x => x.ToString() != "Test"), x =>
                x.Equals(selected)
                    ? Bold(Translations.For(x.ToString()))
                    : Translations.For(x.ToString()))
        );
    }

    public static string MakeJoinedHtmlForInput(this InputType selected)
    {
        InputType[] values = new[] {
            //#if UNITY_ANDROID
            //    InputType.Keyboard, InputType.DualshockAndroid, InputType.XBox, InputType.Touchpad, InputType.Custom
            //#else
                InputType.KeyboardGamepad, InputType.Touchpad, InputType.TouchpadInvisible
             //#endif
        };
        return MakeJoinedHtmlFromArray(values, selected);
    }

    public static string Bold(string s) { return "<b>" + s + "</b>"; }

    public static void PlayIfNotPlaying(this ParticleSystem particles)
    {
        if (particles != null && !particles.isPlaying)
        {
            particles.Play();
        }
    }

    public static bool RemoveComponent<T>(this GameObject gameObject) where T : UnityEngine.Object
    {
        T comp = gameObject.GetComponent<T>();
        if (comp != null)
        {
            UnityEngine.Object.Destroy(comp);
            return true;
        }
        return false;
    }

    public static long? GetTile(this long[][] tmx, Vector3Int pos)
    {
        var y = -pos.y;
        if (0 <= y && y < tmx.Length)
        {
            var row = tmx[y];
            if (0 <= pos.x && pos.x < row.Length)
            {
                var value = row[pos.x];
                return value == 0 ? null : (long?)value;
            }
        }
        return null;
    }


    public static TileFileArea GetNeighbour(this TileFile world, TileFileArea current, Edge edge)
    {
        return edge == Edge.Right ? Array.Find(world.areas, other => other.x == current.x + current.width && other.y == current.y)
             : edge == Edge.Top ? Array.Find(world.areas, other => other.x == current.x && other.y == current.y - current.height)
             : edge == Edge.Left ? Array.Find(world.areas, other => other.x == current.x - current.width && other.y == current.y)
             : edge == Edge.Bottom ? Array.Find(world.areas, other => other.x == current.x && other.y == current.y + current.height)
             : null;
    }

    public static TileFileArea[] GetBlack(this TileFile world, TileFileArea current)
    {
        return Array.FindAll(world.blackAreas, other => current.Overlaps(other));
    }

    public static bool Overlaps(this TileFileArea area, TileFileArea other)
    {
        return area.x <= other.x && other.x <= area.x + area.width
               && area.y <= other.y && other.y <= area.y + area.height;
    }

    public static bool Contains(this TileFileArea area, TileFileArea other)
    {
        return area.x <= other.x && other.x <= area.x + area.width
               && area.y <= other.y && other.y <= area.y + area.height;
    }

    public static string ToJson(this object data)
    {
        return JsonUtility.ToJson(data, true);
    }

    public static string GetBiome(this string currentArea)
    {
        var split = currentArea.Split('.');
        if (split.Length == 3)
            return split[2];
        else
            return split[0];
    }

    public static string GetCleanedAreaName(this string currentArea)
    {
        var split = currentArea.Split('.');
        if (split.Length == 3)
            return split[2] + "." + split[1];
        else
            return currentArea;
    }

    public static string FormatPlayTime(this float timeInSec)
    {
        return (int)timeInSec + "s";
    }


    public static void DestroyOrImmediate(this GameObject gameObject)
    {
#if UNITY_EDITOR
                GameObject.DestroyImmediate(gameObject);
#else
        GameObject.Destroy(gameObject);
#endif
    }

    public static void DestroyOrImmediate(this MonoBehaviour beh)
    {
#if UNITY_EDITOR
        GameObject.DestroyImmediate(beh);
#else
        GameObject.Destroy(beh);
#endif
    }

    public static GameObject InstantiateOrPrefab(this GameObject gameObject)
    {
        //    #if UNITY_EDITOR
        //        return PrefabUtility.InstantiatePrefab(gameObject) as GameObject;
        //    #else
        return GameObject.Instantiate(gameObject);
        //    #endif
    }
}