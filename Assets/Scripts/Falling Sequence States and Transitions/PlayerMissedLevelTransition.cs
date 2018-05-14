using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMissedLevelTransition : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        if (Services.playerTransform.position.y < -5f) {

            //if (!Services.fallingSequenceManager.isPlayerFalling) {
            //    Services.fallingSequenceManager.BeginFallingInstant();
            //} else {
            //    Services.fallingSequenceManager.BeginFalling();
            //}
            //Services.fallingSequenceManager.BeginFalling();

            return true;
        }
        else {
            return false;
        }
    }
}
