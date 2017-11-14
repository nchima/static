using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTouchedDownTransition : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        if (Physics.Raycast(GameManager.player.transform.position, Vector3.down, 7f)) { return true; } 
        else { return false; }
    }
}
