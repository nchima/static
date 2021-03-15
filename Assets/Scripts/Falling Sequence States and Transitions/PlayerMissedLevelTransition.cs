using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMissedLevelTransition : Transition {

    bool trigger = false;

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerFellOutOfLevel>(PlayerFellOutOfLevelHandler);
        GameEventManager.instance.Subscribe<GameEvents.FallingSequenceStarted>(FallingSequenceStartedHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerFellOutOfLevel>(PlayerFellOutOfLevelHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.FallingSequenceStarted>(FallingSequenceStartedHandler);
    }

    public void PlayerFellOutOfLevelHandler(GameEvent gameEvent) {
        // Make sure player is not within level boundaries
        if (!Services.playerController.IsWithinLevelBoundsOnXZAxes) {
            trigger = true;
        }
    }

    public void FallingSequenceStartedHandler(GameEvent gameEvent) {
        trigger = false;
    }

    public override bool IsConditionTrue(StateController stateController) {
        FallingSequenceManager controller = stateController as FallingSequenceManager;

        if (Services.levelManager.isLevelCompleted)
        {
            return false;
        }

        if (trigger) {
            //if (!Services.fallingSequenceManager.isPlayerFalling) {
            //    Services.fallingSequenceManager.BeginFallingInstant();
            //} else {
            //    Services.fallingSequenceManager.BeginFalling();
            //}
            //Services.fallingSequenceManager.BeginFalling();
            controller.timesMissedLevel++;
            trigger = false;
            return true;
        }

        return false;
    }
}
