using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using DG.Tweening;
using UnityEngine.Audio;
public class GameManager : MonoBehaviour {

    // DEBUG STUFF
    public bool dontChangeLevel;

    // AUDIO
    [SerializeField] private AudioSource levelWinAudio;   // The audio source that plays when the player completes a level.

    // USED FOR LEVEL GENERATION
    int numberOfEnemies = 2;    // The number of enemies that spawned in the current level.
    public int currentEnemyAmt;    // The number of enemies currently alive in this level.
    public LevelGenerator levelGenerator;  // A reference to the level generator script.
    public static LevelManager levelManager;

    // MENU SCREENS
    [SerializeField] GameObject highScoreScreen;
    [SerializeField] GameObject gameOverScreen; 
    [SerializeField] GameObject nameEntryScreen;
    [SerializeField] GameObject mainMenuScreen; 

    // RANDOM USEFUL STUFF
    public static bool gameStarted = false;
    Vector3 initialGravity;

    // MISC REFERENCES
    [HideInInspector] public static GameManager instance;
    [HideInInspector] public ScoreManager scoreManager;
    [HideInInspector] public static SpecialBarManager specialBarManager;
    [HideInInspector] public static HealthManager healthManager;
    [HideInInspector] public static FallingSequenceManager fallingSequenceManager;
    [HideInInspector] public static MusicManager musicManager;
    [HideInInspector] public static SFXManager sfxManager;
    [HideInInspector] public Gun gun;
    [HideInInspector] public static GunValueManager gunValueManager;
    [HideInInspector] public static ColorPaletteManager colorPaletteManager;
    [HideInInspector] public GenerateNoise noiseGenerator;
    [HideInInspector] public static GameObject player;
    [SerializeField] GameObject gunSliderBorder;


    void Awake() {
        // Get various references.
        instance = this;
        player = GameObject.Find("Player");
        scoreManager = GetComponent<ScoreManager>();
        specialBarManager = GetComponentInChildren<SpecialBarManager>();
        healthManager = GetComponentInChildren<HealthManager>();
        levelGenerator = GetComponent<LevelGenerator>();
        levelManager = GetComponentInChildren<LevelManager>();
        fallingSequenceManager = GetComponentInChildren<FallingSequenceManager>();
        sfxManager = GetComponentInChildren<SFXManager>();
        musicManager = GetComponentInChildren<MusicManager>();
        colorPaletteManager = GetComponentInChildren<ColorPaletteManager>();
        gun = FindObjectOfType<Gun>();
        gunValueManager = GetComponentInChildren<GunValueManager>();
        noiseGenerator = GetComponent<GenerateNoise>();
    }


    private void Start() {
        StartCoroutine(InitialSetup());
    }


    private void Update() {
        if (!gameStarted) {
            if (Input.GetMouseButtonDown(0)) {
                StartGame();
            }
        }
    }


    IEnumerator InitialSetup() {
        gun.enabled = false;
        player.GetComponent<PlayerController>().isMovementEnabled = false;
        
        levelManager.LoadLevel(levelManager.currentLevelNumber);

        while (!SceneManager.GetSceneByBuildIndex(levelManager.currentLevelNumber).isLoaded) { yield return null; }

        levelManager.SetEnemiesActive(false);
        fallingSequenceManager.BeginFallingInstant();

        initialGravity = Physics.gravity;
        Physics.gravity = Vector3.zero;

        yield return null;
    }


    public void LoadNextLevel() {
        levelManager.LoadNextLevel();
    }


    public void PlayerUsedSpecialMove() {
        specialBarManager.PlayerUsedSpecialMove();
    }


    public void ReturnToFullSpeed() {
        // Begin tweening time scale, gun burst rate, and music pitch back to normal.
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
        DOTween.To(() => gun.burstsPerSecondSloMoModifierCurrent, x => gun.burstsPerSecondSloMoModifierCurrent = x, 1f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
    }


    public void PlayerKilledEnemy(int scoreValue, float specialValue) {
        // Add score and special bar values.
        scoreManager.PlayerKilledEnemy(scoreValue);
        specialBarManager.AddValue(specialValue);

        // If player has killed all the enemies in the current level, begin the level completion sequence.
        currentEnemyAmt -= 1;
        if (currentEnemyAmt <= 0) { LevelComplete(); }
    }


    public void LevelComplete() {
        if (fallingSequenceManager.isPlayerFalling) return;
        if (dontChangeLevel) return;

        levelWinAudio.Play();

        scoreManager.LevelComplete();
        levelManager.isLevelCompleted = true;

        gun.canShoot = false;

        GatherRemainingAmmoPickups();

        //if (healthManager.playerHealth < 5) healthManager.playerHealth++;

        // Disable the floor's collider so the player falls through it.
        SetFloorCollidersActive(false);

        // Initiate falling sequence.
        fallingSequenceManager.BeginFalling();
    }


    void GatherRemainingAmmoPickups() {
        foreach(SpecialMoveAmmo specialMoveAmmo in FindObjectsOfType<SpecialMoveAmmo>()) { specialMoveAmmo.BeginMovingTowardsPlayer(); }
    }


    public void CountEnemies() {
        currentEnemyAmt = FindObjectsOfType<EnemyOld>().Length;
        currentEnemyAmt = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }


    //public void FreezeSpecialBarDecay(bool value) {
    //    specialBarManager.freezeDecay = value;
    //}


    public void DetermineBonusTime() {
        scoreManager.DetermineBonusTime();
    }


    public void SetFloorCollidersActive(bool value) {
        levelManager.SetFloorCollidersActive(value);
    }


    public void PlayerWasHurt() {
        scoreManager.GetHurt();
        specialBarManager.PlayerWasHurt();
        healthManager.playerHealth -= 4;

        // If health is now less than zero, trigger a game over.
        if (healthManager.playerHealth <= 0) {
            ShowGameOverScreen();
        }
    }


    public void ShowHighScores() {
        scoreManager.RetrieveScoresForHighScoreScreen();

        gameOverScreen.gameObject.SetActive(false);
        nameEntryScreen.gameObject.SetActive(false);
        highScoreScreen.gameObject.SetActive(true);
    }


    public void BulletHitEnemy() {
        //if (gunMethod == GunMethod.TuningBased && (currentSine < currentIdealRange.min || currentSine > currentIdealRange.max)) return;
        scoreManager.BulletHitEnemy();
        //sineTime += bulletHitSineIncrease;
    }


    void ShowGameOverScreen() {
        gameOverScreen.SetActive(true);
    }


    public void StartGame() {
        // Unpause enemies in the background.
        levelManager.SetEnemiesActive(true);

        player.GetComponent<PlayerController>().isMovementEnabled = true;
        gun.enabled = true;

        Physics.gravity = initialGravity;

        gameStarted = true;
    }


    public void RestartGame() {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void UpdateBillboards() {
        FindObjectOfType<BatchBillboard>().FindAllBillboards();
    }

    public bool PositionIsInLevelBoundaries(Vector3 position) {
        if (position.x > levelGenerator.baseLevelSize / 2 ||
            position.x < -levelGenerator.baseLevelSize / 2 ||
            position.z > levelGenerator.baseLevelSize / 2 ||
            position.z < -levelGenerator.baseLevelSize / 2)
        {
            return false;
        }

        else return true;
    }
}
