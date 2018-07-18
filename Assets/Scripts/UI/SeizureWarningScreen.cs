using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeizureWarningScreen : MonoBehaviour {

    private void Update() {
        if (Input.GetKey(KeyCode.Escape) || InputManager.cancelButtonDown) {
            Debug.Log("quit");
            Application.Quit();
        }

        else if (InputManager.pauseButtonDown || InputManager.submitButtonDown) {
            Services.gameManager.LoadGame();
            Services.uiManager.ShowTitleScreen(true);
        }
    }
}
