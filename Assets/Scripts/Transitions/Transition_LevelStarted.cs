using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_LevelStarted : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        Enemy enemy = stateController as Enemy;
        if (!Services.gameManager.gameStarted || enemy.isAIPaused) { return false; }
        else { return true; }
    }
}
