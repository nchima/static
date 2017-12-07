using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunChargeTransition_PlayerPressedButton : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        bool buttonPressed = Input.GetButton("Fire1") || Input.GetButton("Fire2");
        bool playerIsAboveFloor = FindObjectOfType<PlayerController>().isAboveFloor;
        bool hasChargedForMinimumTime = FindObjectOfType<ShotgunCharge>().hasChargedForMinimumTime;

        return buttonPressed && playerIsAboveFloor && hasChargedForMinimumTime;
    }
}
