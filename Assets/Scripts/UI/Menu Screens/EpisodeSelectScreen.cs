using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EpisodeSelectScreen : MonoBehaviour {

    private void OnEnable() {
        Services.uiManager.gameMap.SetActive(true);
        Services.uiManager.gameMap.GetComponent<GameMap>().AllowSelection(true);
        Services.uiManager.gameMap.GetComponent<GameMap>().HighlightUnlockedPaths();
    }

    private void OnDisable() {
        Services.uiManager.gameMap.GetComponent<GameMap>().AllowSelection(false);
        Services.uiManager.gameMap.SetActive(false);
    }

    private void Update() {
        if (InputManager.cancelButtonDown || InputManager.pauseButtonDown) {
            BackButton();
        }
    }

    public void BackButton() {
        Services.uiManager.ShowTitleScreen(true);
    }
}
