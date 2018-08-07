using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreScreen : MonoBehaviour {

    float time = 1f;
    float timer = 0;

    private void Start() {
        Services.scoreManager.RetrieveScoresForHighScoreScreen();
    }

    private void Update()
    {
        if (!Services.gameManager.isGameStarted) {
            if (InputManager.pauseButtonDown || InputManager.submitButtonDown) {
                Services.uiManager.highScoreScreen.SetActive(false);
                Services.uiManager.titleScreen.SetActive(true);
            }
        } else {
            timer += Time.unscaledDeltaTime;
            if (timer >= time) {
                if (InputManager.pauseButtonDown || InputManager.submitButtonDown) {
                    Services.gameManager.GetComponent<GameManager>().RestartGame();
                }
            }
        }
    }
}
