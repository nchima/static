using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PauseAfterLevelCompleteFallingState : State {

    public override void Initialize(StateController stateController) {
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;
        Services.uiManager.crosshair.SetActive(false);
        Services.playerGameObject.GetComponent<Rigidbody>().useGravity = true;
        Services.playerGameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        //Services.playerGameObject.GetComponent<Rigidbody>().AddForce(new Vector3(0f, 500f, 0f), ForceMode.Impulse);
        Services.playerController.isMovementEnabled = false;
        Services.playerController.state = PlayerController.State.Falling;
        fallingSequenceManager.timesMissedLevel = 0;
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
    }

    public override void End(StateController stateController) {
        base.End(stateController);
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;
    
        //fallingSequenceManager.SetUpFallingVariables();\

        // Load next level.
        //if (!Services.gameManager.dontChangeLevel && Services.levelManager.isLevelCompleted) {
            //Services.levelManager.loadingState = LevelManager.LoadingState.LoadingRandomly;
        //    Services.levelManager.StartCoroutine(Services.levelManager.LoadLevelCoroutine(Services.levelManager.currentLevelSet.NextLevel));
        //}

        Services.playerController.isMovementEnabled = true;
    }
}
