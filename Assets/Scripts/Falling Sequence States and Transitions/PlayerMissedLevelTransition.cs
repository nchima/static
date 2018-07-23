using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMissedLevelTransition : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        FallingSequenceManager controller = stateController as FallingSequenceManager;
        if (Services.playerTransform.position.y < -1f && !controller.PlayerIsWithinLevelBoundsOnXZAxes) {
            //if (!Services.fallingSequenceManager.isPlayerFalling) {
            //    Services.fallingSequenceManager.BeginFallingInstant();
            //} else {
            //    Services.fallingSequenceManager.BeginFalling();
            //}
            //Services.fallingSequenceManager.BeginFalling();
            Debug.Log("player missed level");
            controller.timesMissedLevel++;
            return true;
        }
        else {
            return false;
        }
    }
}
