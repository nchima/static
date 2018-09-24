using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class LevelManager : MonoBehaviour {

    [SerializeField] List<LevelSet> levelSets;
    [SerializeField] LevelSet overrideLevelSet;
    [SerializeField] LevelBranchNode firstBranchNode;

    [HideInInspector] public LevelSet currentLevelSet;
    LevelBranchNode currentBranchNode;

    [HideInInspector] public LevelData currentlyLoadedLevelData;
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
            if (currentlyLoadedLevelData == null) { return false; }
            return SceneManager.GetSceneByBuildIndex(currentlyLoadedLevelData.buildIndex).isLoaded;
        }
    }

    private void Awake() {
        // Make sure the first level set is unlocked
        firstBranchNode.IsUnlocked = true;

        // Load the first level set.
        if (overrideLevelSet != null) {
#if UNITY_EDITOR
            SetStartingLevelSet(overrideLevelSet);
#endif
        } else {
            currentBranchNode = firstBranchNode;
            currentBranchNode.levelSet.levelsCompleted = 0;
            SetStartingLevelSet(currentBranchNode.levelSet);
        }
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

    public void GameStartedHandler(GameEvent gameEvent) {
        LoadNextLevel();
        SetEnemiesAIActive(true);
    }

    public void LevelCompletedHandler(GameEvent gameEvent) {
        isLevelCompleted = true;
        currentLevelSet.levelsCompleted++;
        levelsCompleted++;
        if (currentLevelSet.AllLevelsCompleted) { Services.uiManager.ShowEpisodeCompleteScreen(true); }
        else { Services.uiManager.ShowLevelCompleteScreen(true); }
        SetFloorCollidersActive(false);
    }

    public void LoadNextLevel() {
        // If the player has completed all the levels in a set, load the next set.
        if (currentLevelSet.AllLevelsCompleted) {
            Debug.Log("all levels complete");

            // See if the player has completed the final level.
            if (currentBranchNode.DetermineNext() == null) {
                Services.gameManager.PlayerCompletedGame();
                return;
            }

            // Determine the next level set and load it.
            currentBranchNode = currentBranchNode.DetermineNext();
            Debug.Log("Unlocking: " + currentBranchNode.levelSet.Name);
            currentBranchNode.IsUnlocked = true;
            currentLevelSet = currentBranchNode.levelSet;
            currentLevelSet.levelsCompleted = 0;
        }

        StartCoroutine(LoadLevelCoroutine(currentLevelSet.NextLevel));
    }

    private IEnumerator LoadLevelCoroutine(LevelData levelData) {
        if (IsLevelLoaded) {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(currentlyLoadedLevelData.buildIndex);
            while (!unload.isDone) { yield return null; }
        }

        AsyncOperation load = SceneManager.LoadSceneAsync(levelData.buildIndex, LoadSceneMode.Additive);
        while (!load.isDone) { yield return null; }

        // Scale level according to, you know, whatever I guess
        //levelScaler.ScaleLevel(levelInfos[levelsCompleted].levelSize);

        yield return new WaitForSeconds(0.2f);

        //enemyPlacer.PlaceEnemies(levelInfos[levelsCompleted]);
        //for (int i = 0; i < 5; i++) { enemyPlacer.PlaceObject(scoreBonusPrefab); }
        //SetEnemiesActive(false);

        currentlyLoadedLevelData = levelData;

        yield return null;
    }

    void LoadBranchNode(LevelBranchNode branchNode) {
        currentLevelSet = branchNode.levelSet;
    }

    public void SetStartingLevelSet(LevelSet startingSet) {

#if UNITY_EDITOR
        if (overrideLevelSet != null) {
            Debug.Log("Starting level overridden by user.");
            startingSet = overrideLevelSet;
        }
#endif

        if (!levelSets.Contains(startingSet)) {
            levelSets.Add(startingSet);
        }

        currentLevelSet = startingSet;

        Debug.Log("Setting starting level set to: " + currentLevelSet.name);
    }

    public void SetStartingLevelSet(string setName) {
        SetStartingLevelSet(GetLevelSet(setName));
    }

    public void SetEnemiesAIActive(bool value) {
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
        Services.levelManager.SetEnemiesAIActive(true);

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

    public void ResetLevelProgress() {
        RecursiveBranchReset(firstBranchNode);
    }

    void RecursiveBranchReset(LevelBranchNode node) {
        node.IsUnlocked = false;
        if (node.branch1 != null) { RecursiveBranchReset(node.branch1); }
        if (node.branch2 != null) { RecursiveBranchReset(node.branch2); }
    }

    // Deprecated code:
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
