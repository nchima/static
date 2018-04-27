using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankEnemyTransition_PlayerIsClose : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        TankEnemy controller = stateController as TankEnemy;
        if (Vector3.Distance(controller.transform.position, Services.playerTransform.position) < controller.runAwayDistance) {
            return true;
        } else {
            return false;
        }
    }
}
