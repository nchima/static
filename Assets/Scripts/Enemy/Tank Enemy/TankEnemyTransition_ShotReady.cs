using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankEnemyTransition_ShotReady : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        TankEnemy controller = stateController as TankEnemy;
        if (controller.shotTrigger) {
            return true;
        }
        
        else {
            return false;
        }
    }
}
