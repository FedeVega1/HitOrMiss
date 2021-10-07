using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : CachedTransform
{
    [SerializeField] ObjectPool[] pools;

    bool gameStarted;
    float timeForNextSpawn;

    void Start()
    {
        LevelManager.INS.OnGameStart += OnGameStart;
    }

    void OnGameStart()
    {
        int size = pools.Length;
        for (int i = 0; i < size; i++) pools[i].InitObjects();

        gameStarted = true;
    }

    void Update()
    {
        if (!gameStarted) return;

        if (Time.time >= timeForNextSpawn)
        {
            int randomPool = Random.Range(0, pools.Length);
            pools[randomPool].SpawnObject(MyTransform.position + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0), Quaternion.identity);

            timeForNextSpawn = Time.time + Random.Range(1f, 5f);
        }
    }
}
