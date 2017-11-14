using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_SpeedFalling : State {

    public override void Initialize(StateController stateController) {
        FallingSequenceManager fallingSequenceManager = stateController as FallingSequenceManager;
        fallingSequenceManager.ActivateSpeedFall();
    }

    public override void End(StateController stateController) {
        
    }
}
