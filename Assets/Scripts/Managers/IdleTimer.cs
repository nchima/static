using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IdleTimer : MonoBehaviour {

    // USED FOR IDLE TIMER
    [SerializeField] float idleResetTime = 20f;
    float timeSinceLastInput = 0f;

    void Update () {
        // Run idle timer.
        if (Services.gameManager.gameStarted) {
            if (timeSinceLastInput >= idleResetTime) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            // Check all joystick buttons.
            bool buttonPressed = false;
            for (int i = 0; i < 20; i++) {
                if (InputManager.AnyInputPressed) {
                    buttonPressed = true;
                }
            }

            // See if any other buttons or keys have been pressed.
            if (Input.anyKeyDown || buttonPressed || Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
                timeSinceLastInput = 0f;
            }

            timeSinceLastInput += Time.deltaTime;
        }

    }
}
