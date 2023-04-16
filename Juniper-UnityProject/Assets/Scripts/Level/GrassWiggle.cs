using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassWiggle : MonoBehaviour
{
    public Material grassMat;
    private Vector2 tile;
    private Vector2 currentWiggle;

    public Vector2 wiggleMin = new Vector2(0.4f, 0.2f);
    public Vector2 wiggle = new Vector2(0.4f, 0.2f);
    public Vector2 speed = new Vector2(0.5f, 0.1f);

    void Start()
    {
        tile = grassMat.GetTextureScale("_MainTex");
    }
    void OnDisable()
    {
        grassMat.SetTextureScale("_MainTex", tile);
    }

    void Update()
    {
        var facX = Mathf.PingPong(Time.unscaledTime * speed.x, wiggle.x * 2) - wiggle.x + wiggleMin.x;
        var facY = Mathf.PingPong(Time.unscaledTime * speed.y, wiggle.y * 2) - wiggle.y + wiggleMin.y;
        currentWiggle = tile + new Vector2(facX, facY);
        grassMat.SetTextureScale("_MainTex", currentWiggle);
        
    }
}
