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

    [HideInInspector] public bool playerTouchedDown = false;

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
    }

    protected override void Update() {
        base.Update();
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

    public void ActivateSpeedFall() {
        if (Services.playerController.state == PlayerController.State.SpeedFalling) { return; }
        //player.GetComponent<Rigidbody>().isKinematic = false;
        Services.playerGameObject.GetComponent<Rigidbody>().AddForce(Vector3.down * 600f, ForceMode.VelocityChange);
        Services.playerController.state = PlayerController.State.SpeedFalling;
        //Physics.gravity *= speedFallGravityMultipier;
        isSpeedFallActive = true;
        Services.healthManager.forceInvincibility = true;
    }

    public void BeginFalling() {
        fallingTrigger.isTriggerSet = true;
    }

    public void BeginFallingInstant() {
        TransitionToState(GetComponentInChildren<FallIntoLevelState>());
    }

    public void LevelCompletedHandler(GameEvent gameEvent) {
        BeginFalling();
    }
}
