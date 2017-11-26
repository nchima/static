using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunChargeState_ChargingUp : State {

    public override void Initialize(StateController stateController) {
        FindObjectOfType<FieldOfViewController>().TweenToShotgunChargeFOV();
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
        GunValueManager.currentGunValue = -1f;
    }

    public override void End(StateController stateController) {}
}
