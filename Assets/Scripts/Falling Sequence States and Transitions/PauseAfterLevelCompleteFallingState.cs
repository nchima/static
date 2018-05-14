using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PauseAfterLevelCompleteFallingState : State {

    public override void Initialize(StateController stateController) {
        Services.levelManager.SetFloorCollidersActive(false);
        Services.playerGameObject.GetComponent<Rigidbody>().useGravity = true;
        Services.playerGameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        //Services.playerGameObject.GetComponent<Rigidbody>().AddForce(new Vector3(0f, 500f, 0f), ForceMode.Impulse);
        Services.playerController.isMovementEnabled = false;
        Services.playerController.state = PlayerController.State.Falling;
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
    }

    public override void End(StateController stateController) {
        base.End(stateController);
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;
        fallingSequenceManager.SetUpFallingVariables();
        Services.playerController.isMovementEnabled = true;
    }
}
