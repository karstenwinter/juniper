using UnityEditor;
using UnityEngine;
using System;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

class Autobuild
{
    static string[] scenes = {
        "Assets/Scene.unity"
     };

    [MenuItem("WebGL/Ping discord")]
    static void PingDiscord()
    {
        return;
        //TODO remove ping return!

        var buildId = File.ReadAllText("Assets/Resources/BuildIdentifier.txt");

        var message0 = "New Version available: " + buildId + " :lizard:\\nAndroid: https://bit.ly/geckoknightandroid" + "\\nWindows: https://bit.ly/geckoknightbuild";
        var hook1 = "https://discord.com/api/webhooks/847409404843917312/YIhGLYLauvJQnwzl82kURA2VQhniSjYMRXjYoVDif7ehHSGt-w1FYedHdBToZy4X9TqB";
        var curlLine = "-d \"{\\\"content\\\": \\\"" + message0 + "\\\"}\" -H \"Content-Type: application/json\" \"" + hook1 + "\"";

        UnityEngine.Debug.Log("curl " + curlLine);

        var curlExePath = "curl.exe";
        var commandProcess = new Process();
        commandProcess.StartInfo.UseShellExecute = false;
        commandProcess.StartInfo.FileName = curlExePath; // this is the path of curl where it is installed;    
        commandProcess.StartInfo.Arguments = curlLine; // your curl command    
        commandProcess.StartInfo.CreateNoWindow = true;
        commandProcess.StartInfo.RedirectStandardInput = true;
        commandProcess.StartInfo.RedirectStandardOutput = true;
        commandProcess.StartInfo.RedirectStandardError = true;
        commandProcess.Start();
    }

    [MenuItem("WebGL/Build all, push, ping and shutdown")]
    static void BuildAllAndPushShutdown()
    {
        Build();
        Push();
        PingDiscord();
        Shutdown();
    }

    [MenuItem("WebGL/Push")]
    static void Push()
    {
        Process.Start("git", "add .");
        Process.Start("git", "commit -m update");
        Process.Start("git", "pull");
        Process.Start("git", "push");
    }
    

    [MenuItem("WebGL/Build")]
    static void Build()
    {
        var buildPath = "../Build";
        var report = BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.WebGL, BuildOptions.None);
        UnityEngine.Debug.Log("reported " + report.summary.totalErrors + " errors");
    }

    [MenuItem("WebGL/Shutdown")]
    static void Shutdown()
    {
        var commandProcess = new Process();
        commandProcess.StartInfo.UseShellExecute = false;
        commandProcess.StartInfo.FileName = "shutdown.exe";
        commandProcess.StartInfo.Arguments = "-s -t 10";
        commandProcess.StartInfo.CreateNoWindow = true;
        commandProcess.StartInfo.RedirectStandardInput = true;
        commandProcess.StartInfo.RedirectStandardOutput = true;
        commandProcess.StartInfo.RedirectStandardError = true;
        commandProcess.Start();
    }

    [MenuItem("WebGL/Clean Win Build Folder")]
    static void CleanWinBuildFolder()
    {
        string buildPathFolder = "./Build";
        var di = new DirectoryInfo(buildPathFolder);

        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            dir.Delete(true);
        }
        UnityEngine.Debug.Log("CleanWinBuildFolder done");
    }

    // [MenuItem("WebGL/Run Test %j")]
    // static void RunTest()
    // {
    //     var behavior = UnityEngine.Object.FindObjectOfType<ContentDownloader>();
    //     // bool? success = null;
    //     var enumerator = TestLogic.RegressionTestEnumerator(value => 
    //     {
    //         Global.LogDebug("test: " + value);
    //     });
    //     behavior.StartCoroutine(enumerator);
    //     // while(enumerator.MoveNext())
    //     //     yield return enumerator.Current;
    //     // if(success == null) {
    //     //     Assert.Fail("success was not set from test");
    //     // }
    //     // Assert.IsTrue(success);
    // }
}