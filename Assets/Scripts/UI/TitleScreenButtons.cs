﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenButtons : MonoBehaviour {

    private void Start() {
        Services.gameManager.LoadGame();
    }

    public void StartGameButton() {
        Services.uiManager.ShowEpisodeSelectScreen();
    }

    public void ViewControlsButtonPressed() {
        Services.uiManager.ShowControlsScreen(true);
    }

    public void LeaderboardsButtonPressed() {
        Services.uiManager.ShowHighScoreScreen();
    }

    public void CreditsButtonPressed() {
        Services.uiManager.ShowCreditsScreen(true);
    }

    public void QuitButton() {
        Application.Quit();
    }
}