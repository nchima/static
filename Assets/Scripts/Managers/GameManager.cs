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
        Services.billboardManager = FindObjectOfType<BatchBillboard>();

        GameEventManager.instance.Subscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameStarted>(GameStartedHandler);
    }


    private void Start() {
        StartCoroutine(InitialSetup());
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


    private void Update() {
        if (!gameStarted && !gamePaused) {
            if (InputManager.fireButtonDown) {
                GameEventManager.instance.FireEvent(new GameEvents.GameStarted());
            }
        }

        if (InputManager.pauseButtonDown) {
            if (!gamePaused && !Services.healthManager.PlayerIsDead) { PauseGame(true); } else { PauseGame(false); }
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


    // MOVE TO TIME SCALE MANAGER
    public void ReturnToFullSpeed() {
        // Begin tweening time scale, gun burst rate, and music pitch back to normal.
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
        DOTween.To(() => Services.gun.burstsPerSecondSloMoModifierCurrent, x => Services.gun.burstsPerSecondSloMoModifierCurrent = x, 1f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
        Services.musicManager.ReturnMusicPitchToFullSpeed();
    }


    // MOVE TO UM... ENEMY MANAGER IF I MAKE ONE
    public void PlayerKilledEnemyHandler(GameEvent gameEvent) {
        // If player has killed all the enemies in the current level, begin the level completion sequence.
        currentEnemyAmt -= 1;
        if (currentEnemyAmt <= 0 && !Services.fallingSequenceManager.isPlayerFalling && !dontChangeLevel) {
            GameEventManager.instance.FireEvent(new GameEvents.LevelCompleted());
        }        
    }


    // Maybe move this functionality to various managers at some point.
    public void LevelCompletedHandler(GameEvent gameEvent) {
        levelWinAudio.Play();
        DeleteRemainingPickups();
    }


    void DeleteRemainingPickups() {
        foreach(Pickup pickup in FindObjectsOfType<Pickup>()) { pickup.GetDeleted(); }
    }


    public void CountEnemies() {
        currentEnemyAmt = FindObjectsOfType<EnemyOld>().Length;
        currentEnemyAmt = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }


    public void GameStartedHandler(GameEvent gameEvent) {
        Physics.gravity = initialGravity;
        gameStarted = true;
    }


    public void RestartGame() {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
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
