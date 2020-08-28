using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FiringShockwaveState : State {

    float sloMoDuration = 0f;

    public override void Initialize(StateController stateController) {
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;

        Services.uiManager.ShowLevelCompleteScreen(false);
        Services.uiManager.ShowNowEnteringScreen(false);
        Services.gameManager.CountEnemies();

        // Set up bonus time for next level.
        Services.scoreManager.DetermineBonusTime();

        foreach(Camera cam in FindObjectOfType<FieldOfViewController>().orthographicCams) {
            cam.enabled = true;
        }

        Services.playerTransform.position = new Vector3(Services.playerTransform.position.x, 2.15f, Services.playerTransform.position.z);

        // Begin rotating camera back to regular position.
        Services.playerTransform.Find("Cameras").transform.DOLocalRotate(new Vector3(0f, 0f, 0f), fallingSequenceManager.lookUpSpeed * 0.6f, RotateMode.Fast);

        // Reset movement variables.
        Services.playerTransform.position = new Vector3(Services.playerTransform.position.x, 2.8f, Services.playerTransform.position.z);
        Services.playerGameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Services.playerGameObject.GetComponent<Rigidbody>().useGravity = false;
        Services.playerController.state = PlayerController.State.Normal;
        Services.playerController.maxAirSpeed = fallingSequenceManager.savedRegularMoveSpeed;

        RenderSettings.fogColor = Color.black;

        fallingSequenceManager.fallingSequenceTimer = 0f;

        fallingSequenceManager.timesMissedLevel = 0;
        Services.uiManager.landOnLevelScreen.SetActive(false);

        sloMoDuration = GunValueManager.MapTo(0f, 0.25f);
        GetComponent<WaitForSecondsTransition>().duration = sloMoDuration;
        fallingSequenceManager.InstantiateShockwave(fallingSequenceManager.shockwavePrefab, Services.gun.burstsPerSecondSloMoModifierMax, sloMoDuration);
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
    }

    public override void End(StateController stateController) {
        base.End(stateController);
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;

        Debug.Log("slomodur: " + sloMoDuration);
        Services.timeScaleManager.ReturnToFullSpeed(sloMoDuration);
        Services.musicManager.ExitFallingSequence();
        Services.musicManager.RandomizeAllMusicVolumeLevels();

        Services.gun.canShoot = true;
        Services.specialMoveManager.canShoot = true;

        // Allow enemies to start attacking.
        Services.levelManager.SetEnemiesAIActive(true);

        // Destroy any obstacles that the player is touching.
        Collider[] overlappingSolids = Physics.OverlapCapsule(
            Services.playerTransform.position,
            Services.playerTransform.position + Vector3.down * 10f,
            Services.playerGameObject.GetComponent<CapsuleCollider>().radius,
            1 << 8);

        for (int i = 0; i < overlappingSolids.Length; i++) {
            if (overlappingSolids[i].tag == "Obstacle") {
                overlappingSolids[i].GetComponent<Obstacle>().DestroyByPlayerFalling();
            }
        }

        // Begin moving obstacles to their full height.
        //GameObject.Find("Obstacles").transform.DOMoveY(0f, 0.18f, false);

        Physics.gravity = fallingSequenceManager.savedGravity;

        fallingSequenceManager.isSpeedFallActive = false;

        //Services.healthManager.forceInvincibility = false;

        Services.playerGameObject.GetComponent<Collider>().material.bounciness = fallingSequenceManager.normalPlayerBounciness;

        // Reset player fall through floor cooldown
        Services.playerController.fallThroughFloorTimer = 0f;
    }
}
