using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class LevelManager : MonoBehaviour {

    [SerializeField] List<LevelSet> levelSets;
    [SerializeField] LevelSet startingLevelSetOverride;

    [SerializeField] GameObject scoreBonusPrefab;

    LevelSet currentLevelSet;

    [HideInInspector] public LevelData currentlyLoadedLevel;
    [HideInInspector] public int levelsCompleted = 0;
    [HideInInspector] public bool isLevelCompleted = false;

    public int CurrentLevelNumber { get { return levelsCompleted + 1; } }
    public int TotalNumberOfLevels { get {
            int numberOfLevels = 0;
            for (int i = 0; i < levelSets.Count; i++) { numberOfLevels += levelSets[i].levelDataReferences.Count; }
            return numberOfLevels;
        } }
    bool IsLevelLoaded {
        get {
            if (currentlyLoadedLevel == null) { return false; }
            return SceneManager.GetSceneByBuildIndex(currentlyLoadedLevel.buildIndex).isLoaded;
        }
    }

    EnemyPlacer enemyPlacer { get { return GetComponent<EnemyPlacer>(); } }

    // Deprecated things from when levels included more randomness.
    //LevelScaler levelScaler;
    //LevelInfo[] levelInfos;


    private void Awake() {

        if (startingLevelSetOverride != null) {
            SetStartingLevelSet(startingLevelSetOverride);
        } else {
            SetStartingLevelSet(GetLevelSet("GDC Level Set"));
        }

        //LoadNextLevel();

        //levelScaler = GetComponent<LevelScaler>();

        //levelInfos = new LevelInfo[Resources.LoadAll<LevelInfo>("Level Info").Length];
        //for (int i = 0; i < levelInfos.Length; i++) {
        //    string levelNumber = (i+1).ToString();
        //    levelInfos[i] = Resources.Load<LevelInfo>("Level Info/Level Info " + levelNumber);
        //}

        //loadRandomLevelTimer = howOftenToLoadNewLevelWhenFalling;
    }


    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameStarted>(GameStartedHandler);
    }


    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameStarted>(GameStartedHandler);

        for (int i = 0; i < levelSets.Count; i++) {
            levelSets[i].levelsCompleted = 0;
        }
    }

    public void LoadNextLevel() {
        // If the player has completed every level, show the end of demo screen.
        if (IsLevelLoaded && (levelsCompleted >= TotalNumberOfLevels)) {
            SceneManager.UnloadSceneAsync(currentlyLoadedLevel.buildIndex);
            Services.uiManager.ShowEndOfDemoScreen();
            return;
        }

        // If the player has completed all the levels in a set, load the next set.
        if (currentLevelSet.AllLevelsCompleted) {
            LevelSet nextLevelSet = null;
            for (int i = 0; i < levelSets.Count; i++) {
                if (!levelSets[i].AllLevelsCompleted) {
                    nextLevelSet = levelSets[i];
                    break;
                }
            }

            if (nextLevelSet == null) {
                Debug.Log("ending demo here.");
                SceneManager.UnloadSceneAsync(levelsCompleted);
                Services.uiManager.ShowEndOfDemoScreen();
                return;
            } else {
                currentLevelSet = nextLevelSet;
            }
        }

        StartCoroutine(LoadLevelCoroutine(currentLevelSet.NextLevel));
    }

    
    private IEnumerator LoadLevelCoroutine(LevelData levelData) {
        if (IsLevelLoaded) {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(currentlyLoadedLevel.buildIndex);
            while (!unload.isDone) { yield return null; }
        }

        AsyncOperation load = SceneManager.LoadSceneAsync(levelData.buildIndex, LoadSceneMode.Additive);
        while (!load.isDone) { yield return null; }

        // Scale level according to, you know, whatever I guess
        //levelScaler.ScaleLevel(levelInfos[levelsCompleted].levelSize);

        yield return new WaitForSeconds(0.2f);

        //enemyPlacer.PlaceEnemies(levelInfos[levelsCompleted]);
        for (int i = 0; i < 5; i++) { enemyPlacer.PlaceObject(scoreBonusPrefab); }
        //SetEnemiesActive(false);

        isLevelCompleted = false;
        currentlyLoadedLevel = levelData;

        yield return null;
    }


    public void SetStartingLevelSet(LevelSet startingSet) {

        Debug.Log("Setting starting level set to: " + startingSet.name);

        if (startingLevelSetOverride != null) {
            Debug.Log("Starting level overridden by user.");
            return;
        }
 
        if (!levelSets.Contains(startingSet)) {
            levelSets.Add(startingSet);
        }

        currentLevelSet = startingSet;
    }


    public void SetStartingLevelSet(string setName) {
        SetStartingLevelSet(GetLevelSet(setName));
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
            enemy.SetAIActive(value);
            enemy.enabled = value;
        }

        if (value == false) { yield return null; }

        yield return null;
    }


    public void SetFloorCollidersActive(bool value) {
        if (!GameObject.Find("Floor Planes")) { return; }
        Collider[] floorColliders = GameObject.Find("Floor Planes").GetComponentsInChildren<Collider>();
        foreach (Collider collider in floorColliders) collider.enabled = value;
    }


    public void LockInLevel() {
        Services.levelManager.SetEnemiesActive(true);

        // Re-enable the floor's collision (since it is disabled when the player completes a level.)
        Services.levelManager.SetFloorCollidersActive(true);

        // Update billboards.
        Services.gameManager.GetComponent<BatchBillboard>().FindAllBillboards();
    }


    public LevelSet GetLevelSet(string name) {
        for (int i = 0; i < levelSets.Count; i++) {
            if (levelSets[i].name == name) { return levelSets[i]; }
        }

        Debug.LogError("Hey, sorry but I couldn't find a level set with that name. :-(");
        return null;
    }


    public void LevelCompletedHandler(GameEvent gameEvent) {
        isLevelCompleted = true;
        currentLevelSet.levelsCompleted++;
        levelsCompleted++;
        SetFloorCollidersActive(false);
    }


    public void GameStartedHandler(GameEvent gameEvent) {
        LoadNextLevel();
        SetEnemiesActive(true);
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
