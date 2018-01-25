﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FiringShockwaveState : State {

    public override void Initialize(StateController stateController) {
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;

        GameManager.instance.scoreManager.HideLevelCompleteScreen();
        GameManager.instance.CountEnemies();

        // Set up bonus time for next level.
        GameManager.instance.DetermineBonusTime();

        //GameManager.specialBarManager.freezeDecay = false;

        GameManager.player.transform.position = new Vector3(GameManager.player.transform.position.x, 2.15f, GameManager.player.transform.position.z);

        // Begin rotating camera back to regular position.
        GameManager.player.transform.Find("Cameras").transform.DOLocalRotate(new Vector3(0f, 0f, 0f), fallingSequenceManager.lookUpSpeed * 0.6f, RotateMode.Fast);

        // Reset movement variables.
        GameManager.player.transform.position = new Vector3(GameManager.player.transform.position.x, 2.8f, GameManager.player.transform.position.z);
        GameManager.player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        GameManager.player.GetComponent<Rigidbody>().useGravity = false;
        GameManager.player.GetComponent<PlayerController>().state = PlayerController.State.Normal;
        GameManager.player.GetComponent<PlayerController>().maxAirSpeed = fallingSequenceManager.savedRegularMoveSpeed;

        RenderSettings.fogColor = Color.black;

        fallingSequenceManager.fallingSequenceTimer = 0f;

        if (fallingSequenceManager.isSpeedFallActive) fallingSequenceManager.InstantiateShockwave(fallingSequenceManager.shockwavePrefab, GameManager.instance.gun.burstsPerSecondSloMoModifierMax);
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
    }

    public override void End(StateController stateController) {
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;

        GameManager.instance.ReturnToFullSpeed();
        GameManager.musicManager.ExitFallingSequence();
        GameManager.musicManager.RandomizeAllMusicVolumeLevels();

        GameManager.instance.gun.canShoot = true;

        // Allow enemies to start attacking.
        GameManager.levelManager.SetEnemiesActive(true);

        // Destroy any obstacles that the player is touching.
        Collider[] overlappingSolids = Physics.OverlapCapsule(
            GameManager.player.transform.position,
            GameManager.player.transform.position + Vector3.down * 10f,
            GameManager.player.GetComponent<CapsuleCollider>().radius,
            1 << 8);

        for (int i = 0; i < overlappingSolids.Length; i++) {
            if (overlappingSolids[i].tag == "Obstacle") {
                overlappingSolids[i].GetComponent<Obstacle>().DestroyByPlayerFalling();
            }
        }

        // Begin moving obstacles to their full height.
        GameObject.Find("Obstacles").transform.DOMoveY(0f, 0.18f, false);

        GameManager.colorPaletteManager.RestoreSavedPalette();
        //GameManager.colorPaletteManager.ChangeToRandomPalette(0.1f);

        Physics.gravity = fallingSequenceManager.savedGravity;

        fallingSequenceManager.isSpeedFallActive = false;

        GameManager.healthManager.forceInvincibility = false;

        GameManager.player.GetComponent<Collider>().material.bounciness = fallingSequenceManager.normalPlayerBounciness;
    }
}
