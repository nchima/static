using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeizureWarningScreen : MonoBehaviour {

    private void Start() {
        if (FindObjectOfType<SkipSeizureWarning>().skipSeizureWarning) {
            Services.uiManager.ShowTitleScreen(true);
        }
    }

    private void Update() {
        if (Input.GetKey(KeyCode.Escape) || InputManager.cancelButtonDown) {
            Debug.Log("quit");
            Application.Quit();
        }

        else if (InputManager.pauseButtonDown || InputManager.submitButtonDown) {
            FindObjectOfType<SkipSeizureWarning>().skipSeizureWarning = true;
            Services.uiManager.ShowTitleScreen(true);
        }
    }
}
