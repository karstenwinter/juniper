using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    public static string SaveSettings(GameSettings data)
    {
        var name = "settings.json";
        var path = MakeDownloaded(GetUserDataPath(name));
        EnsureDirectoryExists(MakeDownloaded(GetUserDataPath("")));
        return SaveData(data, path);
    }

    public static GameSettings LoadSettings(bool logErrorOnFail = false)
    {
        var name = "settings.json";
        var path = MakeDownloaded(GetUserDataPath(name));
        if (File.Exists(path))
        {
           Debug.Log("Settings file found in " + path); 
            return LoadData<GameSettings>(path);
        }
        else
        {
            if (logErrorOnFail)
            {
                Debug.LogError("Settings file not found in " + path);
            }
            return null;
        }
    }

    public static void EnsureDirectoryExists(string dirPath)
    {
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
    }

    public static string MakeDownloaded(string path, string ext = "")
    {
        // #if UNITY_EDITOR
        // return path;
        // #else
        return Application.persistentDataPath + "/" + path + (ext == "" ? "" : "." + ext);
        // #endif
    }

    public static string GetUserDataPath(string name)
    {
        return "UserData/" + name;
    }

    public static string GetDataPath(string name)
    {
        return "Data/" + name;
    }

    public static string GetMapPath(string name)
    {
        return "Map/" + name;
    }
    
    //public static string GetTilemapPath(string name)
    //{
    //    return "Map/gen/" + name;
    //}

    public static string GetCharacterPath(string name)
    {
        return "Characters/" + name;
    }

    public static string GetAnimatedPath(string name, string variant)
    {
        return "Animated/" + name + (variant == "" ? "" : "/" + variant);
    }
    public static string JsonPrettyPrint<T>(T data)
    {
        return JsonUtility.ToJson(data, true);
    }

    private static bool accessingFiles = false;

    public static string SaveData<T>(T data, string path) where T : new()
    {
        if (accessingFiles)
            return "CONCURRENT ACCESS";
        try
        {
            accessingFiles = true;
            var dataToWrite = data;
            var str = JsonPrettyPrint(dataToWrite);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                using (var w = new StreamWriter(stream))
                {
                    w.Write(str);
                    w.Flush();
                }
            }
            return str;
        }
        catch (Exception e)
        {
            Global.HandleError(e);
            throw;
        }
        finally
        {
            accessingFiles = false;
        }
    }

    public static T LoadData<T>(string path) where T : new()
    {
        if (accessingFiles)
            return default(T);
        try
        {
            accessingFiles = true;

            using (var stream = new FileStream(path, FileMode.Open))
            {
                using (var rd = new StreamReader(stream, true))
                {
                    string str = rd.ReadToEnd();
                    var data = new T();
                    JsonUtility.FromJsonOverwrite(str, data);
                    return data;
                }
            }
        }
        catch (Exception e)
        {
            Global.HandleError(e);
            throw;
        }
        finally
        {
            accessingFiles = false;
        }
    }

    public static string Save(GameProfile index, PlayerSaveState data)
    {
        var name = "profile" + index + ".json";
        var path = MakeDownloaded(GetUserDataPath(name));
        return SaveData(data, path);
    }

    public static DownloadBundle LoadDownloadBundle(bool logErrorOnFail = false)
    {
        var name = "downloads.json";
        var path = SaveSystem.MakeDownloaded(SaveSystem.GetUserDataPath(name));
        if (File.Exists(path))
        {
            Global.LogDebug("downloads file found in " + path);
            return SaveSystem.LoadData<DownloadBundle>(path);
        }
        else
        {
            if (logErrorOnFail)
            {
                Global.LogDebug("downloads file not found in " + path);
            }
            return null;
        }
    }

    public static string SaveDownloadBundle(DownloadBundle data)
    {
        var name = "downloads.json";

        EnsureDirectoryExists(MakeDownloaded(GetUserDataPath("")));
        var path = MakeDownloaded(GetUserDataPath(name));
        return SaveData(data, path);
    }
    
    public static T Clone<T>(T data) where T : new()
    {
        var res = new T();
        var str = JsonUtility.ToJson(data, true);
        JsonUtility.FromJsonOverwrite(str, res);
        return res;
    }

    public static PlayerSaveState LoadPlayerProfile(GameProfile index, bool logErrorOnFail = false)
    {
        var name = "profile" + index + ".json";
        var path = MakeDownloaded(GetUserDataPath(name));
        if (File.Exists(path))
        {
            return LoadData<PlayerSaveState>(path);
        }
        else
        {
            if (logErrorOnFail)
            {
                Debug.LogError("Save file not found in " + path);
            }
            return null;
        }
    }

    public static string Delete(GameProfile index, bool logErrorOnFail = false)
    {
        var name = "profile" + index + ".json";
        var path = MakeDownloaded(GetUserDataPath(name));
        if (File.Exists(path))
        {
            File.Delete(path);
            return "deleted " + path;
        }
        else
        {
            if (logErrorOnFail)
            {
                Debug.LogError("file not found in " + path);
            }
            return null;
        }
    }

    public static PlayerSaveState Load(GameProfile index, bool withMapAndRefresh = true, bool incDeaths = false)
    {
        var data = LoadPlayerProfile(index);
        if (data != null)
        {
            Global.allPlayers.ForEach(p => { p.LoadFromState(data, incDeaths); });
            
            if (withMapAndRefresh)
            {
                if (data.map != null)
                {
                    try
                    {
                        //Global.minimapUI?.refreshFromNewArray(MapLogic.stringsToEdges(data.map));
                    }
                    catch (Exception e) { Global.HandleError(e); }
                }
            }
        }
        return data;
    }
}
