﻿using System;
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
        GameManager.instance.forceInvincibility = true;
        GameManager.instance.invincible = true;

        // Make the player start shotgun charging.
        GameManager.player.GetComponent<PlayerController>().state = PlayerController.State.ShotgunCharge;

        // Make sure player does move up or down.
        GameManager.player.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;

        // Allow player to pass through railings.
        Physics.IgnoreLayerCollision(16, 24, true);

        // Add a kick to get the player going.
        GameManager.player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        GameManager.player.GetComponent<Rigidbody>().AddForce(GameManager.player.transform.forward * shotgunCharge.kickForce, ForceMode.Impulse);
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
        GunValueManager.currentGunValue = -1f;
    }

    public override void End(StateController stateController) {
        
    }
}
