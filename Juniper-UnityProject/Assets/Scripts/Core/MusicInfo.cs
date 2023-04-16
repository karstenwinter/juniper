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
[CreateAssetMenu(fileName = "MusicInfo", menuName = "Music Info")]
public class MusicInfo : ScriptableObject {
public MusicInfoEntry[] entries = new MusicInfoEntry[0];
}

[System.Serializable]
public class MusicInfoEntry
{
    public string sectionCommaSep = "", musicTrack = "";
    public bool isTransition;
}