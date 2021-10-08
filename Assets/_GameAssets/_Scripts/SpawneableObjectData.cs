using UnityEngine;

[CreateAssetMenu(fileName = "ObjectData", menuName = "DataObjects/SpawneableObjectData")]
public class SpawneableObjectData : ScriptableObject
{
    public ObjectType objectType;
    public ObjectBehaviour objectBehaviour;
    public DestroyBehaviour destroyBehaviour;

    public float travelSpeed;
    public float timeToFail = 5;
    public float collisionProtectionTime = 1.25f;

    public int lives;
    public int coinsToSpawnOnClick;
    public int points;
    public int failPoints;
}
