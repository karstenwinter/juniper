using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public static class Global
{
    public static string buildID = "2023-04-XX";
    public static int pickups = 1000;
    public static string playerId { get { return AnalyticsSessionInfo.userId; } }
    public static string discordInvite = "https://discord.gg/PHXRWVf";

    public static string buildNum { get { return "Build " + buildID; } }
    public static bool isPaused = false;

    public static HUD hud;
    public static MinimapUI minimapUI;
    public static MainMenu mainMenu;
    public static SoundManager soundManager;
    public static ContentDownloader contentDownloader;
    public static PlayerController playerController;
    public static readonly List<PlayerController> allPlayers = new List<PlayerController>();

    public static GameSettings settings = new GameSettings();
    public static PlayerSaveState profile = new PlayerSaveState();
    public static DownloadBundle downloadBundle = new DownloadBundle();
    public static string contentId { get { return downloadBundle.version; } }
    public static bool isDebug { get { return settings.debug == OffOn.On; } }

    public static string replayFile = null;
    public static string logPath;

    public static bool inTransition = false;
    public static bool playerNeedsToLoadData = false;

    public static string GetPlayerIdOrName()
    {
        PlayerTableData res;
        Caches.Players.TryGetValue(playerId, out res);
        return res?.discordId ?? playerId;
    }

    public static void FadeOutAndDo(Action action)
    {
        if (hud != null && hud.gameObject.activeInHierarchy)
            hud.FadeOut(action);
        else
            action.Invoke();
    }

    public static void FadeInAndDo(Action action)
    {
        if (hud != null && hud.gameObject.activeInHierarchy)
            hud.FadeIn(action);
        else
            action.Invoke();
    }

    public static void SaveSettings()
    {
        var written = SaveSystem.SaveSettings(settings);
        LogDebug("Saved: " + written);
    }

    static string getForHour(int x)
    {
        return "abcdefghijklmnopqrstuvwxyz"[x].ToString();
    }

    public static string GetCurrentId()
    {
        var now = DateTime.Now;
        var newId = now.ToString("yyyy-MM-dd") + getForHour(now.Hour);
        return newId;
    }

    static Global()
    {
        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, args) => Global.HandleError(args.ExceptionObject));

        logPath = SaveSystem.MakeDownloaded("log", "txt");
        var fresh = !File.Exists(logPath);

        if (fresh)
        {
            File.Create(logPath).Close();
        }

        try
        {
            buildID = ExternalDataLoader.ReadFileToString("BuildIdentifier", "txt", false);
        }
        catch (Exception e)
        {
            HandleError(e);
        }

        LogImportant("Starting Juniper's Path Open Source with " + buildID + (fresh ? " for the first time" : ""), "Info");

#if UNITY_EDITOR
        // when entering play mode update file :)
        var newId = GetCurrentId();
        var different = buildID != newId;
        Global.LogDebug("refresh: old=" + buildID + ", new=" + newId + ", different: " + different);
        if (different)
        {
            try
            {
                System.IO.File.WriteAllText("Assets/Resources/BuildIdentifier.txt", newId);
                buildID = newId;
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }
#endif

        try
        {
            settings = SaveSystem.LoadSettings(false);
            if (settings == null)
            {
                settings = new GameSettings();
                // TODO read locale and set language...
                SaveSettings();
            }
        }
        catch (Exception e)
        {
            HandleError(e);
        }

        try
        {
            Global.profile = SaveSystem.LoadPlayerProfile(Global.settings.profile, false);

            if (Global.profile == null)
            {
                Global.profile = new PlayerSaveState();
            }
        }
        catch (Exception e)
        {
            HandleError(e);
        }


        try
        {
            var args = Environment.GetCommandLineArgs();
            replayFile = args != null && args.Length > 1 ? args[1] : null;
            var isReplay = replayFile?.ToLower()?.Contains("session") == true;
            UnityEngine.Debug.Log("first cmd line arg: " + replayFile + " => replay: " + isReplay);
            if (!isReplay)
            {
                replayFile = null;
            }
        }
        catch (Exception e)
        {
            HandleError(e);
        }
    }

    public static void HandleError(object e)
    {
        LogError(e);
    }

    public static void LogDebug(object x)
    {
        UnityEngine.Debug.Log(x);

        if (Global.isDebug)
        {
            LogToFile(x, "Verbose");
        }

        // #if !UNITY_EDITOR
        if (hud != null && hud.debug != null)
        {
            hud.debug.text = x == null ? "null" : x.ToString();
        }
        // #endif
    }

    public static void LogError(object x)
    {
        UnityEngine.Debug.LogError(x);
        if (hud != null && hud.debug != null)
        {
            hud.debug.text = x == null ? "null" : x.ToString();
        }
        LogImportant(x, "Error");
    }

    public static void LogGameplay(object x)
    {
        LogToFile(x, "Gameplay");
        LogToDiscord(x, "Gameplay");
    }

    public static void LogImportant(object x, string type)
    {
        LogToFile(x, type);
        LogToDiscord(x, type);
    }

    public static HashSet<String> discordSent = new HashSet<string>();

    public static void LogToDiscord(object x, string type)
    {
        var key = type + "" + x;
        if (discordSent.Contains(key))
            return;
        discordSent.Add(key);

        //System.Threading.ThreadPool.QueueUserWorkItem(workItemParam =>
        LogToDiscordC(x, type);
        //);
    }

    public static string discordWebhook = "";
    public static string speedrunWebhook = "";

    static string WinUserName = "";

    public static void LogToDiscordC(object x, string type)
    {
        if (WinUserName == "")
        {
            try
            {
                WinUserName = Environment.UserName;
            }
            catch (Exception)
            {
            }
            WinUserName = (WinUserName ?? "");
        }


        var isDev = false;
#if UNITY_EDITOR
        // isDev = true;
#endif

        try
        {
            var info = isDev ? " (Developer)" : "";

            if (!isDev && discordWebhook != "")
            {
                var message0 = type + " for version " + buildID + "/" + contentId + " for player " + Global.GetPlayerIdOrName() + info + "(" + WinUserName + "): " + x;

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(discordWebhook);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"content\": \"" + message0 + "\"}";
                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Debug.Log("to discord: " + result);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("to discord err: " + e);
        }
    }

    public static void LogToFile(object x, string type)
    {
        if (type != "Verbose")
            Debug.Log("[LOGGED] " + type + ": " + x);

        try
        {
            var date = DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture);
            File.AppendAllText(logPath, date + "\t" + type + "\t" + x + Environment.NewLine);
        }
        catch { }
    }

    public static void RefreshSound(AudioVolume x)
    {
        AudioListener.volume = x.ToFloat();
    }

    public static void TransitionToScene(string sceneName, Vector3? targetPos = null,
        bool saveBeforeTransition = true,
        bool loadAfterTransition = true,
        bool fadeOutBefore = true,
        Action whenDone = null)
    {
        if (Global.inTransition)
        {
            LogDebug("already in transition (requested was " + sceneName + " with targetPos " + targetPos + ")");
            return;
        }

        Global.inTransition = true;
        Global.playerNeedsToLoadData = loadAfterTransition;
        LogDebug("TransitionToScene " + sceneName + (targetPos == null ? "" : " with target pos " + targetPos) + ", load after transition: " + loadAfterTransition);

        if (saveBeforeTransition && Global.playerController != null)
        {
            Global.playerController.state.sceneName = sceneName;
            SaveGame(targetPos);
        }

        Action action = () =>
        {
            try
            {
                var loading = SceneManager.LoadSceneAsync(sceneName);
                if (loading == null)
                {
                    HandleError("scene not found: " + sceneName);
                    whenDone?.Invoke();
                }
                else
                {
                    loading.completed += (x) =>
                    {
                        Global.inTransition = false;
                        whenDone?.Invoke();
                    };
                }
            }
            catch (Exception e)
            {
                Global.HandleError(e);
            }
        };

        if (fadeOutBefore)
        {
            Global.FadeOutAndDo(action);
        }
        else
        {
            action.Invoke();
        }
    }

    public static void SaveGame(Vector3? targetPos = null, bool justProfile = false, bool useCheckpoint = true)
    {
        var st = Global.profile;
        if (!justProfile && Global.playerController != null)
        {
            st = Global.playerController.GetStateWithMap(useCheckpoint: useCheckpoint);
            if (targetPos != null)
            {
                st.position = targetPos.Value;
            }
            Global.hud.SaveAnimation();
        }
        SaveSystem.Save(Global.settings.profile, st);
    }
}
