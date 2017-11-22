using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunChargeTransition_EndCharge : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        bool buttonReleased = Input.GetButtonUp("Fire2");
        bool playerIsAboveFloor = FindObjectOfType<PlayerController>().isAboveFloor;
        bool hasChargedForMinimumTime = FindObjectOfType<ShotgunCharge>().hasChargedForMinimumTime;

        return buttonReleased && playerIsAboveFloor && hasChargedForMinimumTime;
    }
}
