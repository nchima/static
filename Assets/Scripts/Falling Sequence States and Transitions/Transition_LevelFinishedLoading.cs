using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_LevelFinishedLoading : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        if (Services.specialMoveManager.fireMissilesWhenFallingTrigger) {
            return true;
        }

        if (Services.levelManager.uiSequencedFinishedTrigger && Services.levelManager.loadingSequenceFinishedTrigger) {
            Services.levelManager.uiSequencedFinishedTrigger = false;
            Services.levelManager.loadingSequenceFinishedTrigger = false;
            return true;
        }

        return false;
    }
}
