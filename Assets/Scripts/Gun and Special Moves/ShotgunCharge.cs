using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShotgunCharge : MonoBehaviour {

	ShotgunChargeSphere sphere;

    [SerializeField] int collideDamage = 20;
    [SerializeField] float kickForce = 100f;


    List<GameObject> capturedEnemies = new List<GameObject>();

    bool isReturningToFullSpeed;
    bool isFiringShockwave = false;
    float slowMoDuration = 0.25f;
    float sloMoTimer = 0f;
    [SerializeField] GameObject shockwavePrefab;

    bool isCharging = false;
    float chargeTimer = 0f;
    float minimumChargeDuration = 0.5f;
    public bool hasChargedForMinimumTime {
        get {
            // See if the player has been charging for at least the minimum time allowed.
            if (chargeTimer >= minimumChargeDuration) { Debug.Log("has charged for minimum time");  return true; }
            else { return false; }
        }
    }

    Transform player;


    private void Awake() {
        player = FindObjectOfType<PlayerController>().transform;
        sphere = GetComponentInChildren<ShotgunChargeSphere>();
    }


    private void Update() {

        if (isCharging) {
            chargeTimer += Time.deltaTime;
        }

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
            else if (sloMoTimer >= slowMoDuration + 0.25f)
            {
                ResetAllVariables();
            }
        }
    }


    void ResetAllVariables() {
        isFiringShockwave = false;
        isReturningToFullSpeed = false;
        GameManager.instance.forceInvincibility = false;
        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        sloMoTimer = 0f;
    }


    public void BeginCharge() {
        sphere.BeginCharge();

        // Make player temporarily invisible.
        GameManager.instance.forceInvincibility = true;

        // Make sure player does move up or down.
        player.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;

        // Allow player to pass through railings.
        Physics.IgnoreLayerCollision(16, 24, true);

        // Add a kick to get the player going.
        player.GetComponent<Rigidbody>().AddForce(player.transform.forward * kickForce, ForceMode.Impulse);

        chargeTimer = 0f;
        isCharging = true;
    }


    public void EndCharge() {
        sphere.EndCharge();

        if (player.GetComponent<PlayerController>().state != PlayerController.State.ShotgunCharge) { return; }

        isCharging = false;

        Physics.IgnoreLayerCollision(16, 24, false);

        if (player.GetComponent<PlayerController>().isAboveFloor) { FireShockwave(); }
        else { ResetAllVariables(); }

        player.GetComponent<PlayerController>().state = PlayerController.State.Normal;
    }

    void FireShockwave() {
        GameManager.fallingSequenceManager.InstantiateShockwave(shockwavePrefab, 50f);
        player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
        capturedEnemies.Clear();
        isFiringShockwave = true;
    }

    public void OnTriggerEnter(Collider other) {
        if (isCharging && other.GetComponent<Enemy>() != null && !capturedEnemies.Contains(other.gameObject)) {
            other.GetComponent<Enemy>().HP -= collideDamage;
            other.GetComponent<Enemy>().BecomePhysicsObject(2f);
            capturedEnemies.Add(other.gameObject);
        }
    }
}
