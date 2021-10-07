using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyData", menuName = "DataObjects/DifficultyData")]
public class DifficultyData : ScriptableObject
{
    public DifficultyLevel difficultyLevel;
    public float[] spawnChancePerObject;

    public float minTimeToSpawn, maxTimeToSpawn;
    public float minObjectSpawnQuantity, maxObjectSpawnQuantity;
}
