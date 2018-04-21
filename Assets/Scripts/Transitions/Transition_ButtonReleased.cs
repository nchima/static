using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_ButtonReleased : Transition {

    [SerializeField] string buttonName;

    public override bool IsConditionTrue(StateController stateController) {
        if (InputManager.specialMoveButtonUp) { return true; }
        else { return false; }
    }
}
