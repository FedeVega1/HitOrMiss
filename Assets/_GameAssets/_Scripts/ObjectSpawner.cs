using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : CachedTransform
{
    [SerializeField] ObjectPool[] pools;

    Camera _MainCamera;
    Camera MainCamera
    {
        get
        {
            if (_MainCamera == null) _MainCamera = Camera.main;
            return _MainCamera;
        }
    }

    DifficultyData currentDifficulty;
    bool gameStarted;
    float timeForNextSpawn;
    int numberOfActiveObjects;

    public void OnGameStart(DifficultyData diffData)
    {
        int size = pools.Length;
        for (int i = 0; i < size; i++) pools[i].InitObjects(this);

        gameStarted = true;
        currentDifficulty = diffData;
    }

    void Update()
    {
        if (!gameStarted || numberOfActiveObjects >= currentDifficulty.maxObjectSpawnQuantity) return;

        if (Time.time >= timeForNextSpawn)
        {
            float random = Random.Range(0, 1f);

            int size = pools.Length;
            int randomPool = -1;
            float lowestProbability = 9999;

            for (int i = 0; i < size; i++)
            {
                float chance = currentDifficulty.spawnChancePerObject[pools[i].objectsType];
                if (chance >= random && chance < lowestProbability)
                {
                    randomPool = i;
                    lowestProbability = chance;
                }
            }

            if (randomPool == -1) return;

            ClickeableObject objectToSpawn = pools[randomPool].PeekObject();

            float xPos;
            Vector3 offScreenSide;
            Vector3 spawnPos;
            Quaternion spawnRot;

            switch (objectToSpawn.ObjBehaviour)
            {
                case ObjectBehaviour.Travel:
                    offScreenSide = MainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, (MainCamera.fieldOfView / 2) - 10));
                    xPos = Random.Range(0f, 1f) < .5f ? offScreenSide.x : -offScreenSide.x;

                    spawnPos = MyTransform.position + new Vector3(xPos, Random.Range(-offScreenSide.y, offScreenSide.y), 0);
                    spawnRot = Quaternion.LookRotation(MyTransform.position - spawnPos, Vector3.up);
                    //Debug.DrawLine(spawnPos, MyTransform.position, Color.yellow, 5);
                    //Debug.DrawRay(spawnPos, Vector3.Normalize(spawnPos - MyTransform.position), Color.cyan, 5);
                    break;

                case ObjectBehaviour.Jump:
                    offScreenSide = MainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, (MainCamera.fieldOfView / 2) - 10));
                    offScreenSide.x *= .5f;

                    xPos = Random.Range(0f, 1f) < .5f ? offScreenSide.x : -offScreenSide.x;

                    spawnPos = MyTransform.position + new Vector3(xPos, 0, 0);
                    spawnRot = Quaternion.identity;
                    break;

                default:
                    spawnPos = MyTransform.position + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
                    spawnRot = Quaternion.identity;
                    break;
            }

            pools[randomPool].SpawnObject(spawnPos, spawnRot);
            IncreaseSpawnTime();
            numberOfActiveObjects++;
        }
    }

    public void IncreaseSpawnTime() => timeForNextSpawn = Time.time + Random.Range(currentDifficulty.minTimeToSpawn, currentDifficulty.maxTimeToSpawn);

    public void DecreaseObjectNumber()
    {
        if (numberOfActiveObjects < currentDifficulty.minObjectSpawnQuantity) timeForNextSpawn = 0;
        if (numberOfActiveObjects >= currentDifficulty.maxObjectSpawnQuantity) IncreaseSpawnTime();

        numberOfActiveObjects--;
    }
}
