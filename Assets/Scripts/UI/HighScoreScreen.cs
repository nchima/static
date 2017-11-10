using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScoreScreen : MonoBehaviour {

	private void Update()
    {
        if (Input.GetButtonDown("Start"))
        {
            GameObject.Find("Game Manager").GetComponent<GameManager>().RestartGame();
        }
    }
}
