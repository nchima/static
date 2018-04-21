using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour {

    // SCREENS
    public GameObject titleScreen;
    public GameObject levelCompleteScreen;
    public GameObject specialMoveReadyScreen;
    public GameObject pauseScreen;
    public GameObject gameOverScreen;
    public GameObject nameEntryScreen;
    public GameObject highScoreScreen;
    public GameObject endOfDemoScreen;
    public GameObject crosshair;

    public EventSystem eventSystem;
    public GameObject resumeButton;

    public ScreenShake[] turnTheseOffPlz;

    public GameObject pauseVeil;

    bool titleScreenActiveBeforePause;
    bool levelCompleteScreenActiveBeforePause;
    bool specialMoveReadyScreenActiveBeforePause;


    public void ShowTitleScreen(bool value) {
        titleScreen.SetActive(value);
    }


    public void ShowPauseScreen() {
        pauseScreen.SetActive(true);
        pauseVeil.SetActive(true);
        crosshair.SetActive(false);

        titleScreenActiveBeforePause = titleScreen.activeSelf;
        levelCompleteScreenActiveBeforePause = levelCompleteScreen.activeSelf;
        specialMoveReadyScreenActiveBeforePause = specialMoveReadyScreen.activeSelf;

        titleScreen.SetActive(false);
        levelCompleteScreen.SetActive(false);
        specialMoveReadyScreen.SetActive(false);

        if (InputManager.inputMode == InputManager.InputMode.Controller) {
            eventSystem.SetSelectedGameObject(resumeButton);
        }
    }


    public void HidePauseScreen() {
        pauseScreen.SetActive(false);
        pauseVeil.SetActive(false);
        crosshair.SetActive(true);

        titleScreen.SetActive(titleScreenActiveBeforePause);
        levelCompleteScreen.SetActive(levelCompleteScreenActiveBeforePause);
        specialMoveReadyScreen.SetActive(specialMoveReadyScreenActiveBeforePause);
    }


    public void ShowGameOverScreen() {
        gameOverScreen.SetActive(true);
        pauseVeil.SetActive(true);
        crosshair.SetActive(false);

        foreach (ScreenShake turnMeOff in turnTheseOffPlz) {
            turnMeOff.shakeScale = Vector3.zero;
        }

        levelCompleteScreen.SetActive(false);
        specialMoveReadyScreen.SetActive(false);
    }


    public void ShowHighScoreScreen() {
        gameOverScreen.gameObject.SetActive(false);
        nameEntryScreen.gameObject.SetActive(false);
        highScoreScreen.gameObject.SetActive(true);
    }


    public void ShowEndOfDemoScreen() {
        endOfDemoScreen.SetActive(true);
        levelCompleteScreen.SetActive(false);
        specialMoveReadyScreen.SetActive(false);
    }


    public void ReduceScreenShakeForGameOverScreen() {

    }
}
