using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunChargeTransition_ReachedEndPoint : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        ShotgunCharge shotgunCharge = stateController as ShotgunCharge;
        if (shotgunCharge.currentDistanceDashed >= shotgunCharge.currentDashEndDistance
            && Services.playerController.isAboveFloor) {
            return true;
        } else {
            return false;
        }
    }
}
