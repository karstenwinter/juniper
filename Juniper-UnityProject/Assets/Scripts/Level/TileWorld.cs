using System;
using UnityEngine;

public enum Edge { None = 0, Top = 1, Right = 2, Bottom = 4, Left = 8 }

[Serializable]
[CreateAssetMenu(fileName = "TileWorld", menuName = "Gecko Knight Tile World")]
public class TileWorld : ScriptableObject
{
    public string TmxFileName, TsxFileName;
}
