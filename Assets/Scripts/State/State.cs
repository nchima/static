﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour {

    Transition[] transitions;

    void Awake() {
        transitions = GetComponents<Transition>();
    }

    public virtual void Initialize(StateController stateController) {
        for (int i = 0; i < transitions.Length; i++) {
            transitions[i].Initialize();
        }
    }

    public virtual void Run(StateController stateController) {
        CheckTransitions(stateController);
    }

    public abstract void End(StateController stateController);

    void CheckTransitions(StateController stateController) {
        if (transitions == null || transitions.Length == 0)  { return; }

        for (int i = 0; i < transitions.Length; i++) {
            if (transitions[i].IsConditionTrue(stateController)) {
                stateController.TransitionToState(transitions[i].nextState);
            }
        }
    }
}
