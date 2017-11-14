using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_MouseButtonPressed : Transition {

    [SerializeField] int mouseButton;

    public override bool IsConditionTrue(StateController stateController) {
        if (Input.GetMouseButtonDown(mouseButton)) { return true; }
        else { return false; }
    }
}
