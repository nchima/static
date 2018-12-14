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

    private Button buttonToSelect;

    private void Awake() {
        buttonToSelect = startGameButton;
        startGameButton.Select();
    }

    private void OnEnable() {
        //buttonToSelect.Select();
        StartCoroutine(SelectionCoroutine());
    }

    IEnumerator SelectionCoroutine() {
        yield return new WaitForEndOfFrame();
        buttonToSelect.Select();
        yield return null;
    }

    private void Start() {
        Services.gameManager.LoadGame();
        startGameButton.Select();
        startGameButton.GetComponent<ButtonTextModifier>().OnSelect(new UnityEngine.EventSystems.BaseEventData(UnityEngine.EventSystems.EventSystem.current));
    }

    public void StartGameButtonPressed() {
        Services.uiManager.ShowEpisodeSelectScreen();
        buttonToSelect = startGameButton;
    }

    public void ViewControlsButtonPressed() {
        Services.uiManager.ShowControlsScreen(true);
        buttonToSelect = viewControlsButton;
    }

    public void LeaderboardsButtonPressed() {
        Services.uiManager.ShowHighScoreScreen();
        buttonToSelect = leaderboardsButton;
    }

    public void OptionsButtonPressed() {
        Services.uiManager.ShowOptionsScreen(true);
        buttonToSelect = optionsButton;
    }

    public void CreditsButtonPressed() {
        Services.uiManager.ShowCreditsScreen(true);
        buttonToSelect = creditsButton;
    }

    public void QuitButton() {
        Application.Quit();
    }
}
