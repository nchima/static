using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour {

    Transition[] transitions;

    void Awake() {
        transitions = GetComponents<Transition>();
    }

    public abstract void Initialize(StateController stateController);

    public virtual void Run(StateController stateController) {
        CheckTransitions(stateController);
    }

    public abstract void End(StateController stateController);

    void CheckTransitions(StateController stateController) {
        for (int i = 0; i < transitions.Length; i++) {
            Debug.Log(stateController.name + " is making " + this + " check transition: " + transitions[i]);
            if (transitions[i].IsConditionTrue(stateController)) {
                Debug.Log(transitions[i] + " was found to be true.");
                stateController.TransitionToState(transitions[i].nextState);
            }
        }
    }
}
