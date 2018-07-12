using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreScreen : MonoBehaviour {

    private void Update()
    {
        if (InputManager.pauseButtonDown || InputManager.submitButtonDown) {
            Services.gameManager.GetComponent<GameManager>().RestartGame();
        }
    }
}
