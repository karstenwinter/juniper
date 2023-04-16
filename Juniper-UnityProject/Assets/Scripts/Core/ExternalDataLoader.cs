using System;
using System.Linq;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TranslationData
{
    public Language language;
    public string key = "", en = "", de = "", es = "", ru = "", ja = "", cn = "";
}

public class CharmData
{
    public string charmType = "", name = "", condition = "", effect = "", staticText = "";
}

public enum RuneType
{ Condition, Triggered, Static }

public class RuneData
{
    public string name, translation, staticEffect, triggeredEffect, condition, rune, url;
    public RuneType runeType;
}

public class TableData
{
    public string location = "", objectName = "", name = "", url = "", data = "";
    public int hp, damage, value;
}

public class ObjectData
{
    public string prefab = "";
    public int index;
}

public abstract class LayeredBase
{
    public string tone = "", unityPrefabOverride = "";
    public int sortingOrder;
    public float layerZ, scale;
}

public abstract class AnimatedBase : LayeredBase
{
    private string _name = "";
    public string state = "", data = "", url = "";
    public int frames;

    public virtual string name { get { return _name; } set { _name = value; } }
}

public class CharacterData : AnimatedBase
{
}

public class AnimatedData : AnimatedBase
{
    public string prefab = "", variant = "", cleanedVariant = "";

    public override string name { get { return prefab + "/" + variant; } set { throw new Exception("not supported by AnimatedData"); } }
}

public class ObjectVariantData : LayeredBase
{
    public string prefab = "", variant = "", cleanedVariant = "", url = "";
}

public class AreaData
{
    public string name = "", tone = "", url = "";
}

public class PlayerTableData
{
    public string unityId = "", discordId = "", message = "", comment = "";
}

public static class ExternalDataLoader
{
    public static ObjectData[] ReadObjectDataFile()
    {
        var tuple = ReadTsvFile("objects");
        var headers = tuple.Item1;
        var lines = tuple.Item2;
        return lines.Select(parts =>
        {
            int index;
            int.TryParse(parts[0], out index);
            return new ObjectData
            {
                index = index,
                prefab = parts[1]
            };
        }).ToArray();
    }

    public static PlayerTableData[] ReadPlayerTableDataFile()
    {
        var tuple = ReadTsvFile("players");
        var headers = tuple.Item1;
        var lines = tuple.Item2;
        return lines.Select(parts =>
        {
            return new PlayerTableData
            {
                unityId = parts[0],
                discordId = parts[1],
                message = parts[2],
                comment = parts[3]
            };
        }).ToArray();
    }

    public static CharacterData[] ReadCharacterDataFile()
    {
        var tuple = ReadTsvFile("characters");
        var headers = tuple.Item1;
        var lines = tuple.Item2;
        return lines.Select(parts =>
        {
            int frames;
            int.TryParse(parts[2], out frames);
            return new CharacterData
            {
                name = parts[0],
                state = parts[1],
                frames = frames,
                data = parts[3],
                url = parts[4]
            };
        }).ToArray();
    }

    public static AnimatedData[] ReadAnimatedDataFile()
    {
        var tuple = ReadTsvFile("animated");
        var headers = tuple.Item1;
        var lines = tuple.Item2;
        var regex = new System.Text.RegularExpressions.Regex("[0-9]*");                                                  
        return lines.Select(parts =>
        {
            int frames;
            int.TryParse(parts[3], out frames);

            var layer = parts[5].ToLower();
            float layerZ;
            float.TryParse(parts[7], NumberStyles.Float, CultureInfo.InvariantCulture, out layerZ);
            int sortingOrder;
            int.TryParse(parts[8], out sortingOrder);

            return new AnimatedData
            {
                prefab = parts[0],
                variant = parts[1],
                cleanedVariant = regex.Replace(parts[1], "").ToLower(),
                state = parts[2],
                frames = frames,
                data = parts[4],
                tone = parts[6].ToLower(),
                layerZ = layerZ,
                sortingOrder = sortingOrder,
                url = parts[9]
            };
        }).ToArray();
    }


    public static AreaData[] ReadAreaDataFile()
    {
        var tuple = ReadTsvFile("areas");
        var headers = tuple.Item1;
        var lines = tuple.Item2;
        return lines.Select(parts =>
        {
            return new AreaData
            {
                name = parts[0],
                tone = parts[1],
                url = parts[2]
            };
        }).ToArray();
    }

    public static RuneData[] ReadRuneDataFile()
    {
        var tuple = ReadTsvFile("runes");
        var headers = tuple.Item1;
        var lines = tuple.Item2;
        return lines.Select(parts =>
        {
            var staticEffect = parts[2];
            var triggeredEffect = parts[3];
            var condition = parts[4];
            var runeType =
                condition != "" ? RuneType.Condition
                : triggeredEffect != "" ? RuneType.Triggered
                : RuneType.Static;
            return new RuneData
            {
                name = parts[0],
                translation = parts[1],
                rune = parts[5],
                url = parts[6],
                staticEffect = staticEffect,
                triggeredEffect = triggeredEffect,
                condition = condition,
                runeType = runeType
            };
        }).ToArray();
    }

    public static TableData[] ReadTableDataFile()
    {
        var tuple = ReadTsvFile("data");
        var headers = tuple.Item1;
        var lines = tuple.Item2;
        return lines.Select(parts =>
        {
            int hp, damage, value; // objectId, 
            int.TryParse(parts[5], out hp);
            int.TryParse(parts[6], out damage);
            int.TryParse(parts[7], out value);
            return new TableData
            {
                location = parts[0],
                objectName = parts[1],
                name = parts[2],
                url = parts[3],
                hp = hp,
                damage = damage,
                data = parts[8],
                value = value,
            };
        }).ToArray();
    }

    public static TranslationData[] ReadTranslationDataFile()
    {
        var tuple = ReadTsvFile("translations");
        var headers = tuple.Item1;
        var lines = tuple.Item2;
        return lines.Select(parts =>
        {
            return new TranslationData
            {
                key = parts[0],
                en = parts[2],
                de = parts[3],
                es = parts[4],
                ru = parts[5],
                ja = parts[6],
                cn = parts[7]
            };
        }).ToArray();
    }

    public static Tuple<string[], string[][]> ReadTsvFile(string path)
    {
        var content = ReadFileToString(SaveSystem.GetDataPath(path), "tsv");

        var lines = content.Replace("\r", "").Split('\n');

        var headers = (lines.FirstOrDefault() ?? "").Split('\t');
        var records = lines.Skip(1).Where(x => x.Trim().Length > 0).Select(x =>
        {
            var parts = x.Split('\t').Select(y => y.Trim()).ToArray();
            return parts;
        }).Where(x => x.Length > 0).ToArray();
        return Tuple.Create(headers, records);
    }

    public static string ReadFileToString(string fullPath, string ext, bool log = true)
    {
        var downloadedPath = SaveSystem.MakeDownloaded(fullPath, ext);
        if (File.Exists(downloadedPath))
        {
            if (log)
                Global.LogDebug("Found downloaded file: " + downloadedPath);
            return File.ReadAllText(downloadedPath);
        }
        else
        {
            var asset = Resources.Load(fullPath) as TextAsset;
            if (asset == null)
            {
#if !UNITY_EDITOR
                if (log)
                    Global.LogDebug("No downloaded file and no fallback for: " + fullPath + " (checked " + downloadedPath + ")");
#endif
                return "";
            }
            else
            {
#if !UNITY_EDITOR
                if (log)
                    Global.LogDebug("No downloaded file (but using fallback) for: " + fullPath + " (checked " + downloadedPath + ")");
#endif

                var content = asset.text;
                return content;
            }
        }
    }

    public static Sprite LoadSpriteForFilename(string resourcePath, string ext, bool log = true)
    {
        Sprite sp;
        if (Caches.spriteDict.ContainsKey(resourcePath))
        {
            sp = Caches.spriteDict[resourcePath];
            return sp;
        }
        else
        {
            var t = LoadTextureForFilename(resourcePath, ext, log);

            if (t == null)
            {
                sp = Resources.Load<Sprite>(resourcePath);
                if (sp != null)
                {
                    Caches.spriteDict[resourcePath] = sp;
                    return sp;
                }
            }

            sp = SpriteFromTexture2D(t);
            Caches.spriteDict[resourcePath] = sp;
            return sp;
        }
    }
    public static Texture2D TextureFromBytes(byte[] bytes)
    {
        var tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return tex;
    }

    public static Sprite SpriteFromBytes(byte[] bytes)
    {
        return SpriteFromTexture2D(TextureFromBytes(bytes));
    }

    public static Sprite LoadSpriteForCharacterOrAnimated(bool character, string name, int x, int y, int w, float unit = 120, float pixelsPerUnit = 120, Vector2? pos = null)
    {
        pos = pos ?? new Vector2(0.5f, 0.5f);

        var key = name + "@y" + y + "x" + x;
        Sprite res = null;
        if (Caches.spriteDict.TryGetValue(key, out res))
        {
            //return res;
        }

        try
        {
            var sheet = LoadTextureForCharacterOrAnimated(character, name);
            if (sheet != null)
            {
                var rect = new Rect(unit * x, -unit * (y + 1), unit, unit);
                Global.LogDebug("LoadTextureForCharacterOrAnimated returned a texture, using rect " + rect + " for " + name + ", pixelsPerUnit " + pixelsPerUnit);
                res = Sprite.Create(sheet, rect, pos.Value, pixelsPerUnit);
                Caches.spriteDict[key] = res;
                return res;
            }

            var num = y * w + x;
            var nameToLoad = "gen/" + name + "_" + num;
            var parts = name.Split('/');
            if (parts.Length > 1)
                nameToLoad = parts[0] + "/gen/" + parts[1] + "_" + num;


            var path = character ? SaveSystem.GetCharacterPath(nameToLoad) : SaveSystem.GetAnimatedPath(nameToLoad, "");
            var tile = Resources.Load<Tile>(path);
            Global.LogDebug("last step load sprite part using tile: " + path + " => " + tile);
            res = tile?.sprite;
            return res;
        }
        catch (Exception e)
        {
            Global.HandleError(e);
            return null;
        }

    }

    public static Texture2D LoadTextureForCharacterOrAnimated(bool character, string name)
    {
        return null;

        /*Texture2D tex;
        var path = character ? SaveSystem.GetCharacterPath(name) : SaveSystem.GetAnimatedPath("", name);
        if (Caches.textureDict.TryGetValue(path, out tex))
        {
            return tex;
        }

        tex = LoadTextureForFilename(path, "png");
        Global.LogDebug("tex bytes for " + path + "=" + tex);
        Caches.textureDict[path] = tex;
        return tex;*/
    }


    public static Texture2D LoadTextureForFilename(string resourcePath, string ext, bool log = true)
    {
        var filename = SaveSystem.MakeDownloaded(resourcePath, ext);
        Texture2D tex = null;
        if (Caches.textureDict.ContainsKey(filename))
        {
            tex = Caches.textureDict[filename];
            return tex;
        }
        else
        {
            if (Global.isDebug && File.Exists(filename))
            {
                var bytes = File.ReadAllBytes(filename);
                if (log)
                    Global.LogDebug("Found " + filename + " (size: " + bytes.Length + ")");

                tex = new Texture2D(2, 2); // will be resized
                tex.LoadImage(bytes);
                Caches.textureDict[filename] = tex;
                return tex;
            }
        }
        return null;
    }

    public static Sprite SpriteFromTexture2D(Texture2D texture, float units = 120f)
    {
        return texture == null ? null : Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), units);
    }

}
