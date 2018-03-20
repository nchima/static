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
        GameManager.instance.ReturnToFullSpeed();

        // If the player is not currently set to falling state, set them to that state.
        if (GameManager.player.GetComponent<PlayerController>().state != PlayerController.State.SpeedFalling) {
            GameManager.player.GetComponent<PlayerController>().state = PlayerController.State.Falling;
        }

        GameManager.player.GetComponent<Collider>().material.bounciness = 0f;
        GameManager.healthManager.forceInvincibility = false;

        // Reset gravity to starting value.
        Physics.gravity = fallingSequenceManager.savedGravity;

        // Load next level.
        if (!GameManager.instance.dontChangeLevel && GameManager.levelManager.isLevelCompleted) {
            GameManager.instance.LoadNextLevel();
        }

        // Drain color palette.
        //GameManager.colorPaletteManager.LoadFallingSequencePalette();

        // Place the player in the correct spot above the level.
        GameManager.player.transform.position = fallingSequenceManager.playerSpawnPoint.position;

        // Re-enable the floor's collision (since it is disabled when the player completes a level.)
        GameManager.instance.SetFloorCollidersActive(true);

        // Update billboards.
        GameManager.instance.GetComponent<BatchBillboard>().FindAllBillboards();

        // Set up variables for falling.
        fallingSequenceManager.playerTouchedDown = false;
        fallingSequenceManager.savedRegularMoveSpeed = GameManager.player.GetComponent<PlayerController>().maxAirSpeed;
        //GameManager.player.GetComponent<PlayerController>().maxAirSpeed = fallingSequenceManager.playerMoveSpeedWhenFalling;

        // Turn off fog.
        fallingSequenceManager.savedFogColor = RenderSettings.fogColor;
        RenderSettings.fogColor = Color.white;

        // Begin rotating player camera to face down.
        GameObject.Find("Cameras").transform.DOLocalRotate(new Vector3(90f, 0f, 0f), 0.75f, RotateMode.Fast);

        // Begin falling sequence.
        //GameManager.instance.FreezeSpecialBarDecay(true);
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;

        if (GameManager.player.transform.position.y <= 600f) {
            GameManager.scoreManager.HideLevelCompleteScreen();
        }
    }

    public override void End(StateController stateController) {
    }
}
