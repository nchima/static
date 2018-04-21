using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreScreen : MonoBehaviour {

	private void Update()
    {
        if (InputManager.pauseButtonDown || InputManager.submitButtonDown) {
            GameObject.Find("Game Manager").GetComponent<GameManager>().RestartGame();
        }
    }
}
