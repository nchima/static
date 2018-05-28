using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour {

    //* THIS SCRIPT WILL EVENTUALLY CONTAIN CODE FOR A THEORETICAL PAUSE MENU - RIGHT NOW IT JUST RESETS THE SCENE WHEN THE PLAYER PRESSES ESCAPE *//


    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            ResetButton();
        }
    }


    public void ResetButton()
    {
        Services.gameManager.PauseGame(false);

        // Reload the scene.
		SceneManager.LoadScene ("Main");

        // Unlock and show cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void QuitButton() {
        Application.Quit();
    }


    public void ResumeButton() {
        Services.gameManager.PauseGame(false);
    }
}
