using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunChargeState_Charging : State {

    Vector3 previousPlayerPosition;

    public override void Initialize(StateController stateController) {
        ShotgunCharge shotgunCharge = stateController as ShotgunCharge;

        Services.playerController.dashRechargeTimer = 0f;

        Services.musicManager.BeginDash();
        
        shotgunCharge.sphere.MoveIntoChargePosition();
        shotgunCharge.StoreDashDistance();
        shotgunCharge.currentDistanceDashed = 0f;
        shotgunCharge.dashLine.AttachToParent(false);
        previousPlayerPosition = Services.playerTransform.position;
        shotgunCharge.isCharging = true;

        FindObjectOfType<FieldOfViewController>().TweenToNormalFOV();


        // Make player temporarily invincible
        Services.healthManager.forceInvincibility = true;

        // Make the player start shotgun charging.
        Services.playerController.state = PlayerController.State.ShotgunCharge;

        // Make sure player does move up or down.
        Services.playerGameObject.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;

        //Services.specialBarManager.PlayerUsedSpecialMove();

        // Allow player to pass through railings.
        Physics.IgnoreLayerCollision(16, 24, true);

        // Add a kick to get the player going.
        Services.playerGameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Services.playerGameObject.GetComponent<Rigidbody>().AddForce(Services.playerTransform.forward * shotgunCharge.kickForce, ForceMode.Impulse);
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
        ShotgunCharge shotgunCharge = stateController as ShotgunCharge;
        shotgunCharge.currentDistanceDashed += Vector3.Distance(Services.playerTransform.position, previousPlayerPosition);
        previousPlayerPosition = Services.playerTransform.position;
    }

    public override void End(StateController stateController) {
        base.End(stateController);
        ShotgunCharge shotgunCharge = stateController as ShotgunCharge;
        shotgunCharge.dashLine.AttachToParent(true);
        shotgunCharge.dashLine.SetActive(false);
        Services.gun.canShoot = true;
        Services.uiManager.crosshair.SetActive(true);
        Services.musicManager.ExitDash();
    }
}
