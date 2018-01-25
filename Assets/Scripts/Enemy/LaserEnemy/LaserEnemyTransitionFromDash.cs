using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEnemyTransitionFromDash : Transition {

    bool _isTriggerSet = false;
    public bool isTriggerSet {
        get { return _isTriggerSet; }
        set {
            _isTriggerSet = value;
            if (_isTriggerSet == true) { StartCoroutine(Reset()); }
        }
    }
    [SerializeField] float resetTime = 0.1f;


    public override bool IsConditionTrue(StateController stateController) {
        if (isTriggerSet) {
            isTriggerSet = false;
            return true;
        } else {
            return false;
        }
    }

    private IEnumerator Reset() {
        yield return new WaitForSeconds(resetTime);
        isTriggerSet = false;
        yield return null;
    }
}
