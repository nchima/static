using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenButtons : MonoBehaviour {

    [SerializeField] private Button startGameButton;
    [SerializeField] private Button viewControlsButton;
    [SerializeField] private Button leaderboardsButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;

    Button buttonToSelect;

    bool isUsingControllerInput = false;

    private void Awake() {
        buttonToSelect = startGameButton;
    }

    private void Start() {
        Services.gameManager.LoadGame();
    }

    private void OnEnable() {
        if (InputManager.inputMode == InputManager.InputMode.Controller) {
            StartCoroutine(SelectionCoroutine());
        }
    }

    IEnumerator SelectionCoroutine() {
        yield return new WaitForEndOfFrame();
        buttonToSelect.GetComponent<ButtonTextModifier>().ForceHighlight();
    }

    private void OnDisable() {
        isUsingControllerInput = false;
    }

    private void Update() {
        if (!isUsingControllerInput && InputManager.inputMode == InputManager.InputMode.Controller) {
            buttonToSelect.Select();
            isUsingControllerInput = true;
        }

        else if (isUsingControllerInput && InputManager.inputMode == InputManager.InputMode.MouseAndKeyboard) {
            isUsingControllerInput = false;
        }
    }

    public void StartGameButtonPressed() {
        buttonToSelect = startGameButton;
        Services.uiManager.ShowEpisodeSelectScreen();
    }

    public void ViewControlsButtonPressed() {
        buttonToSelect = viewControlsButton;
        Services.uiManager.ShowControlsScreen(true);
    }

    public void LeaderboardsButtonPressed() {
        buttonToSelect = leaderboardsButton;
        Services.uiManager.ShowHighScoreScreen();
    }

    public void OptionsButtonPressed() {
        buttonToSelect = optionsButton;
        Services.uiManager.ShowOptionsScreen(true);
    }

    public void CreditsButtonPressed() {
        buttonToSelect = creditsButton;
        Services.uiManager.ShowCreditsScreen(true);
    }

    public void QuitButton() {
        Application.Quit();
    }
}
