using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Globalization;
using System.Xml.Linq;
using System.Linq;
using System;

[System.Serializable]
[CreateAssetMenu(fileName = "DownloadBundle", menuName = "Download Bundle")]
public class DownloadBundle : ScriptableObject
{
    public bool untouched = true;
    public string version = "";
    public string filesTsv = "https://spreadsheets.google.com/feeds/download/spreadsheets/Export?exportFormat=tsv&key=1mGDn5SY4lGd5ApF4VTtwsDwU_Htcx6RNl4DSm39q8uk&gid=1175270910";
    public DownloadBundleEntry[] entries = Type.EmptyTypes as object[] as DownloadBundleEntry[];
}

[System.Serializable]
public class DownloadBundleEntry
{
    public string key = "", value = "", format = "";
    public int prio = 0, version = 0, downloadedVersion = -1, downloadedSize = -1;
}