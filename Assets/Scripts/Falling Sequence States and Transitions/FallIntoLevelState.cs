using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

using UnityEngine.SceneManagement;

public class FallIntoLevelState : State {

    public bool speedFallTrigger = false;

    public override void Initialize(StateController stateController) {
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;
        Services.playerTransform.position = fallingSequenceManager.playerSpawnPoint.position;
        Services.playerGameObject.GetComponent<Rigidbody>().AddForce(Vector3.down * 100f, ForceMode.VelocityChange);
        Services.levelManager.SetFloorCollidersActive(true);
        fallingSequenceManager.SetUpFallingVariables();
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;

        if (InputManager.fireButtonDown && !fallingSequenceManager.isSpeedFallActive || speedFallTrigger) {
            BeginSpeedFall(stateController);
            speedFallTrigger = false;
        }

        if (Services.playerTransform.position.y <= 600f) {
            Services.scoreManager.HideLevelCompleteScreen();
        }
    }

    public override void End(StateController stateController) {
        base.End(stateController);
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;
        Services.levelManager.LockInLevel();
    }

    private void BeginSpeedFall(StateController stateController) {
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;

        Services.playerGameObject.GetComponent<Rigidbody>().AddForce(Vector3.down * 600f, ForceMode.VelocityChange);

        Services.playerController.state = PlayerController.State.SpeedFalling;
        fallingSequenceManager.isSpeedFallActive = true;
        Services.healthManager.forceInvincibility = true;
        Services.musicManager.EnterSpeedFall();
    }
}
