using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTransition_ReachedNavMeshDestination : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        Enemy controller = stateController as Enemy;
        if (Vector3.Distance(controller.transform.position, controller.m_NavMeshAgent.destination) < 5f) {
            return true;
        } else {
            return false;
        }
    }
}
