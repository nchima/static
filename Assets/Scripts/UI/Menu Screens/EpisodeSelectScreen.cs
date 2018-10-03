using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EpisodeSelectScreen : MonoBehaviour {

    private void Start() {
        Services.uiManager.gameMap.SetActive(true);
        Services.uiManager.gameMap.GetComponent<GameMap>().AllowMouseInput(true);
        Services.uiManager.gameMap.GetComponent<GameMap>().HighlightUnlockedPaths();
    }

    private void OnDisable() {
        Services.uiManager.gameMap.GetComponent<GameMap>().AllowMouseInput(false);
        Services.uiManager.gameMap.SetActive(false);
    }

    public void BackButton() {
        Services.uiManager.ShowTitleScreen(true);
    }
}
