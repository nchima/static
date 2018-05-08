using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEnemyTransitionFromDash : Transition {

    public bool isTriggerSet;
    [SerializeField] float resetTime = 0.1f;


    public override bool IsConditionTrue(StateController stateController) {
        return isTriggerSet;
    }

    private IEnumerator Reset() {
        yield return new WaitForSeconds(resetTime);
        isTriggerSet = false;
        yield return null;
    }

    public override void Deinitialize() {
        base.Deinitialize();
        isTriggerSet = false;
    }
}
