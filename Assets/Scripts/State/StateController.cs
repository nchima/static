using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour {

    public State currentState;
    State initialState;

    private void Awake() {
        initialState = currentState;
    }

    private void Start() {
        currentState.Initialize(this);
    }

    protected virtual void Update() {
        currentState.Run(this);
    }

    protected virtual void FixedUpdate() {
        currentState.FixedRun(this);
    }

    public void TransitionToState(State nextState) {
        if (nextState == null) { return; }
        currentState.End(this);
        nextState.Initialize(this);
        currentState = nextState;
    }

    protected void ReturnToInitialState() {
        TransitionToState(initialState);
    }

    public State GetCurrentState() {
        return currentState;
    }
}
