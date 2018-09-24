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
        Services.fieldOfViewController.SetClearVeilActive(true);
        Services.fieldOfViewController.ActivateCameraClearing(true);
        Services.colorPaletteManager.LoadFallingSequencePalette();
        fallingSequenceManager.SetUpFallingVariables();
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;

        if (InputManager.fireButtonDown && !fallingSequenceManager.isSpeedFallActive || speedFallTrigger) {
            BeginSpeedFall(stateController);
            speedFallTrigger = false;
        }

        if (Services.levelManager.isLevelCompleted) {
            if (Services.playerTransform.position.y <= 1450f && (Services.uiManager.levelCompleteScreen.activeInHierarchy || Services.uiManager.nowEnteringScreen.activeInHierarchy)) {
                Services.uiManager.HideCompleteScreens();
            }

            if (Services.playerTransform.position.y <= 1400f && !Services.uiManager.nowEnteringScreen.activeInHierarchy) {
                Services.uiManager.ShowNowEnteringScreen(true);
            }

            if (Services.playerTransform.position.y <= 900f && Services.uiManager.nowEnteringScreen.activeInHierarchy) {
                Services.uiManager.ShowNowEnteringScreen(false);
            }
        }

        // Move the player off of any obstacles they might land on.
        RaycastHit hit;
        if (Physics.SphereCast(Services.playerTransform.position, Services.playerTransform.GetComponent<CapsuleCollider>().radius, Vector3.down, out hit, 10f)) {
            // If the player landed on a bad thing, move them into the level.
            if (hit.collider.tag == "Wall" || hit.collider.tag == "Railing" || hit.collider.tag == "Obstacle") {
                Vector3 moveDirection = -hit.collider.transform.forward;
                Services.playerTransform.position += moveDirection * Services.playerTransform.GetComponent<CapsuleCollider>().radius * 2f;
            }
        }

    }

    public override void End(StateController stateController) {
        base.End(stateController);
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;
        Services.levelManager.LockInLevel();
        Services.fieldOfViewController.SetClearVeilActive(false);
        Services.fieldOfViewController.ActivateCameraClearing(false);
        Services.colorPaletteManager.LoadLevelPalette();
        Services.levelManager.isLevelCompleted = false;
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
