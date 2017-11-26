using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_ButtonReleased : Transition {

    [SerializeField] string buttonName;

    public override bool IsConditionTrue(StateController stateController) {
        if (Input.GetButtonUp(buttonName)) { return true; }
        else { return false; }
    }
}
