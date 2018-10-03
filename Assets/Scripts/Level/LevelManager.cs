using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class LevelManager : MonoBehaviour {

    [SerializeField] LevelSet overrideLevelSet;
    [SerializeField] LevelBranchNode firstBranchNode;

    [HideInInspector] public LevelSet currentLevelSet;
    LevelBranchNode currentBranchNode;

    [HideInInspector] public LevelData currentlyLoadedLevelData;
    [HideInInspector] public int totalLevelsCompleted = 0;
    [HideInInspector] public bool isLevelCompleted = false;

    bool newEpisodeUITrigger = false;
    LevelBranchNode previousNode;

    // Key: name of branch node gameObject, Value: name of branch node gameObject entered from that one
    public List<ChosenPath> chosenPaths = new List<ChosenPath>();
    public class ChosenPath {
        public LevelBranchNode fromNode;
        public LevelBranchNode toNode;
        public ChosenPath(LevelBranchNode fromNode, LevelBranchNode toNode) {
            this.fromNode = fromNode;
            this.toNode = toNode;
        }
    }

    public int CurrentLevelNumber { get { return totalLevelsCompleted + 1; } }
    bool IsLevelCurrentlyLoaded {
        get {
            if (currentlyLoadedLevelData == null) { return false; }
            return SceneManager.GetSceneByBuildIndex(currentlyLoadedLevelData.buildIndex).isLoaded;
        }
    }

    // Triggers used by falling sequence
    [HideInInspector] public bool loadingSequenceFinishedTrigger = false;
    [HideInInspector] public bool uiSequencedFinishedTrigger = false;

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
    }

    public void GameStartedHandler(GameEvent gameEvent) {
        chosenPaths.Clear();
        LoadNextLevel();
        SetEnemiesAIActive(true);
    }

    public void LevelCompletedHandler(GameEvent gameEvent) {
        isLevelCompleted = true;

        SetFloorCollidersActive(false);

        // Update completed level counts
        currentLevelSet.levelsCompleted++;
        totalLevelsCompleted++;

        // Begin loading the next level
        LoadNextLevel(1f);

        // Determine whether we are entering a new episode and show the correct UI sequence
        if (newEpisodeUITrigger) {
            newEpisodeUITrigger = false;
            StartCoroutine(EpisodeCompleteUISequence());
        }
        else { StartCoroutine(LevelCompleteUISequence()); }
    }

    IEnumerator EpisodeCompleteUISequence() {
        // Hide HUD and show episode complete screen.
        Services.uiManager.hud.SetActive(false);
        Services.uiManager.ShowEpisodeCompleteScreen(true);

        // Wait until a certain amount of time has transpired or the player has clicked.
        float duration = 2f;
        float timer = 0f;
        bool inputAccepted = false;
        yield return new WaitUntil(() => {
            if ((InputManager.submitButtonDown || InputManager.fireButtonDown) && timer >= 0.5f && !inputAccepted) {
                inputAccepted = true;
                return true;
            }

            if (timer >= duration) {
                return true;
            }

            else {
                timer += Time.unscaledDeltaTime;
                return false;
            }
        });

        // Show game map
        Services.uiManager.episodeCompleteScreen.SetActive(false);
        Services.uiManager.gameMap.SetActive(true);
        Services.uiManager.pathSelectedScreen.SetActive(true);
        Services.uiManager.pathSelectedScreen.GetComponent<PathSelectedScreen>().UpdateText(Services.levelManager.currentLevelSet.Name);
        Services.uiManager.gameMap.GetComponent<GameMap>().UnHighlightAll();
        Services.uiManager.gameMap.GetComponent<GameMap>().HighlightPath();

        // Wait for timer or mouseclick again...
        duration = 5f;
        timer = 0f;
        yield return new WaitUntil(() => {
            if ((InputManager.submitButtonDown || InputManager.fireButtonDown) && timer >= 0.5f && !inputAccepted) {
                inputAccepted = true;
                return true;
            }

            else {
                inputAccepted = false;
            }

            if (timer >= duration) { return true; }
            else {
                timer += Time.unscaledDeltaTime;
                return false;
            }
        });

        // Show hud again
        Services.uiManager.gameMap.SetActive(false);
        Services.uiManager.pathSelectedScreen.SetActive(false);
        Services.uiManager.hud.SetActive(true);

        uiSequencedFinishedTrigger = true;

        yield return null;
    }

    IEnumerator LevelCompleteUISequence() {

        Services.uiManager.ShowLevelCompleteScreen(true);

        yield return new WaitForSeconds(1.5f);

        uiSequencedFinishedTrigger = true;

        yield return null;
    }

    private void LoadNextLevel() {
        LoadNextLevel(0f);
    }
    private void LoadNextLevel(float delay) {
        // If the player has completed all the levels in a set, load the next set.
        if (currentLevelSet.AllLevelsCompleted) {
            newEpisodeUITrigger = true;

            // See if the player has completed one of the final nodes, end the game.
            if (currentBranchNode.DetermineNext() == null) {
                Debug.Log("Player completed game.");
                Services.gameManager.PlayerCompletedGame();
                return;
            }

            // Otherwise, determine the next level set and set it as the current branch node
            previousNode = currentBranchNode;
            currentBranchNode = currentBranchNode.DetermineNext();
            Debug.Log("Entering branch node: " + currentBranchNode.name);

            // Unlock the current branch and reset its levels complete count.
            currentBranchNode.IsUnlocked = true;
            currentLevelSet = currentBranchNode.levelSet;
            currentLevelSet.levelsCompleted = 0;

            // Save the path taken
            chosenPaths.Add(new ChosenPath(previousNode, currentBranchNode));
        }

        // Begin loading the level
        Debug.Log("Loading: " + currentLevelSet.NextLevel.LevelName);
        StartCoroutine(LoadLevelCoroutine(currentLevelSet.NextLevel, delay));
    }

    private IEnumerator LoadLevelCoroutine(LevelData levelData, float delay) {
        // Delay is used when the player completes a level to give them a moment to fall below the floor of the current level before it is unloaded/reloaded.
        yield return new WaitForSeconds(delay);

        if (IsLevelCurrentlyLoaded) {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(currentlyLoadedLevelData.buildIndex);
            while (!unload.isDone) { yield return null; }
        }

        AsyncOperation load = SceneManager.LoadSceneAsync(levelData.buildIndex, LoadSceneMode.Additive);
        while (!load.isDone) { yield return null; }

        // Scale level according to, you know, whatever I guess
        //levelScaler.ScaleLevel(levelInfos[levelsCompleted].levelSize);

        //enemyPlacer.PlaceEnemies(levelInfos[levelsCompleted]);
        //for (int i = 0; i < 5; i++) { enemyPlacer.PlaceObject(scoreBonusPrefab); }
        //SetEnemiesActive(false);

        currentlyLoadedLevelData = levelData;

        loadingSequenceFinishedTrigger = true;

        yield return null;
    }

    public void SetStartingLevelSet(LevelSet startingSet) {

#if UNITY_EDITOR
        if (overrideLevelSet != null) {
            Debug.Log("Starting level overridden by user.");
            startingSet = overrideLevelSet;
        }
#endif
        currentLevelSet = startingSet;

        Debug.Log("Setting starting level set to: " + currentLevelSet.name);
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

        // Update billboards.
        Services.gameManager.GetComponent<BatchBillboard>().FindAllBillboards();
    }

    public void SetAllLevelsUnlockState(bool value) {
        RecursiveBranchLock(firstBranchNode, value);
    }

    void RecursiveBranchLock(LevelBranchNode node, bool value) {
        node.IsUnlocked = value;
        Debug.Log("Setting " + node.gameObject.name + " unlocked state to " + value);
        if (node.branches == null || node.branches.Length == 0) { Debug.Log("Returning from " + node.gameObject.name);  return; }
        foreach (LevelBranchNode branchNode in node.branches) {
            if (branchNode != null) { RecursiveBranchLock(branchNode, value); }
        }
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
