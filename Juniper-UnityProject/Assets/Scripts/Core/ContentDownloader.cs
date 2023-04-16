using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Threading;

public class ContentDownloader : MonoBehaviour
{
    string latestBuildVersionOnline = "https://bit.ly/JunipersPathVersion";
    // string downloadUrlForAndroidBuild = "https://bit.ly/JunipersPathDemo";
    string downloadUrlForWindowsBuild = "https://bit.ly/JunipersPathDemo";

    public DownloadBundle downloadBundle;
    
    public void Awake()
    {
        Global.contentDownloader = this;
    }

    public static DownloadBundleEntry[] parseFilesTsv(string filesTsvContent)  {
            var formats = new Dictionary<string, string>();

            Global.LogDebug("filesTsvContent length: " + filesTsvContent.Length);

            var entries = new List<DownloadBundleEntry>();
         
            string[][] tsvEntries = Array.ConvertAll(filesTsvContent.Split('\n'), x => x.Split('\t'));
            Global.LogDebug("entries: " + tsvEntries.Length);
            foreach(var row in tsvEntries)
            {
                var key = row[0];
                var format = row[1];
                var value = row[3];

                if(format.ToLower() == "format")
                {
                    formats.Add(key, value);
                }
            }

            foreach(string[] row in tsvEntries)
            {
                var key = row[0];
                var format = row[1];
                var versionStr = row[2];
                var value = row[3];
                var prioStr = row[4];

                int version;
                int prio;
                int.TryParse(versionStr, out version);
                int.TryParse(prioStr, out prio);

                var f = "MISSING FORMAT FOR " + format + ", value: {0}";
                if(formats.ContainsKey(format))
                    f = formats[format];

                var downloadLink = String.Format(f, value).Trim();
                if(key.ToLower() != "key" && format.ToLower() != "format")
                {
                    entries.Add(new DownloadBundleEntry { key = key, value = downloadLink, format = format, prio = prio, version = version });
                }
            }

            return entries.ToArray();
    }

    public void RefreshDownloadsJson(Action then, Action<string> info = null)
    {
        if(info != null)
            info(Translations.For("SyncingServer"));
            
        var bundle = EnsureDownloadsJson();
        var len = bundle.entries.Length;
        
        var filesTsv = bundle.filesTsv;
        Download(url: filesTsv, whenDone: tsv =>
        {
            Global.LogDebug("filesTsv: " + filesTsv.Length);
            var entries = parseFilesTsv(tsv);
            bundle.version = Global.GetCurrentId();
            bundle.entries = entries.ToArray();
            if(info != null)
                info(Translations.For("SyncedServer") + " " + bundle.entries.Length);

            Caches.Clear();

            SaveSystem.SaveDownloadBundle(bundle);
            then?.Invoke();
        }, onErr: err =>
        {
            if(info != null)
            {
                info(Translations.For("Offline") + " " + err);
            }
        });
    }

    public void CheckForUpdatesAndDownload(Action<string> info = null, IModal modal = null)
    {
        Download(latestBuildVersionOnline, whenDone: newestId0 => {
            var newestId = newestId0.Trim();

            info?.Invoke(Translations.For("BinaryVersion") + " " + newestId);
            if(Global.buildID == newestId)
            {
                info?.Invoke(Translations.For("BinaryVersionUpToDate"));
            }
            else {
                var downloadUrl = downloadUrlForWindowsBuild;
                #if UNITY_ANDROID
                downloadUrl = downloadUrlForAndroidBuild;
                #endif

                modal?.OpenModal(
                    Translations.For("BinaryVersionAvailable", newestId, downloadUrl),
                    onYes: () => {
                        Global.soundManager.Play("select");
                        Application.OpenURL(downloadUrl);
                    },
                    onNo: () => {    
                        Global.soundManager.Play("select");
                    });
            }
        });

    }

    void WriteFile(string path, byte[] bytes)
    {
        try
        {
            File.Create(path).Close();
            File.WriteAllBytes(path, bytes);
        } catch(Exception e)
        {
            Global.HandleError(e);
        }
    }

    public void DownloadImages(Action<string> info = null, bool intoResources = false, Action whenDone = null)
    {
        if (!Global.isDebug)
            return;

        Action<string> onErr = err => 
        {
            if(info != null)
            {
                info(Translations.For("Offline") + " " + err);
            }
        };

        var areas = Array.FindAll(Caches.AreaData, x => x.url != "");

        var characters = Array.FindAll(Caches.CharacterData, item => item.url != "");

        var animated = Array.FindAll(Caches.AnimatedData, item => item.url != "");

        var tableObjectImages = Array.FindAll(Caches.TableData, x => x.url != "");
        var runeImages = Array.FindAll(Caches.RuneData, x => x.url != "");

        int len = areas.Length + tableObjectImages.Length + characters.Length + animated.Length + runeImages.Length;
        int deco = 0;
        len += deco;
        var infoString = "Server: " + deco + " Deco Objects, " + areas.Length + " Tilesets, " + tableObjectImages.Length
            + " Objects, " + characters.Length + " Characters";
        Global.LogDebug(infoString);
        int counter = 0;

        SaveSystem.EnsureDirectoryExists(SaveSystem.MakeDownloaded(SaveSystem.GetCharacterPath("")));
        foreach(var item in characters)
        {
            var path = SaveSystem.MakeDownloaded(SaveSystem.GetCharacterPath(item.name), "png");
            if(intoResources)
            {
                path = "Assets/Resources/" + SaveSystem.GetCharacterPath(item.name) + ".png";
            }
            Download(item.url, readBytes: true, whenDoneBytes: bytes => 
            {
                    Interlocked.Increment(ref counter);
                    info?.Invoke(Translations.For("SyncProgress") + ": " + counter + " / " + len + " (" + item.name + " character image)");
                    Global.LogDebug("Gonna write bytes " + path + " (" + bytes.Length + ")");
                    WriteFile(path, bytes);
            }, onErr: onErr);
        }

        SaveSystem.EnsureDirectoryExists(SaveSystem.MakeDownloaded(SaveSystem.GetAnimatedPath("", "")));
        foreach(var item in animated)
        {
            SaveSystem.EnsureDirectoryExists(SaveSystem.MakeDownloaded(SaveSystem.GetAnimatedPath(item.prefab, "")));
            
            var path = SaveSystem.MakeDownloaded(SaveSystem.GetAnimatedPath(item.prefab, item.variant), "png");
            if(intoResources)
            {
                SaveSystem.EnsureDirectoryExists("Assets/Resources/" + SaveSystem.GetAnimatedPath(item.prefab, ""));
                path = "Assets/Resources/" + SaveSystem.GetAnimatedPath(item.prefab, item.variant) + ".png";
            }
            Download(item.url, readBytes: true, whenDoneBytes: bytes => 
            {
                    Interlocked.Increment(ref counter);
                    info?.Invoke(Translations.For("SyncProgress") + ": " + counter + " / " + len + " (" + item.prefab + " - " + item.variant + " animated image)");
                    Global.LogDebug("Gonna write bytes " + path + " (" + bytes.Length + ")");
                    WriteFile(path, bytes);
            }, onErr: onErr);
        }

    }

    [ContextMenu("Re-Download and put into unity project-assets-resources")]
    public void DownloadIntoResourcesDL()
    {
        DownloadDataIntoResourcesDL(() => DownloadImages(intoResources: true));
    }

    public void DownloadIntoResourcesDL2(Action whenDone)
    {
        DownloadDataIntoResourcesDL(() => DownloadImages(intoResources: true, whenDone: whenDone));
    }

    public void DownloadDataIntoResourcesDL(Action whenDone)
    {
        RefreshDownloadsJson(then: () => {
            Debug.Log("RefreshDownloadsJson done");
            ReDownloadAll(intoResources: true, whenDone: () => {
                Debug.Log("ReDownloadAll done");
                whenDone.Invoke();
                });
            }
        );
    }

    DownloadBundle EnsureDownloadsJson()
    {
        
        if(Global.downloadBundle.untouched)
        {
            Global.downloadBundle = Clone(downloadBundle);
            Global.downloadBundle.untouched = false;
            SaveSystem.SaveDownloadBundle(Global.downloadBundle);
        }
        
        return Global.downloadBundle;
    }

    public T Clone<T>(T data) where T : new()
    {
        var res = new T();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data), res);
        return res;
    }

    public void ReDownloadAll(Action<string> info = null, bool intoResources = false, Action whenDone = null)
    {
        var bundle = EnsureDownloadsJson();
        var counter = 0;
        var bytesTotal = 0;
        var len = bundle.entries.Length;

        SaveSystem.EnsureDirectoryExists(SaveSystem.MakeDownloaded(SaveSystem.GetDataPath("")));

        foreach(var item in bundle.entries)
        {
            var downloadLink = item.value;
            var fileName = item.key + "." + item.format;
            var path = SaveSystem.MakeDownloaded(SaveSystem.GetDataPath(fileName));
            //if(!File.Exists(path)) //  || item.downloadedVersion != item.version
            Download(downloadLink, readBytes: true, whenDoneBytes: bytes => 
            {   
                Interlocked.Increment(ref counter);
                bytesTotal += bytes.Length;
                if(intoResources && (item.format == "tsv" || item.format == "tmx") && bytes.Length > 0)
                {
                    path = "Assets/Resources/" + SaveSystem.GetDataPath(fileName);
                }

                Global.LogDebug("Gonna write bytes " + path + " (" + bytes.Length + ")");
                try
                { 
                    File.Create(path).Close();
                    File.WriteAllBytes(path, bytes);
                    item.downloadedSize = bytes.Length;
                    markAsStored(item);
                    if(info != null)
                    {
                        info(Translations.For("SyncProgress") + " " + counter + " / " + len + " (" + item.key + ")");
                    }
                }
                catch(Exception e)
                {
                    Global.HandleError(e);
                }
                Global.LogDebug("Written " + path + " (" + bytes.Length + ")");

                if(counter == len)
                {
                    whenDone?.Invoke();
                    SetBuildNum.done = false;
                }
            });
        }
    }

    void markAsStored(DownloadBundleEntry downloaded)
    {
        var bundle = Global.downloadBundle;

        long len = 0;
        foreach(var item in bundle.entries)
        {
            if(item.format == "tmx" || item.format == "tsv")
                len += item.downloadedSize;

            if(item.key == downloaded.key)
            {
                item.downloadedVersion = item.version;
            }
        }
        bundle.version = len.ToString("X");

        SaveSystem.SaveDownloadBundle(bundle);
    }
    
    public void Download(string url, bool readBytes = false, Action<string> whenDone = null, Action<byte[]> whenDoneBytes = null, Action<string> onErr = null)
    {
        StartCoroutine(DownloadC(url, readBytes, whenDone, whenDoneBytes, onErr));
    }

    public IEnumerator DownloadC(string url, bool readBytes = false, Action<string> whenDone = null, Action<byte[]> whenDoneBytes = null, Action<string> onErr = null)
    {
        using(UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError)
            {
                if(onErr != null)
                    onErr(www.error + " for file " + url);
                else
                    Global.LogDebug(www.error + " for file " + url);
            }
            else
            {
                // Show results as text
                if(readBytes)
                {
                    byte[] results = www.downloadHandler.data;
                    whenDoneBytes(results);
                }
                else
                {
                    string s = www.downloadHandler.text;
                    whenDone(s);
                }
            }
        }
    }
}