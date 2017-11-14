using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FallIntoLevelState : State {

    public override void Initialize(StateController stateController) {

        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;

        GameManager.instance.ReturnToFullSpeed();

        if (GameManager.player.GetComponent<PlayerController>().state != PlayerController.State.SpeedFalling) {
            GameManager.player.GetComponent<PlayerController>().state = PlayerController.State.Falling;
        }

        GameManager.player.GetComponent<Collider>().material.bounciness = 0f;
        GameManager.instance.forceInvincibility = false;
        Physics.gravity = fallingSequenceManager.savedGravity;

        // Load next level.
        if (!GameManager.instance.dontChangeLevel && GameManager.levelManager.isLevelCompleted /* && !startMidFall*/) {
            GameManager.instance.LoadNextLevel();
        }

        // Place the player in the correct spot above the level.
        GameManager.player.transform.position = new Vector3(
            GameManager.player.transform.position.x,
            fallingSequenceManager.playerSpawnPoint.position.y,
            GameManager.player.transform.position.z);

        // Re-enable the floor's collision (since it is disabled when the player completes a level.)
        GameManager.instance.SetFloorCollidersActive(true);

        // Update billboards.
        GameManager.instance.GetComponent<BatchBillboard>().FindAllBillboards();

        // Set up variables for falling.
        fallingSequenceManager.playerTouchedDown = false;
        fallingSequenceManager.savedRegularMoveSpeed = GameManager.player.GetComponent<PlayerController>().maxAirSpeed;
        GameManager.player.GetComponent<PlayerController>().maxAirSpeed = fallingSequenceManager.playerMoveSpeedWhenFalling;

        // Turn off fog.
        fallingSequenceManager.savedFogColor = RenderSettings.fogColor;
        RenderSettings.fogColor = Color.white;

        // Begin rotating player camera to face down.
        GameObject.Find("Cameras").transform.DOLocalRotate(new Vector3(90f, 0f, 0f), 0.75f, RotateMode.Fast);

        // Begin falling sequence.
        GameManager.instance.FreezeSpecialBarDecay(true);
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;

        if (GameManager.player.transform.position.y <= 600f) {
            GameManager.instance.scoreManager.HideLevelCompleteScreen();
        }
    }

    public override void End(StateController stateController) {
    }
}
