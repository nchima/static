using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyTransition_Deflected : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        MeleeEnemy controller = stateController as MeleeEnemy;
        if (controller.hitByPlayerMissileTrigger) {
            controller.hitByPlayerMissileTrigger = false;
            Debug.Log("Melee enemy attack deflected.");
            return true;
        } else {
            return false;
        }
    }
}
