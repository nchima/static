using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunChargeTransition_PlayerReleasedButton : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        bool buttonReleased = InputManager.fireButtonUp;
        bool playerIsAboveFloor = FindObjectOfType<PlayerController>().isAboveFloor;
        bool hasChargedForMinimumTime = FindObjectOfType<ShotgunCharge>().hasChargedForMinimumTime;

        return buttonReleased && playerIsAboveFloor && hasChargedForMinimumTime;
    }
}
