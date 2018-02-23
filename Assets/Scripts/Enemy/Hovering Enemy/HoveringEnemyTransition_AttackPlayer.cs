using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoveringEnemyTransition_AttackPlayer : Transition {

    [SerializeField] float attackRange;

    public override bool IsConditionTrue(StateController stateController) {
        if (Vector3.Distance(stateController.transform.position, GameManager.player.transform.position) <= attackRange) {
            return true;
        } else {
            return false;
        }
    }
}
