using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : CachedTransform
{
    [SerializeField] ClickeableObject[] poolObjects;

    public ObjectType objectsType;

    int index;
    ObjectSpawner objectSpawner;

    public void InitObjects(ObjectSpawner spawner)
    {
        objectSpawner = spawner;

        SpawneableObjectData objectData = Resources.Load<SpawneableObjectData>(objectsType.ToString());
        if (objectData != null)
        {
            int size = poolObjects.Length;
            for (int i = 0; i < size; i++)
            {
                poolObjects[i].Init(objectData, objectSpawner);
                poolObjects[i].OnDeath += ReturnObject;
            }
        }
        else
        {
            Debug.LogError($"Couldn't load {objectData} data");
        }
        //poolQueue = new Queue<Transform>();

        //for (int i = 0; i < numberOfObjectsToUse; i++)
        //    poolQueue.Enqueue(poolObjects[i]);
    }

    public ClickeableObject PeekObject() => poolObjects[index];

    public void SpawnObject(Vector3 startPos, Quaternion startRotation)
    {
        poolObjects[index].MyTransform.position = startPos;
        poolObjects[index].MyTransform.rotation = startRotation;
        poolObjects[index].StartBehaviour();

        index++;
        if (index >= poolObjects.Length) index = 0;
    }

    public void ReturnObject(ClickeableObject objectToReturn)
    {
        objectToReturn.MyTransform.localPosition = Vector3.zero;
        objectToReturn.MyTransform.rotation = Quaternion.identity;
        objectToReturn.MyTransform.localScale = Vector3.one;
        objectToReturn.ResetData();
        objectSpawner.DecreaseObjectNumber();
    }
}
