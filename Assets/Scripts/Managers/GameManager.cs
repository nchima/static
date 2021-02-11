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
    [SerializeField] private int maxFrameRate = 21;
    public bool isGameStarted = false;
    Vector3 initialGravity;
    const float PAUSE_INPUT_COOLDOWN = 0.2f;
    float pauseInputCooldownTimer = 0.2f;
    [SerializeField] public bool feetEnabled;


    void Awake() {
        // Set up services manager
        Services.gameManager = this;
        Services.playerGameObject = FindObjectOfType<PlayerController>().gameObject;
        Services.playerTransform = FindObjectOfType<PlayerController>().transform;
        Services.playerController = FindObjectOfType<PlayerController>();
        Services.scoreManager = GetComponent<ScoreManager>();
        Services.specialMoveManager = GetComponentInChildren<SpecialMoveManager>();
        Services.specialBarManager = GetComponentInChildren<SpecialBarManager>();
        Services.healthManager = GetComponentInChildren<HealthManager>();
        Services.levelManager = GetComponentInChildren<LevelManager>();
        Services.levelGenerator = GetComponentInChildren<LevelGenerator>();
        Services.fallingSequenceManager = GetComponentInChildren<FallingSequenceManager>();
        Services.fieldOfViewController = FindObjectOfType<FieldOfViewController>();
        Services.sfxManager = GetComponentInChildren<SFXManager>();
        Services.uiManager = GetComponentInChildren<UIManager>();
        Services.musicManager = GetComponentInChildren<MusicManager>();
        Services.colorPaletteManager = GetComponentInChildren<ColorPaletteManager>();
        Services.gun = FindObjectOfType<Gun>();
        Services.gunValueManager = GetComponentInChildren<GunValueManager>();
        Services.noiseGenerator = GetComponent<GenerateNoise>();
        Services.flashManager = GetComponentInChildren<FlashManager>();
        Services.billboardManager = FindObjectOfType<BatchBillboard>();
        Services.scorePopupManager = GetComponentInChildren<ScorePopupManager>();
        Services.extraScreenManager = GetComponentInChildren<ExtraScreenManager>();
        Services.steamLeaderboardManager = GetComponentInChildren<SteamLeaderboardManager>();
        Services.analyticsManager = GetComponentInChildren<AnalyticsManager>();
        Services.comboManager = GetComponentInChildren<ComboManager>();
        Services.timeScaleManager = GetComponentInChildren<TimeScaleManager>();
        Services.finalCamera = GameObject.Find("Final Camera").GetComponent<Camera>();
        Services.taserManager = GetComponentInChildren<TaserManager>();
        Services.screenShakeManager = GetComponentInChildren<ScreenShakeManager>();

        // Limit FPS
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = maxFrameRate;
    }

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameStarted>(GameStartedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerLanded>(PlayerLandedHandler);
        GameEventManager.instance.Subscribe<GameEvents.LevelLoaded>(LevelLoadedHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameStarted>(GameStartedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerLanded>(PlayerLandedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.LevelLoaded>(LevelLoadedHandler);
    }

    private void Start() {
#if UNITY_EDITOR
        // If any scenes aside from the main scene is currently loaded, unload them. This is to make testing levels in the editor more convenient.
        for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++) {
            Scene scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);
            if (scene.name != "Main") {
                SceneManager.UnloadSceneAsync(scene);
            }
        }
#endif

        if (feetEnabled) {
            GameEventManager.instance.FireEvent(new GameEvents.EnableFeet(true));
        }
    }

    public void LoadGame() {
        StartCoroutine(InitialSetup());
    }

    IEnumerator InitialSetup() {
        //Services.gun.enabled = false;
        Services.extraScreenManager.SetScreensActive(true);
        Services.extraScreenManager.SetRotationScale(1f);

        initialGravity = Physics.gravity;
        Physics.gravity = Vector3.zero;

        yield return null;
    }


    private void Update() {
        pauseInputCooldownTimer += Time.unscaledDeltaTime;
        if (!isGamePaused && InputManager.pauseButtonDown && isGameStarted && !Services.healthManager.PlayerIsDead && pauseInputCooldownTimer >= PAUSE_INPUT_COOLDOWN) {
            PauseGame(true);
        }

        // Limit fps
        if (Application.targetFrameRate != maxFrameRate) {
            Application.targetFrameRate = maxFrameRate;
        }
    }

    private bool canShootBeforePause;
    // private bool canUseSpecialBeforePause;
    public static bool isGamePaused;
    public void PauseGame(bool value) {
        pauseInputCooldownTimer = 0f;
        if (value == true) {
            Services.uiManager.ShowPauseScreen();
            Services.timeScaleManager.Pause(true);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            canShootBeforePause = Services.gun.canShoot;
            // canUseSpecialBeforePause = Services.specialMoveManager.canShoot;
            Services.gun.canShoot = false;
            // Services.specialMoveManager.canShoot = false;
            isGamePaused = true;
        }

        else {
            Services.uiManager.HidePauseScreen();
            Services.timeScaleManager.Pause(false);
            if (!isGameStarted) { Services.uiManager.titleScreen.SetActive(true); }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Services.gun.canShoot = canShootBeforePause;
            // Services.specialMoveManager.canShoot = canUseSpecialBeforePause;
            isGamePaused = false;
        }
    }

    // MOVE TO UM... ENEMY MANAGER IF I MAKE ONE
    public void PlayerKilledEnemyHandler(GameEvent gameEvent) {
        // If player has killed all the enemies in the current level, begin the level completion sequence.
        currentEnemyAmt -= 1;
        if (currentEnemyAmt == 0 && !Services.fallingSequenceManager.isPlayerFalling && !dontChangeLevel) {
            GameEventManager.instance.FireEvent(new GameEvents.LevelCompleted());
            Debug.Log("lev compl because of kill");
        }
    }

    public void PlayerLandedHandler(GameEvent gameEvent) {
        if (currentEnemyAmt == 0) {
            GameEventManager.instance.FireEvent(new GameEvents.LevelCompleted());
        }
    }

    // Maybe move this functionality to various managers at some point.
    public void LevelCompletedHandler(GameEvent gameEvent) {
        levelWinAudio.Play();
        DeleteThings();
    }


    void DeleteThings() {
        foreach(Pickup pickup in FindObjectsOfType<Pickup>()) { pickup.Delete(); }
        foreach(PlayerMissile missile in FindObjectsOfType<PlayerMissile>()) { missile.Destroy(); }
    }


    public void CountEnemies() {
        currentEnemyAmt = GameObject.FindGameObjectsWithTag("Enemy").Length;
        Debug.Log("enemy amt: " + currentEnemyAmt);
    }


    public void GameStartedHandler(GameEvent gameEvent) {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Services.uiManager.hud.SetActive(true);
        Services.playerController.isMovementEnabled = true;
        Services.playerController.GetComponent<Rigidbody>().isKinematic = false;
        Services.extraScreenManager.ReturnToZeroAndDeactivate(0.5f);
        Services.fallingSequenceManager.SetUpFallingVariables();
        Services.scoreManager.UpdateHighScoreDisplay();
        Physics.gravity = initialGravity;   // Move to gravity manager
        isGameStarted = true;
    }

    public void LevelLoadedHandler(GameEvent gameEvent) {
        CountEnemies();
    }

    public void RestartGame() {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void PlayerCompletedGame() {
        // This is broken right now... the game will just freeze.
        SceneManager.UnloadSceneAsync(Services.levelManager.currentlyLoadedLevelData.buildIndex);
        Services.uiManager.ShowEndOfDemoScreen();
    }
}
