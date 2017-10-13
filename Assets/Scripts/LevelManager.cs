using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    [SerializeField] Scene[] levels;
    public int currentLevelNumber = 1;
    bool isLevelLoaded
    {
        get
        {
            return SceneManager.GetSceneByBuildIndex(currentLevelNumber).isLoaded;
        }
    }


    public void LoadNextLevel()
    {
        LoadLevel(currentLevelNumber + 1);
        currentLevelNumber++;
    }


    public void LoadLevel(int levelNumber)
    {
        if (isLevelLoaded) { SceneManager.UnloadSceneAsync(currentLevelNumber); }
        SceneManager.LoadScene(levelNumber, LoadSceneMode.Additive);
    }


    public void SetFloorCollidersActive(bool value)
    {
        Collider[] floorColliders = GameObject.Find("Floor Planes").GetComponentsInChildren<Collider>();
        foreach (Collider collider in floorColliders) collider.enabled = value;
    }
}
