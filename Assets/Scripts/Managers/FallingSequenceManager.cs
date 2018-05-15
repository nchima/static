using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FallingSequenceManager : StateController {

    [HideInInspector] public enum PlayerState { Normal, PauseAfterLevelComplete, FallingIntoLevel, FiringShockwave };
    [HideInInspector] public PlayerState playerState = PlayerState.Normal;

    [SerializeField] TriggerTransition fallingTrigger;
    public Collider fallCatcher;

    public bool isPlayerFalling {
        get {
            if (currentState.name.ToLower().Contains("fall") || currentState.name.ToLower().Contains("fall")) { return true; }
            else { return false; }
        }
    }
    public bool isStateIdle {
        get {
            if (currentState.name.ToLower().Contains("idle")) { return true; }
            else { return false; }
        }
    }

    [HideInInspector] public float fallingSequenceTimer; // General purpose timer.

    float pauseAfterLevelCompleteLength = 1.5f;
    [HideInInspector] public float lookUpSpeed = 0.25f;  // How quickly the player looks up after touching down.

    [HideInInspector] public float playerMoveSpeedWhenFalling = 50f;
    [HideInInspector] public float savedRegularMoveSpeed;
    [HideInInspector] public Vector3 savedGravity;
    [HideInInspector] public Color savedFogColor;

    float speedFallGravityMultipier = 10f;
    [HideInInspector] public bool isSpeedFallActive = false;

    [HideInInspector] public int timesMissedLevel = 0;

    [HideInInspector] public float normalPlayerBounciness;

    public GameObject shockwavePrefab;

    [HideInInspector] public Transform playerSpawnPoint;
    Transform player;


    private void Awake() {
        playerSpawnPoint = GameObject.Find("Player Spawn Point").transform;
        savedGravity = Physics.gravity;
        normalPlayerBounciness =
            FindObjectOfType<PlayerController>().
            GetComponent<CapsuleCollider>().
            material.bounciness;

        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameStarted>(GameStartedHandler);
    }

    protected override void Update() {
        base.Update();
        if (timesMissedLevel > 2) {
            Services.uiManager.landOnLevelScreen.SetActive(true);
        }
    }

    public void InstantiateShockwave(GameObject prefab, float gunRate) {
        // Begin tweening the time scale towards slow-motion. (Also lower music pitch.)
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.1f, 0.1f).SetEase(Ease.InQuad).SetUpdate(true);
        Services.musicManager.PitchDownMusicForSlowMotion();

        // Re-enable gun and begin tweening its burst rate to quick-fire. (This allows the player to fire more quickly during slow motion.
        Services.gun.canShoot = true;
        DOTween.To
            (() => Services.gun.burstsPerSecondSloMoModifierCurrent, x => Services.gun.burstsPerSecondSloMoModifierCurrent = x, gunRate, 0.1f).SetEase(Ease.InQuad).SetUpdate(true);

        Vector3 shockwavePosition = Services.playerTransform.position;
        shockwavePosition.y = 0f;
        Instantiate(prefab, shockwavePosition, Quaternion.identity);
    }

    public void SetUpFallingVariables() {
        // In case the game is currently running in slow motion, return to full speed.
        if (Time.timeScale != 1f) { Services.gameManager.ReturnToFullSpeed(); }

        // If the player is not currently set to falling state, set them to that state.
        if (Services.playerController.state != PlayerController.State.SpeedFalling) {
            Services.playerController.state = PlayerController.State.Falling;
        }

        Services.gun.canShoot = false;

        Services.playerGameObject.GetComponent<Collider>().material.bounciness = 0f;
        Services.healthManager.forceInvincibility = false;

        // Drain color palette.
        //Services.colorPaletteManager.LoadFallingSequencePalette();

        // Place the player in the correct spot above the level.
        Services.playerTransform.position = playerSpawnPoint.position;

        // Set up variables for falling.
        savedRegularMoveSpeed = Services.playerController.maxAirSpeed;

        // Turn off fog.
        savedFogColor = RenderSettings.fogColor;
        RenderSettings.fogColor = Color.white;

        // Begin rotating player camera to face down.
        GameObject.Find("Cameras").transform.DOLocalRotate(new Vector3(90f, 0f, 0f), 0.75f, RotateMode.Fast);
    }

    public void BeginFalling() {
        TransitionToState(GetComponentInChildren<FallIntoLevelState>());
    }

    public void BeginFallingInstant() {
        TransitionToState(GetComponentInChildren<FallIntoLevelState>());
    }

    public void LevelCompletedHandler(GameEvent gameEvent) {
        fallingTrigger.isTriggerSet = true;
    }

    public void GameStartedHandler(GameEvent gameEvent) {
        BeginFalling();
    }
}
