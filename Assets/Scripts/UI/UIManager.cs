using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    // SCREENS
    public GameObject seizureWarningScreen;
    public GameObject titleScreen;
    public GameObject episodeSelectScreen;
    public GameObject gameMap;
    public GameObject controlsScreen;
    public GameObject optionsScreen;
    public GameObject creditsScreen;
    public GameObject levelCompleteScreen;
    public GameObject nowEnteringScreen;
    public GameObject episodeCompleteScreen;
    public GameObject selectPathScreen;
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
    bool episodeCompleteScreenActiveBeforePause;
    bool pathSelectScreenActiveBeforePause;
    bool crosshairActiveBeforePause;
    bool nowEnteringScreenActiveBeforePause;

    public void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.GameOver>(GameOverHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameStarted>(GameStartedHandler);
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.LevelLoaded>(LevelLoadedHandler);
    }

    public void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.GameOver>(GameOverHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameStarted>(GameStartedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.LevelLoaded>(LevelLoadedHandler);
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
    
    public void ShowOptionsScreen(bool value) {
        if (Services.gameManager.isGameStarted) { pauseScreen.SetActive(!value); } 
        else { titleScreen.SetActive(!value); }
        optionsScreen.SetActive(value);
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

    public void ShowControlsScreen(bool value) {
        titleScreen.SetActive(!value);
        controlsScreen.SetActive(value);
    }

    public void ShowCreditsScreen(bool value) {
        titleScreen.SetActive(!value);
        creditsScreen.SetActive(value);
    }

    public void ShowPauseScreen() {
        pauseScreen.SetActive(true);
        pauseVeil.SetActive(true);

        titleScreenActiveBeforePause = titleScreen.activeSelf;
        levelCompleteScreenActiveBeforePause = levelCompleteScreen.activeSelf;
        healthWarningScreenActiveBeforePause = healthWarningScreen.activeSelf;
        episodeCompleteScreenActiveBeforePause = episodeCompleteScreen.activeSelf;
        pathSelectScreenActiveBeforePause = selectPathScreen.activeSelf;
        crosshairActiveBeforePause = crosshair.activeSelf;
        nowEnteringScreenActiveBeforePause = nowEnteringScreen.activeSelf;

        crosshair.SetActive(false);
        hud.SetActive(false);
        titleScreen.SetActive(false);
        levelCompleteScreen.SetActive(false);
        healthWarningScreen.SetActive(false);
        episodeCompleteScreen.SetActive(false);
        selectPathScreen.SetActive(false);
        nowEnteringScreen.SetActive(false);

        Services.comboManager.PauseAllFinishers(true);
    }

    public void HidePauseScreen() {
        pauseScreen.SetActive(false);
        pauseVeil.SetActive(false);
        hud.SetActive(true);

        Services.comboManager.PauseAllFinishers(false);

        titleScreen.SetActive(titleScreenActiveBeforePause);
        levelCompleteScreen.SetActive(levelCompleteScreenActiveBeforePause);
        healthWarningScreen.SetActive(healthWarningScreenActiveBeforePause);
        episodeCompleteScreen.SetActive(episodeCompleteScreenActiveBeforePause);
        selectPathScreen.SetActive(pathSelectScreenActiveBeforePause);
        crosshair.SetActive(crosshairActiveBeforePause);
        nowEnteringScreen.SetActive(nowEnteringScreenActiveBeforePause);
    }

    public void ShowLevelCompleteScreen(bool value) {
        levelCompleteScreen.SetActive(value);
        Services.specialBarManager.ShowDisplay(!value);
        Services.comboManager.SetAllFinishersVisible(!value);
    }

    public void ShowEpisodeCompleteScreen(bool value) {
        pauseVeil.SetActive(value);
        episodeCompleteScreen.SetActive(value);
        Services.specialBarManager.ShowDisplay(!value);
        Services.comboManager.SetAllFinishersVisible(!value);
    }

    public void HideCompleteScreens() {
        ShowLevelCompleteScreen(false);
        ShowEpisodeCompleteScreen(false);
    }

    public void ShowNowEnteringScreen(bool value) {
        nowEnteringScreen.SetActive(value);
        // GameEventManager.instance.FireEvent(new GameEvents.NowEnteringScreenActivated(value));
        if (value == true) {
            nowEnteringScreen.GetComponent<NowEnteringScreen>().UpdateText();
        }
    }

    public void ShowGameOverScreen() {
        hud.SetActive(false);
        gameOverScreen.SetActive(true);
        pauseVeil.SetActive(true);
        crosshair.SetActive(false);

        Services.comboManager.PauseAllFinishers(true);

        foreach (ScreenShake turnMeOff in screenShakesToTurnOffOnGameOver) {
            turnMeOff.shakeScale = Vector3.zero;
        }

        levelCompleteScreen.SetActive(false);
        healthWarningScreen.SetActive(false);
        gameMap.SetActive(false);
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
        gameMap.SetActive(false);

        Services.comboManager.PauseAllFinishers(true);
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

    public void LevelCompletedHandler(GameEvent gameEvent) {
        ShowNowEnteringScreen(false);
        ShowLevelCompleteScreen(true);
    }

    public void LevelLoadedHandler(GameEvent gameEvent) {
        ShowLevelCompleteScreen(false);
        ShowNowEnteringScreen(true);
    }

    public void SetHealthWarningTemporaryVisibility(bool value) {
        // Get all ui texts of health warning screen.
        Text[] texts = healthWarningScreen.transform.GetChild(0).GetComponentsInChildren<Text>();
        foreach (Text text in texts) {
            if (value == true) { text.color = Color.white; }
            else { text.color = new Color(0, 0, 0, 0); }
        }
    }
}
