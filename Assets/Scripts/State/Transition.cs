using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Transition : MonoBehaviour {
    public State nextState;
    public abstract bool IsConditionTrue(StateController stateController);
    public virtual void Initialize() { }
}