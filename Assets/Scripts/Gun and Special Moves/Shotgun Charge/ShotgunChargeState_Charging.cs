using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunChargeState_Charging : State {

    public override void Initialize(StateController stateController) {
        ShotgunCharge shotgunCharge = stateController as ShotgunCharge;
        
        shotgunCharge.sphere.MoveIntoChargePosition();

        shotgunCharge.isCharging = true;

        FindObjectOfType<FieldOfViewController>().TweenToNormalFOV();

        // Make player temporarily invincible (Why am I setting two separate variables for this??)
        Services.healthManager.forceInvincibility = true;

        // Make the player start shotgun charging.
        Services.playerController.state = PlayerController.State.ShotgunCharge;

        // Make sure player does move up or down.
        Services.playerGameObject.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;

        Services.specialBarManager.PlayerUsedSpecialMove();

        // Allow player to pass through railings.
        Physics.IgnoreLayerCollision(16, 24, true);

        // Add a kick to get the player going.
        Services.playerGameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Services.playerGameObject.GetComponent<Rigidbody>().AddForce(Services.playerTransform.forward * shotgunCharge.kickForce, ForceMode.Impulse);
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
    }
}
