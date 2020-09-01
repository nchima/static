using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootEnabler : MonoBehaviour {

    public void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.EnableFeet>(FeetEnabledHandler);
    }

    public void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.EnableFeet>(FeetEnabledHandler);
    }

    public void FeetEnabledHandler(GameEvent gameEvent) {
        GameEvents.EnableFeet feetEnabledEvent = gameEvent as GameEvents.EnableFeet;
        for (var i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(feetEnabledEvent.value);
        }
    }
}
