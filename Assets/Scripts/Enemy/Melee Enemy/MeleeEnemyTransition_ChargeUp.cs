using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyTransition_ChargeUp : Transition {

    [SerializeField] float attackRange;

    // Return true if we're close enough to the player to attack.
    public override bool IsConditionTrue(StateController stateController) {
        if (Vector3.Distance(GameManager.player.transform.position, stateController.transform.position) <= attackRange) {
            return true;
        } else {
            return false;
        }
    }
}
