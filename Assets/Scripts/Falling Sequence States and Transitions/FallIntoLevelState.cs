using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

using UnityEngine.SceneManagement;

public class FallIntoLevelState : State {

    public override void Initialize(StateController stateController) {
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;

        // In case the game is currently running in slow motion, return to full speed.
        Services.gameManager.ReturnToFullSpeed();

        // If the player is not currently set to falling state, set them to that state.
        if (Services.playerController.state != PlayerController.State.SpeedFalling) {
            Services.playerController.state = PlayerController.State.Falling;
        }

        Services.playerGameObject.GetComponent<Collider>().material.bounciness = 0f;
        Services.healthManager.forceInvincibility = false;

        // Reset gravity to starting value.
        //Physics.gravity = fallingSequenceManager.savedGravity;

        // Load next level.
        if (!Services.gameManager.dontChangeLevel && Services.levelManager.isLevelCompleted) {
            //Services.levelManager.loadingState = LevelManager.LoadingState.LoadingRandomly;
            Services.levelManager.LoadNextLevel();
        }

        // Drain color palette.
        //Services.colorPaletteManager.LoadFallingSequencePalette();

        // Place the player in the correct spot above the level.
        Services.playerTransform.position = fallingSequenceManager.playerSpawnPoint.position;

        // Set up variables for falling.
        fallingSequenceManager.playerTouchedDown = false;
        fallingSequenceManager.savedRegularMoveSpeed = Services.playerController.maxAirSpeed;
        //Services.playerController.maxAirSpeed = fallingSequenceManager.playerMoveSpeedWhenFalling;

        // Turn off fog.
        fallingSequenceManager.savedFogColor = RenderSettings.fogColor;
        RenderSettings.fogColor = Color.white;

        // Begin rotating player camera to face down.
        GameObject.Find("Cameras").transform.DOLocalRotate(new Vector3(90f, 0f, 0f), 0.75f, RotateMode.Fast);

        // Begin falling sequence.
        //Services.gameManager.FreezeSpecialBarDecay(true);
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;

        if (Services.playerTransform.position.y <= 600f) {
            Services.scoreManager.HideLevelCompleteScreen();
        }
    }

    public override void End(StateController stateController) {
        base.End(stateController);
        Services.levelManager.LockInLevel();
    }
}
