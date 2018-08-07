using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour {

    private void Update() {
        if (InputManager.cancelButtonDown || InputManager.pauseButtonDown) {
            Services.gameManager.PauseGame(false);
        }
    }

    public void ResetButton() {
        Services.gameManager.PauseGame(false);

        // Reload the scene.
        Services.gameManager.RestartGame();
        //SceneManager.LoadScene ("Main");

        // Unlock and show cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void OptionsButtonPressed() {
        Services.uiManager.ShowOptionsScreen(true);
    }


    public void QuitButton() {
        Application.Quit();
    }


    public void ResumeButton() {
        Services.gameManager.PauseGame(false);
    }
}
