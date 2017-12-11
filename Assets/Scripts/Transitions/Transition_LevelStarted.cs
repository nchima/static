using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_LevelStarted : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        if (GameManager.fallingSequenceManager.isPlayerFalling) { return false; }
        else { return true; }
    }
}
