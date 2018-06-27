using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunChargeState_ChargingUp : State {

    public override void Initialize(StateController stateController) {
        ShotgunCharge shotgunCharge = stateController as ShotgunCharge;
        shotgunCharge.dashLine.SetActive(true);
        FindObjectOfType<FieldOfViewController>().TweenToShotgunChargeFOV();
        Services.gun.canShoot = false;
        Services.uiManager.crosshair.SetActive(false);
        Services.musicManager.EnterDashCharge();
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
    }

    public override void End(StateController stateController) {
        base.End(stateController);
        Services.playerController.isSuperDashCharging = false;
    }
}
