﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    [SerializeField] IntRange levelSceneIndices;
    [SerializeField] float howOftenToLoadNewLevelWhenFalling = 1f;

    public enum LoadingState { LoadingRandomly, Idle }
    [HideInInspector] public LoadingState loadingState = LoadingState.Idle;

    float loadRandomLevelTimer = 0f;

    [HideInInspector] int currentlyLoadedLevel = 1;

    [HideInInspector] public int levelsCompleted = 0;
    public int LevelNumber { get { return levelsCompleted + 1; } }
    bool isLevelLoaded
    {
        get {
            if (currentlyLoadedLevel == 0) { return false; }
            return SceneManager.GetSceneByBuildIndex(currentlyLoadedLevel).isLoaded;
        }
    }
    [HideInInspector] public bool isLevelCompleted = false;

    LevelScaler levelScaler;
    EnemyPlacer enemyPlacer;

    // Deprecated:
    //GameObject[] levelChunks;


    private void Awake() {
        levelScaler = GetComponent<LevelScaler>();
        enemyPlacer = GetComponent<EnemyPlacer>();

        loadRandomLevelTimer = howOftenToLoadNewLevelWhenFalling;
    }


    //private void Update() {
    //    switch (loadingState) {
    //        case LoadingState.Idle:
    //            loadRandomLevelTimer = howOftenToLoadNewLevelWhenFalling;
    //            break;
    //        case LoadingState.LoadingRandomly:
    //            LoadRandomly();
    //            break;
    //    }
    //}

    
    private void LoadRandomly() {
        loadRandomLevelTimer += Time.deltaTime;
        if (loadRandomLevelTimer >= howOftenToLoadNewLevelWhenFalling) {
            LoadRandomLevel();
            loadRandomLevelTimer = 0f;
        }
    }


    public void LoadRandomLevel() {
        if (levelsCompleted == 30) {
            SceneManager.UnloadSceneAsync(levelsCompleted);
            GameManager.instance.ShowEndOfDemoScreen();
            return;
        }

        LoadLevel(levelSceneIndices.Random);
    }


    public void LoadNextLevel() {
        if (levelsCompleted == 30) {
            SceneManager.UnloadSceneAsync(levelsCompleted);
            GameManager.instance.ShowEndOfDemoScreen();
            return;
        }

        LoadLevel(levelsCompleted + 1);
    }


    public void LoadLevel(int levelNumber) {
        StartCoroutine(LoadLevelCoroutine(levelNumber));
    }

    
    private IEnumerator LoadLevelCoroutine(int levelNumber) {
        if (isLevelLoaded) {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(currentlyLoadedLevel);
            while (!unload.isDone) { yield return null; }
        }

        AsyncOperation load = SceneManager.LoadSceneAsync(levelNumber, LoadSceneMode.Additive);
        while (!load.isDone) { yield return null; }

        // Scale level according to, you know, whatever I guess
        //levelScaler.ScaleLevel(1f);
        //enemyPlacer.PlaceEnemies();

        isLevelCompleted = false;
        currentlyLoadedLevel = levelNumber;

        yield return null;
    }


    public void SetEnemiesActive(bool value) {
        StartCoroutine(SetEnemiesActiveCoroutine(value));
    }


    IEnumerator SetEnemiesActiveCoroutine(bool value) {
        yield return new WaitForSeconds(0.1f);

        foreach (EnemyOld enemy in FindObjectsOfType<EnemyOld>()) {
            enemy.enabled = value;
            enemy.willAttack = true;
        }

        foreach (Enemy enemy in FindObjectsOfType<Enemy>()) {
            enemy.isAIPaused = !value;
            enemy.enabled = value;
        }

        yield return null;
    }


    public void SetFloorCollidersActive(bool value) {
        Collider[] floorColliders = GameObject.Find("Floor Planes").GetComponentsInChildren<Collider>();
        foreach (Collider collider in floorColliders) collider.enabled = value;
    }


    public void LockInLevel() {
        loadingState = LoadingState.Idle;

        loadRandomLevelTimer = howOftenToLoadNewLevelWhenFalling;

        GameManager.levelManager.SetEnemiesActive(true);

        // Re-enable the floor's collision (since it is disabled when the player completes a level.)
        GameManager.levelManager.SetFloorCollidersActive(true);

        // Update billboards.
        GameManager.instance.GetComponent<BatchBillboard>().FindAllBillboards();
    }


    /*
    // Deprecated:
    public void ChooseLevelChunks() {

        // If the array of original level chunks has not been collected, do so now.
        if (levelChunks == null) {
            levelChunks = GameObject.FindGameObjectsWithTag("Level Chunk");
        }

        // If it has, then collect any 'used' level chunks and destroy them.
        else {
            foreach(GameObject chunk in GameObject.FindGameObjectsWithTag("Level Chunk")) { Destroy(chunk); }
        }

        for (int i = 0; i < levelChunks.Length; i++) { levelChunks[i].SetActive(true); }

        int chunk1 = 0;
        int chunk2 = 0;

        for (int i = 0; i < 100 && chunk1 == chunk2; i++) {
            chunk1 = Random.Range(0, levelChunks.Length - 1);
            chunk2 = Random.Range(0, levelChunks.Length - 1);
        }

        Instantiate(levelChunks[chunk1]);
        Instantiate(levelChunks[chunk2]);

        for (int i = 0; i < levelChunks.Length; i++) { levelChunks[i].SetActive(false); }

        //for (int i = 0; i < levelChunks.Length; i++) {
        //    if (i == chunk1 || i == chunk2) {
        //        levelChunks[i].SetActive(true);
        //    }
        //    else { levelChunks[i].SetActive(false); }
        //}
    }


    //CloneLevelChunk(int index) {
    //    GameObject newLevelChunk = Instantiate(levelChunks[index]);
    //}
    */
}
