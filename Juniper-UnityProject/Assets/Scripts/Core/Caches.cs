using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class Caches
{
    static TileFile tmx;
    static TileFileTileset tsx;
    static RuneData[] runes;
    static TableData[] tableData;
    static ObjectData[] objectData;
    static CharacterData[] characterData;
    static AnimatedData[] animatedData;
    static AreaData[] areaData;
    static long[] numbersForShells, numbersForCamp, numbersForBoss;

    static Dictionary<string, PlayerTableData> players;
    static DictMap translationsDict;

    public static readonly Dictionary<string, TileBase> tileDict = new Dictionary<string, TileBase>();
    public static readonly Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();
    public static readonly Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();

    public static Dictionary<string, KeyValuePair<Type, System.Reflection.FieldInfo>> fieldLookup = new Dictionary<string, KeyValuePair<Type, System.Reflection.FieldInfo>>();

    public static TileFile Tilemap
    {
        get
        {
            if (tmx != null)
                return tmx;

            //var world = Resources.Load<TileWorld>("TileWorld");
            //var cont = ExternalDataLoader.ReadFileToString(SaveSystem.GetMapPath(world.TmxFileName), "txt");
            //var contTileset = ExternalDataLoader.ReadFileToString(SaveSystem.GetMapPath(world.TsxFileName), "txt");
            //tmx = TileIO.ParseTMX(cont, contTileset);
            return tmx;
        }
    }

    public static TileFileTileset Tileset
    {
        get
        {
            if (tsx != null)
                return tsx;
            return tsx = Array.Find(Tilemap.tilesets, x => x.name == "Prefabs");
        }
    }

    static long[] getNumbersFor(string contains)
    {
        return Array.ConvertAll(Array.FindAll(Caches.ObjectData, x => x.prefab.ToLower().Contains(contains)), x => (long)x.index);
    }

    public static long[] NumbersForShells
    {
        get
        {
            return numbersForShells != null ? numbersForShells
                : (numbersForShells = getNumbersFor("shell"));
        }
    }

    public static long[] NumbersForCamp
    {
        get
        {
            return numbersForCamp != null ? numbersForCamp
                : (numbersForCamp = getNumbersFor("camp"));
        }
    }

    public static long[] NumbersForBoss
    {
        get
        {
            return numbersForBoss != null ? numbersForBoss
                : (numbersForBoss = getNumbersFor("boss"));
        }
    }

    public static RuneData[] RuneData
    {
        get
        {
            if (runes != null)
                return runes;
            runes = ExternalDataLoader.ReadRuneDataFile();
            return runes;
        }
    }

    public static ObjectData[] ObjectData
    {
        get
        {
            if (objectData != null)
                return objectData;
            objectData = ExternalDataLoader.ReadObjectDataFile();
            return objectData;
        }
    }

    public static CharacterData[] CharacterData
    {
        get
        {
            if (characterData != null)
                return characterData;
            characterData = ExternalDataLoader.ReadCharacterDataFile();
            return characterData;
        }
    }

    public static AnimatedData[] AnimatedData
    {
        get
        {
            if (animatedData != null)
                return animatedData;
            animatedData = ExternalDataLoader.ReadAnimatedDataFile();
            return animatedData;
        }
    }

    public static AreaData[] AreaData
    {
        get
        {
            if (areaData != null)
                return areaData;
            areaData = ExternalDataLoader.ReadAreaDataFile();
            return areaData;
        }
    }
    public static Dictionary<string, PlayerTableData> Players
    {
        get
        {
            if (players != null)
                return players;
            players = new Dictionary<string, PlayerTableData>();

            var arr = ExternalDataLoader.ReadPlayerTableDataFile();
            foreach (var item in arr)
            {
                players[item.unityId] = item;
            }
            return players;
        }
    }

    public static TableData[] TableData
    {
        get
        {
            if (tableData != null)
                return tableData;
            tableData = ExternalDataLoader.ReadTableDataFile();
            return tableData;
        }
    }

    public static DictMap Translations
    {
        get
        {
            if (translationsDict != null)
                return translationsDict;

            translationsDict = new DictMap();

            try
            {
                var data = ExternalDataLoader.ReadTranslationDataFile();
                if (data.Length > 0)
                {
                    translationsDict.Clear();
                    foreach (var item in data)
                    {
                        var k = item.key.Trim();
                        if (k.Length > 0 && !k.StartsWith("#"))
                        {
                            translationsDict.Add(Language.EN, k, item.en);
                            translationsDict.Add(Language.DE, k, item.de);
                            translationsDict.Add(Language.ES, k, item.es);
                            translationsDict.Add(Language.RU, k, item.ru);
                            translationsDict.Add(Language.JA, k, item.ja);
                            translationsDict.Add(Language.CN, k, item.cn);
                        }
                    }
                }
                Global.LogDebug("translations: " + data.Length);
            }
            catch (Exception e)
            {
                Global.HandleError(e);
            }
            return translationsDict;
        }
    }

    public static void Clear()
    {
        tmx = null;
        tsx = null;
        runes = null;
        tableData = null;
        objectData = null;
        areaData = null;
        characterData = null;
        animatedData = null;

        numbersForShells = numbersForCamp = numbersForBoss = null;

        players = null;

        translationsDict = null;
        tileDict.Clear();
        spriteDict.Clear();
        textureDict.Clear();
        Global.discordSent.Clear();
    }
}
