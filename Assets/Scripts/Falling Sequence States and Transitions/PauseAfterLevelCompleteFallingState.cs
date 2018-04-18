using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseAfterLevelCompleteFallingState : State {

    public override void Initialize(StateController stateController) {
        GameManager.levelManager.SetFloorCollidersActive(false);
        GameManager.player.GetComponent<Rigidbody>().useGravity = true;
        GameManager.player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        GameManager.player.GetComponent<PlayerController>().isMovementEnabled = false;
        GameManager.player.GetComponent<PlayerController>().state = PlayerController.State.Falling;
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
    }

    public override void End(StateController stateController) {
        GameManager.player.GetComponent<PlayerController>().isMovementEnabled = true;
    }
}
