using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTouchedDownTransition : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        FallingSequenceManager controller = stateController as FallingSequenceManager;
        //bool playerGrounded = Physics.Raycast(Services.playerTransform.position, Vector3.down, 7f);
        bool playerGrounded = Services.playerController.isAboveFloor;
        bool playerBelowFloor = Services.playerTransform.position.y <= 0f;
        if (playerGrounded || (playerBelowFloor && Services.playerController.IsWithinLevelBoundsOnXZAxes)) { 
            return true; 
        } 
        else { 
            return false; 
        }
    }
}
