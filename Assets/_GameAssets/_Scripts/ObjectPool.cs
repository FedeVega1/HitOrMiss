using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : CachedTransform
{
    [SerializeField] ClickeableObject[] poolObjects;
    [SerializeField] string objectsName;

    int index;

    public void InitObjects()
    {
        SpawneableObjectData objectData = Resources.Load<SpawneableObjectData>(objectsName);
        if (objectData != null)
        {
            int size = poolObjects.Length;
            for (int i = 0; i < size; i++)
            {
                poolObjects[i].Init(objectData);
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

    public ClickeableObject SpawnObject(Vector3 startPos, Quaternion startRotation)
    {
        ClickeableObject poolObject = poolObjects[index];
        poolObject.MyTransform.position = startPos;
        poolObject.MyTransform.rotation = startRotation;
        poolObject.StartBehaviour();

        index++;
        if (index >= poolObjects.Length) index = 0;

        return poolObject;
    }

    public void ReturnObject(ClickeableObject objectToReturn)
    {
        objectToReturn.MyTransform.localPosition = Vector3.zero;
        objectToReturn.MyTransform.rotation = Quaternion.identity;
        objectToReturn.MyTransform.localScale = Vector3.one;
    }
}
