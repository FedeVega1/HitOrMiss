using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum ObjectBehaviour { Expand, Travel, Jump, Hover }
public enum DestroyBehaviour { Explode, Fade, Fall }
public enum ObjectType { YellowBlock, BlueSphere, Coin, RedBox, Shield, TargetMark }

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class ClickeableObject : CachedTransform, IClickeable
{
    [SerializeField] ParticleSystem explosionPS, clickPS;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] BoxCollider boxCollider;
    [SerializeField] Transform lblPointsRoot;
    [SerializeField] TMP_Text lblPoints;
    [SerializeField] AudioSource clickASrc, killASrc;
    [SerializeField] AudioClip[] clickClips, killClips;

    public ObjectBehaviour ObjBehaviour => data.objectBehaviour;
    public ObjectType ObjType => data.objectType;

    Transform _MainCameraTransform;
    Transform MainCameraTransform
    {
        get
        {
            if (_MainCameraTransform == null) _MainCameraTransform = Camera.main.transform;
            return _MainCameraTransform;
        }
    }

    Rigidbody _RBody;
    Rigidbody RBody
    {
        get
        {
            if (_RBody == null) _RBody = GetComponent<Rigidbody>();
            return _RBody;
        }
    }

    bool canTravel, enableBounceTimer, enableTimer, enableCollisionTimer, enableHover;
    int lives;
    float bounceTimeToDestroy, lifeTime, rotationSign, collisionTimer, hoverTimer;

    Vector3 travelDestiny, startScale;
    Quaternion startRotation;

    SpawneableObjectData data;
    ObjectSpawner objectSpawner;

    public System.Action<ClickeableObject> OnDeath;

    void Update()
    {
        if (lblPointsRoot != null && lblPointsRoot.gameObject.activeSelf)
            lblPointsRoot.LookAt(MainCameraTransform, Vector3.up);

        if (enableHover && Time.time >= hoverTimer)
            RBody.AddForce(Vector3.up * 12, ForceMode.Impulse);

        if (enableCollisionTimer && Time.time >= collisionTimer)
        {
            enableCollisionTimer = false;
            boxCollider.enabled = true;
        }

        if (enableTimer && Time.time >= lifeTime)
        {
            //print("<color=red>Time's up</color>");
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
        if (enableHover) RBody.AddForce(Vector3.down * 5);

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

    public void Init(SpawneableObjectData data, ObjectSpawner spawner, DifficultyLevel currentDifficulty)
    {
        this.data = data;
        lives = data.lives;
        objectSpawner = spawner;

        if (lblPoints != null)
        {
            if (!LevelManager.INS.EnableHelperPoints)
            {
                lblPointsRoot.gameObject.SetActive(false);
                return;
            }

            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:
                    lblPointsRoot.gameObject.SetActive(true);
                    if (data.objectType != ObjectType.RedBox)
                    {
                        lblPoints.text = '+' + data.points.ToString();
                        return;
                    }

                    lblPoints.text = data.points.ToString();
                    break;

                case DifficultyLevel.Medium:
                    if (data.objectType != ObjectType.RedBox)
                    {
                        lblPointsRoot.gameObject.SetActive(false);
                        return;
                    }

                    lblPointsRoot.gameObject.SetActive(true);
                    lblPoints.text = data.points.ToString();
                    break;

                case DifficultyLevel.Hard:
                    lblPointsRoot.gameObject.SetActive(false);
                    break;
            }
        }
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
                EnableCollisionTimer();

                //float rotation = Random.Range(0, 1) < .5f ? -360 : 360;
                //LeanTween.rotateAround(gameObject, Vector3.up, rotation, .5f).setLoopCount(int.MaxValue);

                //Debug.DrawLine(MyTransform.position, travelDestiny, Color.green, 5);
                //Debug.DrawRay(MyTransform.position, travelDirection, Color.red, 5);
                //print($"Forward: {MyTransform.forward} - Travel Destiny: {travelDestiny}");
                break;

            case ObjectBehaviour.Jump:
                MakeJump(Random.Range(8f, 10f), false);
                EnableCollisionTimer();
                break;

            case ObjectBehaviour.Hover:
                RBody.isKinematic = false;
                enableHover = true;
                hoverTimer = Time.time + 2.5f;

                RBody.AddForce(Vector3.up * 7, ForceMode.Impulse);
                break;
        }

        enableTimer = true;
        lifeTime = Time.time + data.timeToFail;
    }

    void EnableCollisionTimer()
    {
        boxCollider.enabled = false;
        enableCollisionTimer = true;
        collisionTimer = Time.time + 1.25f;
    }

    public void OnClick(Vector3 point)
    {
        if (clickPS != null)
        {
            clickPS.transform.position = point;
            clickPS.Play();
        }

        if (clickASrc != null)
        {
            clickASrc.clip = clickClips[Random.Range(0, clickClips.Length)];
            clickASrc.pitch = Random.Range(.8f, 1.2f);
            clickASrc.Play();
        }

        lives--;
        if (lives < 1) DestroyObject(true);
        if (data.objectType == ObjectType.TargetMark) objectSpawner.ForceCoinsToSpawn(data.coinsToSpawnOnClick);
    }

    void DestroyObject(bool byClick)
    {
        enableHover = false;
        boxCollider.enabled = false;
        if (lblPointsRoot != null) lblPointsRoot.gameObject.SetActive(false);

        if (byClick)
        {
            if (killASrc != null)
            {
                killASrc.clip = killClips[Random.Range(0, killClips.Length)];
                killASrc.pitch = Random.Range(.8f, 1.2f);
                killASrc.Play();
            }

            LeanTween.cancel(gameObject);

            switch (data.destroyBehaviour)
            {
                case DestroyBehaviour.Explode:
                    if (explosionPS != null) explosionPS.Play();
                    meshRenderer.enabled = false;
                    canTravel = false;
                    RBody.isKinematic = true;

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

            //print($"Add {data.points} points");
            LevelManager.INS.ScorePoints(data.points);
        }
        else
        {
            //print($"Remove {data.failPoints} points");
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
        ).setOnComplete(() => 
        {
            colorProperty.SetColor("_Color", Color.white);
            meshRenderer.SetPropertyBlock(colorProperty);
            OnDeath?.Invoke(this); 
        });

        LeanTween.rotateAround(gameObject, Vector3.up, 360, .1f).setLoopCount(10);
    }

    public void FreezeObject()
    {
        RBody.isKinematic = true;
        canTravel = false;
        LeanTween.cancel(gameObject);
        enableTimer = enableBounceTimer = enabled = false;
    }

    public void SetSFXVolume(float newValue)
    {
        if (killASrc != null) killASrc.volume = newValue;
        if (clickASrc != null) clickASrc.volume = newValue;
    }
}
