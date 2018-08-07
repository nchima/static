using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsScreen : MonoBehaviour {

    private void Update() {
        if (InputManager.cancelButtonDown || InputManager.pauseButtonDown) {
            Services.uiManager.ShowControlsScreen(false);
        }
    }
}
