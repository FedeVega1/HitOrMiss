using UnityEngine;

[System.Serializable]
public class ObjectSpawnChance
{
    [System.Serializable]
    public class SpawnChance
    {
        public ObjectType objectType;

        [Range(0, 1)]
        public float chance;
    }

    public SpawnChance[] spawnChance;

    public float this[ObjectType type]
    {
        get 
        {
            int size = spawnChance.Length;
            for (int i = 0; i < size; i++)
            {
                if (spawnChance[i].objectType == type)
                    return spawnChance[i].chance;
            }

            return -1;
        }
    }
}

[CreateAssetMenu(fileName = "DifficultyData", menuName = "DataObjects/DifficultyData")]
public class DifficultyData : ScriptableObject
{
    public DifficultyLevel difficultyLevel;
    public ObjectSpawnChance spawnChancePerObject;

    public float maxRoundTime = 2;

    public float minTimeToSpawn, maxTimeToSpawn;
    public int minObjectSpawnQuantity, maxObjectSpawnQuantity;
}
