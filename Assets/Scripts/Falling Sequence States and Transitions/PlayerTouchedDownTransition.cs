using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTouchedDownTransition : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        //bool playerGrounded = Physics.Raycast(Services.playerTransform.position, Vector3.down, 7f);
        bool playerGrounded = Services.playerController.isAboveFloor;
        if (playerGrounded) { return true; } 
        else { return false; }
    }
}
