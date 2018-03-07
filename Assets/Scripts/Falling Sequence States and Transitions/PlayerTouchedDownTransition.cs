using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTouchedDownTransition : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        //bool playerGrounded = Physics.Raycast(GameManager.player.transform.position, Vector3.down, 7f);
        bool playerGrounded = GameManager.player.GetComponent<PlayerController>().isAboveFloor;
        if (playerGrounded) { return true; } 
        else { return false; }
    }
}
