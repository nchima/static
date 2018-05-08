using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTransition : Transition {

    public bool isTriggerSet;

    public override bool IsConditionTrue(StateController stateController) {
        return isTriggerSet;
    }

    public override void Deinitialize() {
        base.Deinitialize();
        isTriggerSet = false;
    }
}
