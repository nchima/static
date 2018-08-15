using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatePain : State {

    public const float DURATION = 0.3f;

    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);
        StartCoroutine(Wait());
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
    }

    IEnumerator Wait() {
        yield return new WaitForSeconds(DURATION);
        GetComponent<TriggerTransition>().isTriggerSet = true;
        yield return null;
    }

    public override void End(StateController stateController) {
        base.End(stateController);
    }
}
