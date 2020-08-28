using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForSecondsTransition : Transition {

    [SerializeField] public float duration = 0f;
    float elapsed = 0f;

    public override void Initialize() {
        base.Initialize();
        elapsed = 0f;
    }

    public override bool IsConditionTrue(StateController stateController) {
        elapsed += Time.deltaTime;
        if (elapsed >= duration) {
            elapsed = 0;
            return true;
        }
        else { return false; }
    }
}
