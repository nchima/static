using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShotgunCharge : StateController {

	[HideInInspector] public ShotgunChargeSphere sphere;

    [SerializeField] int collideDamage = 20;
    public float kickForce = 100f;

    List<GameObject> capturedEnemies = new List<GameObject>();

    [HideInInspector] public bool isCharging = false;
    [HideInInspector] public float chargeTimer = 0f;
    public float minimumChargeDuration = 0.5f;
    public bool hasChargedForMinimumTime {
        get {
            // See if the player has been charging for at least the minimum time allowed.
            if (chargeTimer >= minimumChargeDuration) { return true; }
            else { return false; }
        }
    }

    [SerializeField] TriggerTransition chargeTrigger;
    public ChargeDashLine dashLine { get { return GetComponentInChildren<ChargeDashLine>(); } }

    [HideInInspector] public float currentDashEndDistance;
    [HideInInspector] public float currentDistanceDashed;

    Transform player;


    private void Awake() {
        player = FindObjectOfType<PlayerController>().transform;
        sphere = GetComponentInChildren<ShotgunChargeSphere>();
    }


    protected override void Update() {
        base.Update();

        if (isCharging) {
            chargeTimer += Time.deltaTime;
        }

        // Keep captured enemies in front of player.
        //for (int i = 0; i < capturedEnemies.Count; i++) {
        //    if (capturedEnemies[i] != null) {
        //        Vector3 forceDirection = Vector3.Normalize((transform.parent.position + transform.parent.forward * 15f) - capturedEnemies[i].transform.position);
        //        capturedEnemies[i].GetComponent<Rigidbody>().AddForce(forceDirection * 20f, ForceMode.Impulse);
        //    }

        //    else {
        //        capturedEnemies.Remove(capturedEnemies[i]);
        //    }
        //}
    }


    public void BeginSequence() {
        chargeTrigger.isTriggerSet = true;
    }


    public void StoreDashDistance() {
        currentDashEndDistance = dashLine.distance;
    }

    //public void OnTriggerEnter(Collider other) {
    //    // If we collide into an enemy while charging, capture it.
    //    if (isCharging && other.GetComponent<Enemy>() != null && !capturedEnemies.Contains(other.gameObject)) {
    //        other.GetComponent<Enemy>().HP -= collideDamage;
    //        other.GetComponent<Enemy>().BecomePhysicsObject(2f);
    //        capturedEnemies.Add(other.gameObject);
    //    }
    //}
}
