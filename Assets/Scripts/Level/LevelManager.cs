using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {

    [SerializeField] Scene[] levels;
    GameObject[] levelChunks;
    public int currentLevelNumber = 1;
    bool isLevelLoaded
    {
        get
        {
            return SceneManager.GetSceneByBuildIndex(currentLevelNumber).isLoaded;
        }
    }
    [HideInInspector] public bool isLevelCompleted = false;


    private void Start() {
        //levelChunks = GameObject.FindGameObjectsWithTag("Level Chunk");
        //Debug.Log("level chunks length: " + levelChunks.Length);
        //if (!isLevelLoaded) { LoadLevel(currentLevelNumber); }
    }


    public void LoadNextLevel() {
        LoadLevel(currentLevelNumber + 1);
        currentLevelNumber++;
    }


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


    public void LoadLevel(int levelNumber) {
        if (isLevelLoaded) { SceneManager.UnloadSceneAsync(currentLevelNumber); }
        SceneManager.LoadScene(levelNumber, LoadSceneMode.Additive);
        isLevelCompleted = false;
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
            enemy.pauseAI = !value;
            enemy.enabled = value;
        }

        yield return null;
    }


    public void SetFloorCollidersActive(bool value) {
        Collider[] floorColliders = GameObject.Find("Floor Planes").GetComponentsInChildren<Collider>();
        foreach (Collider collider in floorColliders) collider.enabled = value;
    }
}
