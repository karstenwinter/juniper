using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.Networking;
using System;
using System.Threading;
using System.Text;

[Serializable]
public class PrefabMapItem
{
    public string symbol, emoji, name;
    public GameObject Instantiate(Vector3 position, Transform parent)
    {
        var key = "Prefabs/" + name;
        var loaded = Resources.Load<GameObject>(key);
        Debug.LogWarning("no asset found for name in Resources: " + key);
        if(loaded != null)
            GameObject.Instantiate(loaded, position, Quaternion.identity, parent);
        return loaded;
    }
}

public class World : MonoBehaviour
{
    static Encoding enc = new UTF32Encoding(false, true, true);

    public GameObject sprite;
    public Transform player;
    public Transform prefabInstanceParent;
    public Tilemap tilemap;
    public TileBase tileBlock;
    public TileBase[] tiles = new TileBase[0];
    public Tilemap tilemapVisual, tilemapVisual2;
    public Image fader;
    //public PrefabMapItem[] prefabMap = new PrefabMapItem[0];
    public Dictionary<string, PrefabMapItem> prefabMap;

    public float scale = 1;
    public float fadeSpeed = 0.1f;

    public int z = 0;
    public int minY = 180, maxY = 330;
    bool playerFoundFadeIn;

    IEnumerable ParseEnemies(string csv)
    {
        var rows = csv.Replace("\n_", "").Replace("_\n", "").Split('\n');
        var started = false;
        var y = -1;

        // var list = new List<PrefabMapItem>();
        prefabMap = new Dictionary<string, PrefabMapItem>();

        Debug.Log("csv " + csv.Length);
        foreach (var row in rows)
        {
            y++;
            var x = -1;
            if (started)
            {
                var colSymbol = rows[y].Split(',');
                var colName = rows[y + 1].Split(',');
                foreach (var symbolRaw in colSymbol)
                {
                    x++;
                    var nameRaw = x < colName.Length ? colName[x] : "";
                    //Debug.Log("Symbol " + name + ": " + symbol);
                    // list.Add();
                    var name = nameRaw.Replace("\"", "");
                    var symbol = symbolRaw.Replace("\"", "");
                    if (symbol != "")
                    {
                        var symbolCode = codeForChar(symbol);
                        var item = new PrefabMapItem { symbol = symbolCode, emoji = symbol, name = name };
                        prefabMap[symbol] = item;
                    }
                }
                var list = prefabMap.Values;
                Debug.Log("Symbols (" + list.Count + "): " + String.Join(", ", list.Select(s => s.symbol + "->" + s.name)));
                yield break;
            }
            if (row.Contains("Symbols:"))
                started = true;
        }
    }

    IEnumerable PopulateWorld(string csv)
    {
        yield return new WaitForEndOfFrame();

        var start = DateTime.Now;
        // int playerFound = -1;

        var textInfo = GameObject.Find("InfoText")?.GetComponent<Text>();

        //Debug.Log(csv);
        var rows = csv.Trim().Replace("\n_", "").Replace("_\n", "").Split('\n');
        var started = false;
        var y = 0;
        var x = 0;
        //Debug.Log("csv " + csv.Length);
        foreach (var item in rows)
        {
            y++;
            if (textInfo != null)
                textInfo.text = "Loaded CSV, " + (y - minY) + "/" + (maxY - minY);

            if (started)
            {
                var cols = item.Split(',');
                x = 0;

                // Debug.Log("r " + y + " has " + cols.Length + " cols, TXT: " + item);
                foreach (var col in cols)
                {
                    x++;
                    var strCol = col.Replace("\"", "").Trim();
                    var c = strCol == "" ? "" : strCol; // .Substring(0, 1);
                    var pos = new Vector3(x, -y + 1, z) * scale;
                    var r = new System.Random(x * 7 + y * 13 + z);

                    if (y < minY || y > maxY)
                        continue;

                    if (c == "⬛") //"⬛") "\u2B1B"
                    {
                        Vector3Int position = new Vector3Int(x, -y, z);
                        tilemap.SetTile(position, tileBlock);
                        var tile = tiles[r.Next(tiles.Length)];
                        tilemapVisual.SetTile(position, tile);
                        tile = tiles[r.Next(tiles.Length)];
                        tilemapVisual2.SetTile(position, tile);
                        // var newObj = Instantiate(sprite, new Vector3(x, -y, z) * scale, Quaternion.identity, transform);
                        //   newObj.GetComponent<SpriteRenderer>();
                    }
                    else if (c == "🦎") //"🦎") { "\u1F98E"
                    {
                        var globalPlayerPos = GameObject.Find("GlobalPlayerStart");
                        if (globalPlayerPos != null)
                            globalPlayerPos.transform.position = pos;

                        playerFoundFadeIn = true;
                        player.gameObject.SetActive(true);
                    }
                    else if (c != "")
                    {
                        var code = codeForChar(c);
                        PrefabMapItem value;
                        if (prefabMap.TryGetValue(c, out value))
                        {
                            Debug.Log("found symbol in map: " + c + "\t" + code + " " + value.name);
                            var prefabPos = pos + new Vector3(0.5f, -0.5f, 0); // so that it is centered in the tilemap cell center
                            value.Instantiate(prefabPos, prefabInstanceParent);
                        }
                        else
                        {
                            Debug.LogWarning("not found symbol in map: " + c + "\t" + code);
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
            }

            if (item.Contains("Map:"))
            {
                Debug.Log("start");
                started = true;
            }
        }

        Debug.Log("FoundCsv, took " + (DateTime.Now - start));
        if (textInfo != null)
            textInfo.text = "";
    }
    string codeForChar(string c)
    {
        return "U+" + BitConverter.ToInt32(enc.GetBytes(c), 0).ToString("X");
    }


    void Start()
    {
        Reset();
    }

    [ContextMenu("Reset")]
    public void Reset()
    {
        player.gameObject.SetActive(false);

        var colorFader = fader.color;
        colorFader.a = 1;
        fader.color = colorFader;
        var start = DateTime.Now;

        var url = ""; //"https://docs.google.com/spreadsheets/d/1wWr9c588Fg5bKED99buWJaaC6_Gc4b555nuNT3tC8QM/gviz/tq?tqx=out:csv";
        Download(url, whenDone: csv =>
        {
            Debug.Log("FoundCsv, took " + (DateTime.Now - start));
            StartCoroutine(ParseAndPopulateWorld(csv));
        }, onErr: x =>
        {
            var textInfo = GameObject.Find("InfoText")?.GetComponent<Text>();
            var oldText = textInfo?.text;

            if (textInfo != null)
                textInfo.text = "Error on Download: " + x + " - using fallback";

            var csvFallback = Resources.Load<TextAsset>("Data/mapSnapshot2023")?.text;
            StartCoroutine(ParseAndPopulateWorld(csvFallback));
        });

    }

    IEnumerator ParseAndPopulateWorld(string csv)
    {
        foreach (var item in ParseEnemies(csv))
            yield return item;

        foreach (var item in PopulateWorld(csv))
            yield return item;
    }

    void Update()
    {
        if (playerFoundFadeIn && fader.color.a > 0)
        {
            var color = fader.color;
            color.a -= fadeSpeed;
            fader.color = color;
        }
    }

    public void Download(string url, bool readBytes = false, Action<string> whenDone = null, Action<byte[]> whenDoneBytes = null, Action<string> onErr = null)
    {
        StartCoroutine(DownloadC(url, readBytes, whenDone, whenDoneBytes, onErr));
    }

    public IEnumerator DownloadC(string url, bool readBytes = false, Action<string> whenDone = null, Action<byte[]> whenDoneBytes = null, Action<string> onErr = null)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            // while(x.MoveNext()) { x.Current.GetHashCode();

            if (www.isNetworkError || www.isHttpError)
            {
                if (onErr != null)
                    onErr(www.error + " for file " + url);
                else
                    Debug.Log(www.error + " for file " + url);
            }
            else
            {
                // Show results as text
                if (readBytes)
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
