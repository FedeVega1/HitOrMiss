using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectBehaviour { Expand, Travel, Jump }
public enum DestroyBehaviour { Explode, Fade, Fall }

[RequireComponent(typeof(Rigidbody))]
public class ClickeableObject : CachedTransform, IClickeable
{
    [SerializeField] ParticleSystem explosionPS;
    [SerializeField] MeshRenderer meshRenderer;

    Rigidbody _RBody;
    Rigidbody RBody
    {
        get
        {
            if (_RBody == null) _RBody = GetComponent<Rigidbody>();
            return _RBody;
        }
    }

    bool canTravel, enableBounceTimer;
    int lives;
    float bounceTimeToDestroy;
    Vector3 travelDirection, travelDestiny;
    SpawneableObjectData data;

    public System.Action<ClickeableObject> OnDeath;

    void Update()
    {
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

        MyTransform.Translate(Time.deltaTime * data.travelSpeed * travelDirection);
    }

    public void Init(SpawneableObjectData data)
    {
        this.data = data;
        lives = data.lives;
    }

    public void StartBehaviour()
    {
        switch (data.objectBehaviour)
        {
            case ObjectBehaviour.Expand:
                RBody.isKinematic = true;
                MyTransform.localScale = Vector3.zero;
                LeanTween.scale(gameObject, Vector3.one, .5f).setEaseOutBounce();

                enableBounceTimer = true;
                bounceTimeToDestroy = Time.time + 5.5f;
                break;

            case ObjectBehaviour.Travel:
                RBody.isKinematic = true;
                travelDirection = Vector3.right;
                travelDestiny = Vector3.zero;
                break;

            case ObjectBehaviour.Jump:
                MakeJump(10);
                break;
        }
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
                    MakeJump(5);
                    LeanTween.value(0, 1, 5).setOnComplete(() => { OnDeath?.Invoke(this); });
                    break;
            }
        }
    }

    void MakeJump(float force)
    {
        RBody.isKinematic = false;

        float randomX = Random.Range(-1f, 1f);
        if (randomX > -.15f && randomX < .15f) randomX = .15f;

        RBody.AddForce(new Vector3(randomX, 1, 0) * force, ForceMode.Impulse);
        RBody.AddTorque(new Vector3(0, 1, 1) * Random.Range(-10f, 10f));
    }

    public void ResetData()
    {
        RBody.isKinematic = true;
        meshRenderer.enabled = true;
        enableBounceTimer = false;
        canTravel = false;
    }

    void FadeAnim()
    {
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
