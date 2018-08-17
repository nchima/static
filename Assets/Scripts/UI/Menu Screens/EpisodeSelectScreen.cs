using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EpisodeSelectScreen : MonoBehaviour {

	public void BeginningButton() {
        Services.levelManager.SetStartingLevelSet("GDC Level Set");
        GameEventManager.instance.FireEvent(new GameEvents.GameStarted());
    }

    public void CathedralButton() {
        Services.levelManager.SetStartingLevelSet("Cathedral Level Set");
        GameEventManager.instance.FireEvent(new GameEvents.GameStarted());
    }

    public void AnatomiaButton() {
        Services.levelManager.SetStartingLevelSet("Anatomia Level Set");
        GameEventManager.instance.FireEvent(new GameEvents.GameStarted());
    }

    public void IslandsButton() {
        Services.levelManager.SetStartingLevelSet("Islands Level Set");
        GameEventManager.instance.FireEvent(new GameEvents.GameStarted());
    }

    public void BackButton() {
        Services.uiManager.ShowTitleScreen(true);
    }
}
