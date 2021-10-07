using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectBehaviour { Expand, Travel, Jump }
public enum DestroyBehaviour { Explode, Fade, Fall }
public enum ObjectType { YellowBlock }

[RequireComponent(typeof(Rigidbody))]
public class ClickeableObject : CachedTransform, IClickeable
{
    [SerializeField] ParticleSystem explosionPS;
    [SerializeField] MeshRenderer meshRenderer;

    public ObjectBehaviour ObjBehaviour => data.objectBehaviour;
    public ObjectType ObjType => data.objectType;

    Rigidbody _RBody;
    Rigidbody RBody
    {
        get
        {
            if (_RBody == null) _RBody = GetComponent<Rigidbody>();
            return _RBody;
        }
    }

    bool canTravel, enableBounceTimer, enableTimer;
    int lives;
    float bounceTimeToDestroy, lifeTime;
    Vector3 travelDestiny;
    SpawneableObjectData data;
    ObjectSpawner objectSpawner;

    public System.Action<ClickeableObject> OnDeath;

    void Update()
    {
        if (enableTimer && Time.time >= lifeTime)
        {
            enableTimer = false;
            DestroyObject(false);
        }

        if (enableBounceTimer && Time.time >= bounceTimeToDestroy)
        {
            enableBounceTimer = false;
            DestroyObject(false);
        }

        if (!canTravel) return;

        if (Vector3.Distance(MyTransform.position, travelDestiny) < .1f)
        {
            canTravel = false;
            DestroyObject(false);
        }

        Vector3 travelDirection = Vector3.Normalize(travelDestiny - MyTransform.position);
        MyTransform.Translate(Time.deltaTime * data.travelSpeed * travelDirection, Space.World);
    }

    public void Init(SpawneableObjectData data, ObjectSpawner spawner)
    {
        this.data = data;
        lives = data.lives;
        objectSpawner = spawner;
    }

    public void StartBehaviour()
    {
        switch (ObjBehaviour)
        {
            case ObjectBehaviour.Expand:
                RBody.isKinematic = true;
                MyTransform.localScale = Vector3.zero;
                LeanTween.scale(gameObject, Vector3.one, .5f).setEaseOutBounce();

                enableBounceTimer = true;
                bounceTimeToDestroy = Time.time + 5.5f;
                break;

            case ObjectBehaviour.Travel:
                canTravel = true;
                RBody.isKinematic = true;

                travelDestiny = MyTransform.position + MyTransform.forward * 50;
                float rotation = Random.Range(0, 1) < .5f ? -360 : 360;
                LeanTween.rotateAround(gameObject, Vector3.up, rotation, .5f).setLoopCount(int.MaxValue);

                //Debug.DrawLine(MyTransform.position, travelDestiny, Color.green, 5);
                //Debug.DrawRay(MyTransform.position, travelDirection, Color.red, 5);
                //print($"Forward: {MyTransform.forward} - Travel Destiny: {travelDestiny}");
                break;

            case ObjectBehaviour.Jump:
                MakeJump(10, false);
                break;
        }

        enableTimer = true;
        lifeTime = Time.time + data.timeToFail;
    }

    public void OnClick()
    {
        lives--;
        if (lives < 1) DestroyObject(true);
    }

    void DestroyObject(bool byClick)
    {
        if (byClick)
        {
            switch (data.destroyBehaviour)
            {
                case DestroyBehaviour.Explode:
                    explosionPS.Play();
                    meshRenderer.enabled = false;

                    LeanTween.value(0, 1, 1).setOnComplete(() => { OnDeath?.Invoke(this); });
                    break;

                case DestroyBehaviour.Fade:
                    FadeAnim();
                    break;

                case DestroyBehaviour.Fall:
                    MakeJump(5, true);
                    LeanTween.value(0, 1, 5).setOnComplete(() => { OnDeath?.Invoke(this); });
                    break;
            }

            LevelManager.INS.ScorePoints(data.points);
        }
        else
        {
            OnDeath?.Invoke(this);
            LevelManager.INS.RemovePoints(data.failPoints);
        }
    }

    void MakeJump(float force, bool randomDir)
    {
        RBody.isKinematic = false;

        float forceDir;
        if (randomDir)
        {
            forceDir = Random.Range(-1f, 1f);
            if (forceDir > -.15f && forceDir < .15f) forceDir = .15f;
        }
        else
        {
            float xPos = MyTransform.position.x - objectSpawner.MyTransform.position.x;
            forceDir = xPos > 0 ? -1 : 1;
        }

        RBody.AddForce(new Vector3(forceDir, 1, 0) * force, ForceMode.Impulse);
        RBody.AddTorque(new Vector3(0, 1, 1) * Random.Range(-10f, 10f));
    }

    public void ResetData()
    {
        RBody.isKinematic = true;
        meshRenderer.enabled = true;
        enableBounceTimer = false;
        canTravel = false;
        enableTimer = false;
        LeanTween.cancel(gameObject);
    }

    void FadeAnim()
    {
        LeanTween.cancel(gameObject);
        MaterialPropertyBlock colorProperty = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(colorProperty);

        Color originalColor = colorProperty.GetColor("_Color");
        LeanTween.value(1, 0, .8f).setOnUpdate((value) =>
        {
            originalColor.a = value;
            colorProperty.SetColor("_Color", originalColor);
            meshRenderer.SetPropertyBlock(colorProperty);
        }
        ).setOnComplete(() => { OnDeath?.Invoke(this); });

        LeanTween.rotateY(gameObject, 360, .1f).setLoopCount(10);
    }
}
