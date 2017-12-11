using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_WaitForSecondsRange : Transition {

    [SerializeField] FloatRange durationRange;
    float currentDuration;
    float elapsedTime;

    public override void Initialize() {
        base.Initialize();
        currentDuration = durationRange.Random;
        elapsedTime = 0;
    }

    public override bool IsConditionTrue(StateController stateController) {
        elapsedTime += Time.deltaTime;
        
        if (elapsedTime >= currentDuration) {
            elapsedTime = 0;
            return true;
        } 
        else { return false; }
    }
}
