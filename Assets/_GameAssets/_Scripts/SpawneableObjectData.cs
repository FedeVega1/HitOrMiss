using UnityEngine;

[CreateAssetMenu(fileName = "ObjectData", menuName = "DataObjects/SpawneableObjectData")]
public class SpawneableObjectData : ScriptableObject
{
    public ObjectBehaviour objectBehaviour;
    public DestroyBehaviour destroyBehaviour;

    public float travelSpeed;
    public float timeToFail;

    public int lives;
    public int points;
    public int failPoints;
}
