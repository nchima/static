using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_MouseButtonPressed : Transition {

    [SerializeField] int mouseButton;

    public override bool IsConditionTrue(StateController stateController) {
        if (InputManager.fireButtonDown) { return true; }
        else { return false; }
    }
}
