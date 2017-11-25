using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShotgunCharge : StateController {

	[HideInInspector] public ShotgunChargeSphere sphere;

    [SerializeField] int collideDamage = 20;
    public float kickForce = 100f;

    List<GameObject> capturedEnemies = new List<GameObject>();

    bool isReturningToFullSpeed;
    [HideInInspector] public bool isFiringShockwave = false;
    float slowMoDuration = 0.25f;
    float sloMoTimer = 0f;

    [HideInInspector] public bool isCharging = false;
    [HideInInspector] public float chargeTimer = 0f;
    public float minimumChargeDuration = 0.5f;
    public bool hasChargedForMinimumTime {
        get {
            // See if the player has been charging for at least the minimum time allowed.
            if (chargeTimer >= minimumChargeDuration) { Debug.Log("has charged for minimum time");  return true; }
            else { return false; }
        }
    }

    [SerializeField] TriggerTransition chargeTrigger;

    Transform player;


    private void Awake() {
        player = FindObjectOfType<PlayerController>().transform;
        sphere = GetComponentInChildren<ShotgunChargeSphere>();
    }


    protected override void Update() {
        base.Update();

        // Keep captured enemies in front of player.
        for (int i = 0; i < capturedEnemies.Count; i++) {
            if (capturedEnemies[i] != null) {
                Vector3 forceDirection = Vector3.Normalize((transform.parent.position + transform.parent.forward * 15f) - capturedEnemies[i].transform.position);
                capturedEnemies[i].GetComponent<Rigidbody>().AddForce(forceDirection * 20f, ForceMode.Impulse);
            }

            else {
                capturedEnemies.Remove(capturedEnemies[i]);
            }
        }

        if (isFiringShockwave)
        {
            GameManager.instance.currentSine = -1f;

            GameManager.instance.gun.FireBurst();

            sloMoTimer += Time.deltaTime;
            if (sloMoTimer >= slowMoDuration && !isReturningToFullSpeed)
            {
                GameManager.instance.ReturnToFullSpeed();
                isReturningToFullSpeed = true;
            }

            // Finish shockwave sequence.
            else if (sloMoTimer >= slowMoDuration + 0.25f) {
                ResetAllVariables();
            }
        }
    }


    public void BeginSequence() {
        chargeTrigger.isTriggerSet = true;
    }


    public void ResetAllVariables() {
        //isFiringShockwave = false;
        //isReturningToFullSpeed = false;
        //GameManager.instance.forceInvincibility = false;
        //player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        //sloMoTimer = 0f;
    }


    public void EndCharge() {
    }


    public void OnTriggerEnter(Collider other) {
        // If we collide into an enemy while charging, capture it.
        if (isCharging && other.GetComponent<Enemy>() != null && !capturedEnemies.Contains(other.gameObject)) {
            other.GetComponent<Enemy>().HP -= collideDamage;
            other.GetComponent<Enemy>().BecomePhysicsObject(2f);
            capturedEnemies.Add(other.gameObject);
        }
    }
}
