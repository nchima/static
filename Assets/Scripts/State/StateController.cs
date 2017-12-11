using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour {

    public State currentState;

    private void Start() {
        currentState.Initialize(this);
    }

    protected virtual void Update() {
        currentState.Run(this);
    }

    public void TransitionToState(State nextState) {
        if (nextState == null) { return; }

        currentState.End(this);
        nextState.Initialize(this);
        currentState = nextState;
    }
}
