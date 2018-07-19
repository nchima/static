using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsScreen : MonoBehaviour {

    void Update() {
        if (InputManager.cancelButtonDown || InputManager.pauseButtonDown) {
            Services.uiManager.ShowCreditsScreen(false);
        }
    }
}
