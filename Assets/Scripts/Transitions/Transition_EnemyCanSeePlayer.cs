using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_EnemyCanSeePlayer : Transition {

    public override bool IsConditionTrue(StateController stateController) {
        Enemy enemyStateController = stateController as Enemy;
        return enemyStateController.CanSeePlayer;
    }
}
