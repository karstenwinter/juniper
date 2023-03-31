using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.Networking;
using System.IO;
using System.Net;
using System.Globalization;
using System.Xml.Linq;
using System.Linq;
using System;
using System.Threading;


public class World : MonoBehaviour
{
    public GameObject sprite;
    public Transform player;
    public Tilemap tilemap;
    public TileBase tile;
    public Tilemap tilemapVisual;
    public TileBase tileVisual;
    public Image fader;

    public float scale = 1;
    public int z = 0;

    void Start()
    {
        player.gameObject.SetActive(false);

        var colorFader = fader.color;
        colorFader.a = 1;
        fader.color = colorFader;
        int playerFound = -1;
        var url = "https://docs.google.com/spreadsheets/d/1wWr9c588Fg5bKED99buWJaaC6_Gc4b555nuNT3tC8QM/gviz/tq?tqx=out:csv";
        Download(url, whenDone: csv =>
        {
            Debug.Log(csv);
            var rows = csv.Split('\n');
            var start = false;
            var y = 0;
            var x = 0;
            Debug.Log("csv " + csv.Length);
            foreach (var item in rows)
            {
                y++;
                if (start)
                {
                    var cols = item.Split(',');
                    x = 0;

                    Debug.Log("r " + y + " has " + cols.Length + " cols, TXT: " + item);
                    foreach (var col in cols)
                    {
                        x++;
                        var c = col.Replace("\"", "");
                        if (c == "⬛") //"⬛") "\u2B1B"
                        {
                            tilemap.SetTile(new Vector3Int(x, -y, z), tile);
                            tilemapVisual.SetTile(new Vector3Int(x, -y, z), tileVisual);
                            // var newObj = Instantiate(sprite, new Vector3(x, -y, z) * scale, Quaternion.identity, transform);
                            //   newObj.GetComponent<SpriteRenderer>();
                        }
                        else if (c == "🦎") //"🦎") { "\u1F98E"
                        {
                            player.position = new Vector3(x, -y + 1, z) * scale;
                            playerFound = y;
                            player.gameObject.SetActive(true);
                        }
                    }
                }
                if (item.Contains("Map:"))
                {
                    Debug.Log("start");
                    start = true;
                }
                if (playerFound >= 0 && y > playerFound + 10 && fader.color.a != 0)
                {
                    var color = fader.color;
                    color.a = 0;
                    fader.color = color;
                }
            }
        });
    }

    void Update()
    {

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
