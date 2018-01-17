using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEnemyTransitionFromDash : Transition {

    public override bool IsConditionTrue(StateController stateController) {
        LaserEnemy controller = stateController as LaserEnemy;
        
        if (controller.timesDashed >= controller.timesToDash) {
            return true;
        }

        else {
            return false;
        }
    }
}
