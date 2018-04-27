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

    // RANDOM USEFUL STUFF
    public bool gameStarted = false;
    Vector3 initialGravity;


    void Awake() {
        // Set up services manager
        Services.gameManager = this;
        Services.playerGameObject = FindObjectOfType<PlayerController>().gameObject;
        Services.playerTransform = FindObjectOfType<PlayerController>().transform;
        Services.playerController = FindObjectOfType<PlayerController>();
        Services.scoreManager = GetComponent<ScoreManager>();
        Services.specialBarManager = GetComponentInChildren<SpecialBarManager>();
        Services.healthManager = GetComponentInChildren<HealthManager>();
        Services.levelManager = GetComponentInChildren<LevelManager>();
        Services.levelGenerator = GetComponentInChildren<LevelGenerator>();
        Services.fallingSequenceManager = GetComponentInChildren<FallingSequenceManager>();
        Services.sfxManager = GetComponentInChildren<SFXManager>();
        Services.uiManager = GetComponentInChildren<UIManager>();
        Services.musicManager = GetComponentInChildren<MusicManager>();
        Services.colorPaletteManager = GetComponentInChildren<ColorPaletteManager>();
        Services.gun = FindObjectOfType<Gun>();
        Services.gunValueManager = GetComponentInChildren<GunValueManager>();
        Services.noiseGenerator = GetComponent<GenerateNoise>();
        Services.flashManager = GetComponentInChildren<FlashManager>();
    }


    private void Start() {
        StartCoroutine(InitialSetup());
    }


    private void Update() {
        if (!gameStarted && !gamePaused) {
            if (InputManager.fireButtonDown) {
                StartGame();
            }
        }

        if (InputManager.pauseButtonDown) {
            if (!gamePaused && !playerIsDead) { PauseGame(true); } else { PauseGame(false); }
        }

    }


    float memorizedTimeScale;
    public static bool gamePaused;
    public void PauseGame(bool value) {
        if (value == true) {
            Services.uiManager.ShowPauseScreen();
            memorizedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            gamePaused = true;
        }

        else {
            Services.uiManager.HidePauseScreen();
            Time.timeScale = memorizedTimeScale;
            if (!gameStarted) { Services.uiManager.titleScreen.SetActive(true); }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            gamePaused = false;
        }
    }


    IEnumerator InitialSetup() {
        Services.gun.enabled = false;
        Services.playerController.isMovementEnabled = false;

        //levelManager.loadingState = LevelManager.LoadingState.LoadingRandomly;
        Services.levelManager.LoadNextLevel();

        //yield return new WaitUntil(() => {
        //    if (SceneManager.GetSceneByBuildIndex(levelManager.levelsCompleted).isLoaded) { return true; } 
        //    else { return false; }
        //});

        //levelManager.SetEnemiesActive(false);
        Services.fallingSequenceManager.BeginFallingInstant();

        initialGravity = Physics.gravity;
        Physics.gravity = Vector3.zero;

        yield return null;
    }

    public void PlayerUsedSpecialMove() {
        Services.specialBarManager.PlayerUsedSpecialMove();
    }


    public void ReturnToFullSpeed() {
        // Begin tweening time scale, gun burst rate, and music pitch back to normal.
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
        DOTween.To(() => Services.gun.burstsPerSecondSloMoModifierCurrent, x => Services.gun.burstsPerSecondSloMoModifierCurrent = x, 1f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
        Services.musicManager.ReturnMusicPitchToFullSpeed();
    }


    public void PlayerKilledEnemy(int scoreValue, float specialValue) {
        // Add score and special bar values.
        Services.scoreManager.PlayerKilledEnemy(scoreValue);
        Services.specialBarManager.AddValue(specialValue);

        // If player has killed all the enemies in the current level, begin the level completion sequence.
        currentEnemyAmt -= 1;
        if (currentEnemyAmt <= 0) { LevelComplete(); }
    }


    public void LevelComplete() {
        if (Services.fallingSequenceManager.isPlayerFalling) return;
        if (dontChangeLevel) return;

        Services.musicManager.EnterFallingSequence();
        levelWinAudio.Play();

        Services.scoreManager.LevelComplete();
        Services.levelManager.isLevelCompleted = true;
        Services.levelManager.levelsCompleted++;

        Services.gun.canShoot = false;

        GatherRemainingAmmoPickups();

        //if (healthManager.playerHealth < 5) healthManager.playerHealth++;

        // Disable the floor's collider so the player falls through it.
        Services.levelManager.SetFloorCollidersActive(false);

        // Initiate falling sequence.
        Services.fallingSequenceManager.BeginFalling();
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
        Services.scoreManager.DetermineBonusTime();
    }


    bool playerIsDead;
    public void PlayerWasHurt() {
        Services.scoreManager.GetHurt();
        Services.specialBarManager.PlayerWasHurt();
        Services.healthManager.playerHealth -= 1;

        // If health is now less than zero, trigger a game over.
        if (Services.healthManager.playerHealth == 0) {
            playerIsDead = true;
            Services.playerController.enabled = false;
            Services.uiManager.ShowGameOverScreen();
        }
    }


    public void ShowHighScores() {
        Services.scoreManager.RetrieveScoresForHighScoreScreen();
        Services.uiManager.ShowHighScoreScreen();
    }


    public void BulletHitEnemy() {
        //if (gunMethod == GunMethod.TuningBased && (currentSine < currentIdealRange.min || currentSine > currentIdealRange.max)) return;
        Services.scoreManager.BulletHitEnemy();
        //sineTime += bulletHitSineIncrease;
    }


    public void ShowEndOfDemoScreen() {
        Services.uiManager.ShowEndOfDemoScreen();
    }


    public void StartGame() {
        if (gamePaused) { return; }

        // Unpause enemies in the background.
        Services.levelManager.SetEnemiesActive(true);

        Services.uiManager.ShowTitleScreen(false);

        Services.playerController.isMovementEnabled = true;
        Services.gun.enabled = true;

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
        if (position.x > Services.levelGenerator.baseLevelSize / 2 ||
            position.x < -Services.levelGenerator.baseLevelSize / 2 ||
            position.z > Services.levelGenerator.baseLevelSize / 2 ||
            position.z < -Services.levelGenerator.baseLevelSize / 2)
        {
            return false;
        }

        else return true;
    }
}
