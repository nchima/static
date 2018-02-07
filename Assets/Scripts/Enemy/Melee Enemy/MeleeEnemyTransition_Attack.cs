using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemyTransition_Attack : Transition {

    [SerializeField] MeleeEnemyState_ChargingUp state;
    float timer = 0f;

    private void Update() {
    }

    public override bool IsConditionTrue(StateController stateController) {
        if (timer >= state.chargeUpDuration) {
            timer = 0f;
            return true;
        }

        else {
            timer += Time.deltaTime;
            return false;
        }
    }
}
