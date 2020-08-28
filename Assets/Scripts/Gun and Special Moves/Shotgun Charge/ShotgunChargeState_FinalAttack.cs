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
        if (Services.playerController.isAboveFloor) { FireShockwave(shotgunCharge); }
        // Otherwise, just end this state right now.
        else { Debug.Log("Returning to idle mode."); GetComponent<TriggerTransition>().isTriggerSet = true; }

        Services.playerGameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Services.playerController.state = PlayerController.State.Normal;
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);

        // Keep gun state at 100% shotgun and fire it over and over.
        //GunValueManager.currentValue = -1f;
        //Services.gun.FireBurst();
    }

    public override void End(StateController stateController) {
        base.End(stateController);
        ShotgunCharge shotgunCharge = stateController as ShotgunCharge;

        //shotgunCharge.isReturningToFullSpeed = false;
        Services.healthManager.forceInvincibility = false;
        Services.playerGameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        //sloMoTimer = 0f;
    }


    void FireShockwave(ShotgunCharge shotgunCharge) {
        //Services.fallingSequenceManager.InstantiateShockwave(shockwavePrefab, 2f);
        Services.playerGameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
        //shotgunCharge.capturedEnemies.Clear();
        StartCoroutine(ShockwaveCoroutine());
    }


    IEnumerator ShockwaveCoroutine() {
        //yield return new WaitForSeconds(0.25f);
        Services.timeScaleManager.ReturnToFullSpeed(1f);
        //yield return new WaitForSeconds(0.25f);
        GetComponent<TriggerTransition>().isTriggerSet = true;
        yield return null;
    }
}
