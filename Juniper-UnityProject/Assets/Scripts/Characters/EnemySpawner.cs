using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float frequency = 4f;
    float timer;
    public string status = "";

    void Update()
    {
        timer += Time.deltaTime;
        status = timer + "/" + frequency;
        if(timer >= frequency) {
            timer = 0;
            var en = Instantiate(enemyPrefab);
            en.transform.parent = transform;
            en.transform.position = transform.position + new Vector3(UnityEngine.Random.Range(0, 2f), 0, 0);

            frequency = Math.Max(1f, frequency - 0.2f);
        }
    }
}
