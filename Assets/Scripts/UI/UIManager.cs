using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    // SCREENS
    public GameObject seizureWarningScreen;
    public GameObject titleScreen;
    public GameObject episodeSelectScreen;
    public GameObject controlsScreen;
    public GameObject creditsScreen;
    public GameObject levelCompleteScreen;
    public GameObject pauseScreen;
    public GameObject gameOverScreen;
    public GameObject nameEntryScreen;
    public GameObject highScoreScreen;
    public GameObject endOfDemoScreen;
    public GameObject landOnLevelScreen;
    public GameObject healthWarningScreen;
    public GameObject hud;
    public GameObject crosshair;

    public ScreenShake[] screenShakesToTurnOffOnGameOver;

    public GameObject pauseVeil;

    [SerializeField] GameObject[] keyboardControlPrompts;
    [SerializeField] GameObject[] xBoxControlPrompts;

    bool titleScreenActiveBeforePause;
    bool levelCompleteScreenActiveBeforePause;
    bool healthWarningScreenActiveBeforePause;


    public void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.GameOver>(GameOverHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameStarted>(GameStartedHandler);
    }

    public void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.GameOver>(GameOverHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameStarted>(GameStartedHandler);
    }


    public void ShowTitleScreen(bool value) {
        seizureWarningScreen.SetActive(false);
        episodeSelectScreen.SetActive(false);
        titleScreen.SetActive(value);
    }


    public void ShowEpisodeSelectScreen() {
        titleScreen.SetActive(false);
        episodeSelectScreen.SetActive(true);
    }


    public void SwitchControlPrompts(InputManager.InputMode newMode) {
        if (newMode == InputManager.InputMode.MouseAndKeyboard) {
            foreach (GameObject gameObject in keyboardControlPrompts) { gameObject.SetActive(true); }
            foreach (GameObject gameObject in xBoxControlPrompts) { gameObject.SetActive(false); }
        }
        else if (newMode == InputManager.InputMode.Controller) {
            foreach (GameObject gameObject in keyboardControlPrompts) { gameObject.SetActive(false); }
            foreach (GameObject gameObject in xBoxControlPrompts) { gameObject.SetActive(true); }
        }
    }


    public void ShowPauseScreen() {
        pauseScreen.SetActive(true);
        pauseVeil.SetActive(true);
        crosshair.SetActive(false);
        hud.SetActive(false);

        titleScreenActiveBeforePause = titleScreen.activeSelf;
        levelCompleteScreenActiveBeforePause = levelCompleteScreen.activeSelf;
        healthWarningScreenActiveBeforePause = healthWarningScreen.activeSelf;

        titleScreen.SetActive(false);
        levelCompleteScreen.SetActive(false);
        healthWarningScreen.SetActive(false);
    }


    public void ShowControlsScreen(bool value) {
        titleScreen.SetActive(!value);
        controlsScreen.SetActive(value);
    }


    public void ShowCreditsScreen(bool value) {
        titleScreen.SetActive(!value);
        creditsScreen.SetActive(value);
    }


    public void HidePauseScreen() {
        pauseScreen.SetActive(false);
        pauseVeil.SetActive(false);
        crosshair.SetActive(true);
        hud.SetActive(true);

        titleScreen.SetActive(titleScreenActiveBeforePause);
        levelCompleteScreen.SetActive(levelCompleteScreenActiveBeforePause);
        healthWarningScreen.SetActive(healthWarningScreenActiveBeforePause);
    }


    public void ShowGameOverScreen() {
        hud.SetActive(false);
        gameOverScreen.SetActive(true);
        pauseVeil.SetActive(true);
        crosshair.SetActive(false);

        foreach (ScreenShake turnMeOff in screenShakesToTurnOffOnGameOver) {
            turnMeOff.shakeScale = Vector3.zero;
        }

        levelCompleteScreen.SetActive(false);
        healthWarningScreen.SetActive(false);
    }


    public void ShowHighScoreScreen() {
        titleScreen.SetActive(false);
        pauseVeil.SetActive(true);
        endOfDemoScreen.SetActive(false);
        gameOverScreen.gameObject.SetActive(false);
        nameEntryScreen.gameObject.SetActive(false);
        highScoreScreen.gameObject.SetActive(true);
        healthWarningScreen.SetActive(false);
    }


    public void ShowEndOfDemoScreen() {
        hud.SetActive(false);
        endOfDemoScreen.SetActive(true);
        levelCompleteScreen.SetActive(false);
        healthWarningScreen.SetActive(false);
    }


    public void ReduceScreenShakeForGameOverScreen() {

    }


    public void GameOverHandler(GameEvent gameEvent) {
        if (highScoreScreen.activeInHierarchy) { return; }
        ShowGameOverScreen();
    }


    public void GameStartedHandler(GameEvent gameEvent) {
        pauseVeil.SetActive(false);
        ShowTitleScreen(false);
    }
}
