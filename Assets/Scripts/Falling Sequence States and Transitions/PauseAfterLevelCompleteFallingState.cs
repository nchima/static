using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseAfterLevelCompleteFallingState : State {

    public override void Initialize(StateController stateController) {
        GameManager.instance.SetFloorCollidersActive(false);
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
    }

    public override void End(StateController stateController) {}
}
