using UnityEngine;

[CreateAssetMenu(fileName = "ObjectData", menuName = "DataObjects/SpawneableObjectData")]
public class SpawneableObjectData : ScriptableObject
{
    public ObjectType objectType;
    public ObjectBehaviour objectBehaviour;
    public DestroyBehaviour destroyBehaviour;

    public float travelSpeed;
    public float timeToFail = 5;

    public int lives;
    public int points;
    public int failPoints;
}
