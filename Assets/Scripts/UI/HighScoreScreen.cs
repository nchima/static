using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreScreen : MonoBehaviour {

    float time = 1f;
    float timer = 0;

    private void Awake() {
        Services.scoreManager.RetrieveScoresForHighScoreScreen();
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= time) {
            if (InputManager.pauseButtonDown || InputManager.submitButtonDown) {
                Services.gameManager.GetComponent<GameManager>().RestartGame();
            }
        }
    }
}
