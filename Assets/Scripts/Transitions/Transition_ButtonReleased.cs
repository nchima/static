using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_ButtonReleased : Transition {

    public override bool IsConditionTrue(StateController stateController) {
        if (InputManager.dashButtonUp) { return true; }
        else { return false; }
    }
}
