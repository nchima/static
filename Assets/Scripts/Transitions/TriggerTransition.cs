using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTransition : Transition {

    public bool isTriggerSet = false;

    private void LateUpdate() {
        isTriggerSet = false;
    }

    public override bool IsConditionTrue(StateController stateController) {
        if (isTriggerSet) {
            isTriggerSet = false;
            return true;
        } else {
            return false;
        }
    }
}
