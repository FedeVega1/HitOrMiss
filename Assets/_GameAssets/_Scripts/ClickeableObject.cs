using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectBehaviour { Expand, Travel, Jump }
public enum DestroyBehaviour { Explode, Fade, Fall }
public enum ObjectType { YellowBlock, BlueSphere, Coin, RedBox, Shield, TargetMark }

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
    float bounceTimeToDestroy, lifeTime, rotationSign;

    Vector3 travelDestiny, startScale;
    Quaternion startRotation;

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
    }

    void FixedUpdate()
    {
        if (!canTravel) return;

        if (Vector3.Distance(RBody.position, travelDestiny) < .1f)
        {
            canTravel = false;
            DestroyObject(false);
        }

        Vector3 travelDirection = Vector3.Normalize(travelDestiny - MyTransform.position);
        RBody.MovePosition(RBody.position + (data.travelSpeed * Time.deltaTime * travelDirection));
        RBody.MoveRotation(Quaternion.Euler(RBody.rotation.eulerAngles + new Vector3(0, rotationSign * 100 * Time.deltaTime, 0)));
        //MyTransform.Translate(Time.deltaTime * data.travelSpeed * travelDirection, Space.World);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!canTravel) return;

        if (data.objectBehaviour == ObjectBehaviour.Travel)
        {
            Vector3 normal = collision.GetContact(0).normal;
            travelDestiny = RBody.position + normal * 50;
            rotationSign = normal.x > 0 ? 1 : -1;
        }
    }

    public void Init(SpawneableObjectData data, ObjectSpawner spawner)
    {
        this.data = data;
        lives = data.lives;
        objectSpawner = spawner;
    }

    public void StartBehaviour()
    {
        startScale = MyTransform.localScale;
        startRotation = MyTransform.rotation;

        switch (ObjBehaviour)
        {
            case ObjectBehaviour.Expand:
                RBody.isKinematic = true;
                MyTransform.localScale = Vector3.zero;

                LeanTween.scale(gameObject, startScale, .5f).setEaseOutBounce();

                enableBounceTimer = true;
                bounceTimeToDestroy = Time.time + 5.5f;
                break;

            case ObjectBehaviour.Travel:
                canTravel = true;
                RBody.isKinematic = true;

                travelDestiny = MyTransform.position + MyTransform.forward * 50;
                rotationSign = Random.Range(0f, 1f) <= .5f ? -1 : 1;
                //float rotation = Random.Range(0, 1) < .5f ? -360 : 360;
                //LeanTween.rotateAround(gameObject, Vector3.up, rotation, .5f).setLoopCount(int.MaxValue);

                //Debug.DrawLine(MyTransform.position, travelDestiny, Color.green, 5);
                //Debug.DrawRay(MyTransform.position, travelDirection, Color.red, 5);
                //print($"Forward: {MyTransform.forward} - Travel Destiny: {travelDestiny}");
                break;

            case ObjectBehaviour.Jump:
                MakeJump(Random.Range(8f, 11f), false);
                break;
        }

        enableTimer = true;
        lifeTime = Time.time + data.timeToFail;
    }

    public void OnClick()
    {
        lives--;
        if (lives < 1) DestroyObject(true);
        if (data.objectType == ObjectType.TargetMark) objectSpawner.ForceCoinsToSpawn(data.coinsToSpawnOnClick);
    }

    void DestroyObject(bool byClick)
    {
        if (byClick)
        {
            LeanTween.cancel(gameObject);

            switch (data.destroyBehaviour)
            {
                case DestroyBehaviour.Explode:
                    if (explosionPS != null) explosionPS.Play();
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

        MyTransform.localScale = startScale;
        MyTransform.rotation = startRotation;

        LeanTween.cancel(gameObject);
    }

    void FadeAnim()
    {
        LeanTween.cancel(gameObject);
        MaterialPropertyBlock colorProperty = new MaterialPropertyBlock();
        Color originalColor = Color.white;

        LeanTween.value(1, 0, .8f).setOnUpdate((value) =>
        {
            originalColor.a = value;
            colorProperty.SetColor("_Color", originalColor);
            meshRenderer.SetPropertyBlock(colorProperty);
        }
        ).setOnComplete(() => { OnDeath?.Invoke(this); });

        LeanTween.rotateY(gameObject, 360, .1f).setLoopCount(10);
    }

    public void FreezeObject()
    {
        RBody.isKinematic = true;
        canTravel = false;
        LeanTween.cancel(gameObject);
        enableTimer = enableBounceTimer = enabled = false;
    }
}
