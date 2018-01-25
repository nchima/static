using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LaserShot : EnemyShot {

    //const float PRE_DAMAGE_DURATION = 0.7f; Actually, this will be set by the firing enemy.
    public float preDamageDuration;
    const float DAMAGE_DURATION = 0.7f;
    public  float postDamageDuration = 0.5f;

    float timer = 0f;

    public enum State { PreDamage, Damage, PostDamage };
    public State state;

    /* USED FOR AUDIO */

    // The child game object which contains all my mesh renderers.
    [SerializeField] GameObject geometry;

    // Values for the thickness of my laser beam at various states.
    const float INITIAL_BEAM_THICKNESS = 0.01f;
    const float MAX_PRE_DAMAGE_BEAM_THICKNESS = 0.4f;
    const float DAMAGE_BEAM_THICKNESS = 4f;

    // Values for the color of my laser beam at various states.
    [SerializeField] Color finalPreDamageBeamColor;
    [SerializeField] Color damageColor;

    // Values for the speed at which my material tiling settings change at various stages.
    const float MATERIAL_OFFSET_SPEED_DAMAGE_STATE_X = 0.1f;
    const float MATERIAL_OFFSET_SPEED_DAMAGE_STATE_Y = 0.5f;

    [SerializeField] GameObject sphere1, sphere2;

    Vector2 currentMaterialOffsetSpeed = Vector2.zero;


    /* USED FOR AUDIO */

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip firingClip;
    const float AUDIO_PITCH_MAX = 3f;


    public void GetShot(Vector3 shotDirection, LaserEnemyStateShoot theEnemyWhoFiredMe)
    {
        base.Start();
        state = State.PreDamage;

        // Raycast in that direction to get the nearest solid collider.
        RaycastHit hit;
        if (Physics.Raycast(theEnemyWhoFiredMe.laserOrigin.position, shotDirection, out hit, 500f, 1<<8))
        {
            // Scale me correctly.
            geometry.transform.localScale = new Vector3(INITIAL_BEAM_THICKNESS, Vector3.Distance(theEnemyWhoFiredMe.laserOrigin.position, hit.point) * 0.5f, INITIAL_BEAM_THICKNESS);
            sphere1.transform.localScale = Vector3.one * INITIAL_BEAM_THICKNESS;
            sphere2.transform.localScale = Vector3.one * INITIAL_BEAM_THICKNESS;

            // Move my position halfway down the ray.
            transform.position += (shotDirection * Vector3.Distance(theEnemyWhoFiredMe.laserOrigin.position, hit.point)) * 0.5f;

            // Rotate me correctly.
            transform.rotation = Quaternion.LookRotation(Vector3.down, shotDirection);

            sphere1.transform.position = hit.point;
            sphere1.transform.localPosition = new Vector3(sphere1.transform.localPosition.x, sphere1.transform.localPosition.y, 0f);
            sphere2.transform.position = theEnemyWhoFiredMe.laserOrigin.position;
            sphere2.transform.localPosition = new Vector3(sphere2.transform.localPosition.x, sphere2.transform.localPosition.y, 0f);
        }

        // Begin tweening.
        geometry.transform.DOScaleX(MAX_PRE_DAMAGE_BEAM_THICKNESS, preDamageDuration * 0.8f);
        geometry.transform.DOScaleZ(MAX_PRE_DAMAGE_BEAM_THICKNESS, preDamageDuration * 0.8f);
        sphere1.transform.DOScale(MAX_PRE_DAMAGE_BEAM_THICKNESS, preDamageDuration * 0.8f);
        sphere2.transform.DOScale(MAX_PRE_DAMAGE_BEAM_THICKNESS, preDamageDuration * 0.8f);

        geometry.transform.Find("Inner Tube").GetComponent<MeshRenderer>().material.DOColor(finalPreDamageBeamColor, "_EmissionColor", preDamageDuration * 0.9f);

        audioSource.DOPitch(AUDIO_PITCH_MAX, preDamageDuration * 0.8f).SetEase(Ease.Linear);
    }


    new void Update()
    {
        base.Update();

        // Handle texture animation.
        if (currentMaterialOffsetSpeed != Vector2.zero)
        {
            foreach(MeshRenderer mr in geometry.GetComponentsInChildren<MeshRenderer>())
            {
                Vector2 newOffset = mr.material.mainTextureOffset + currentMaterialOffsetSpeed;
                mr.material.mainTextureOffset = newOffset;
            }
        }

        switch(state)
        {
            case State.PreDamage:
                PreDamage();
                break;
            case State.Damage:
                Damage();
                break;
            case State.PostDamage:
                PostDamage();
                break;
        }
    }


    void PreDamage()
    {
        timer += Time.deltaTime;
        if (timer >= preDamageDuration)
        {
            // Tween to damage state values.
            geometry.transform.DOScaleX(DAMAGE_BEAM_THICKNESS, 0.05f);
            geometry.transform.DOScaleZ(DAMAGE_BEAM_THICKNESS, 0.05f);
            sphere1.transform.DOScale(DAMAGE_BEAM_THICKNESS, 0.05f);
            sphere2.transform.DOScale(DAMAGE_BEAM_THICKNESS, 0.05f);

            geometry.transform.Find("Inner Tube").GetComponent<MeshRenderer>().material.DOColor(damageColor, "_EmissionColor", 0.05f);
            geometry.transform.Find("Outer Tube").GetComponent<MeshRenderer>().material.DOColor(finalPreDamageBeamColor, "_EmissionColor", 0.05f);

            currentMaterialOffsetSpeed = new Vector2(MATERIAL_OFFSET_SPEED_DAMAGE_STATE_X, MATERIAL_OFFSET_SPEED_DAMAGE_STATE_Y);

            audioSource.Stop();
            audioSource.clip = firingClip;
            audioSource.loop = false;
            audioSource.volume = 1f;
            audioSource.pitch = 0.8f;
            audioSource.Play();

            timer = 0f;
            state = State.Damage;
        }
    }

    void Damage()
    {
        timer += Time.deltaTime;
        if (timer >= DAMAGE_DURATION)
        {
            ReadyPostDamage();
        }
    }

    public void ReadyPostDamage()
    {
        timer = 0f;
        state = State.PostDamage;

        // Begin tweening to post damage values.
        geometry.transform.DOScaleX(INITIAL_BEAM_THICKNESS, 0.05f);
        geometry.transform.DOScaleZ(INITIAL_BEAM_THICKNESS, 0.05f);
        sphere1.transform.DOScale(INITIAL_BEAM_THICKNESS, 0.05f);
        sphere2.transform.DOScale(INITIAL_BEAM_THICKNESS, 0.05f);

        geometry.transform.Find("Inner Tube").GetComponent<MeshRenderer>().material.DOColor(finalPreDamageBeamColor, "_EmissionColor", 0.05f);
        geometry.transform.Find("Outer Tube").GetComponent<MeshRenderer>().material.DOColor(finalPreDamageBeamColor, "_EmissionColor", 0.05f);

        currentMaterialOffsetSpeed = new Vector2(MATERIAL_OFFSET_SPEED_DAMAGE_STATE_X * 0.1f, MATERIAL_OFFSET_SPEED_DAMAGE_STATE_Y * 0.1f);
    }

    void PostDamage()
    {
        timer += Time.deltaTime;
        if (timer >= postDamageDuration)
        {
            Destroy(gameObject);
        }
    }


    void OnTriggerEnterChild(Collider other)
    {
        if (state == State.Damage && other.transform == GameManager.player.transform)
        {
            GameManager.instance.PlayerWasHurt();
            GetComponentInChildren<Collider>().enabled = false;
        }
    }
}
