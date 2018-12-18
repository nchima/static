using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour {

    [SerializeField] Button resumeGameButton;
    [SerializeField] Button optionsButton;

    Button buttonToSelect;

    bool isUsingController;

    private void Awake() {
        buttonToSelect = resumeGameButton;
    }

    private void OnEnable() {
        isUsingController = false;
    }

    private void Update() {
        if (InputManager.cancelButtonDown || InputManager.pauseButtonDown) {
            Services.gameManager.PauseGame(false);
        }

        if (!isUsingController && InputManager.inputMode == InputManager.InputMode.Controller && buttonToSelect != null) {
            buttonToSelect.Select();
            buttonToSelect.GetComponent<ButtonTextModifier>().ForceHighlight();

            isUsingController = true;
        }
    }

    public void ResetButton() {
        Services.gameManager.PauseGame(false);

        // Reload the scene.
        Services.gameManager.RestartGame();
        //SceneManager.LoadScene ("Main");

        buttonToSelect = resumeGameButton;

        // Unlock and show cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void OptionsButtonPressed() {
        buttonToSelect = optionsButton;
        Services.uiManager.ShowOptionsScreen(true);
    }


    public void QuitButton() {
        Application.Quit();
    }


    public void ResumeButton() {
        Services.gameManager.PauseGame(false);
    }
}
