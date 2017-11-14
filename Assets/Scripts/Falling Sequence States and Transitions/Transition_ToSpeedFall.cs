using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_ToSpeedFall : Transition_MouseButtonPressed {

    public override bool IsConditionTrue(StateController stateController) {
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;
        if (fallingSequenceManager.isSpeedFallActive) { return false; }

        return base.IsConditionTrue(stateController);
    }
}
