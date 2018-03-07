using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunChargeState_FinalAttack : State {

    [SerializeField] GameObject shockwavePrefab;


    public override void Initialize(StateController stateController) {
        ShotgunCharge shotgunCharge = stateController as ShotgunCharge;

        // Return visual effects sphere to original position.
        shotgunCharge.sphere.EndCharge();

        shotgunCharge.isCharging = false;
        shotgunCharge.chargeTimer = 0f;

        // Make player once again collide with railings.
        Physics.IgnoreLayerCollision(16, 24, false);

        // If the player is above a floor then fire the shockwave.
        if (GameManager.player.GetComponent<PlayerController>().isAboveFloor) { FireShockwave(shotgunCharge); }
        // Otherwise, just end this state right now.
        else { Debug.Log("Returning to idle mode."); GetComponent<TriggerTransition>().isTriggerSet = true; }

        GameManager.player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        GameManager.player.GetComponent<PlayerController>().state = PlayerController.State.Normal;
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);

        // Keep gun state at 100% shotgun and fire it over and over.
        GunValueManager.currentValue = -1f;
        GameManager.instance.gun.FireBurst();
    }

    public override void End(StateController stateController) {
        ShotgunCharge shotgunCharge = stateController as ShotgunCharge;

        //shotgunCharge.isReturningToFullSpeed = false;
        GameManager.healthManager.forceInvincibility = false;
        GameManager.player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        //sloMoTimer = 0f;
    }


    void FireShockwave(ShotgunCharge shotgunCharge) {
        GameManager.fallingSequenceManager.InstantiateShockwave(shockwavePrefab, 50f);
        GameManager.player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
        //shotgunCharge.capturedEnemies.Clear();
        StartCoroutine(ShockwaveCoroutine());
    }


    IEnumerator ShockwaveCoroutine() {
        yield return new WaitForSeconds(0.25f);
        GameManager.instance.ReturnToFullSpeed();
        yield return new WaitForSeconds(0.25f);
        GetComponent<TriggerTransition>().isTriggerSet = true;
        yield return null;
    }
}
