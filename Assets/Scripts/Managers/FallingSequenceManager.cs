using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FallingSequenceManager : StateController {

    public GameObject shockwavePrefab;
    [SerializeField] TriggerTransition fallingTrigger;

    [HideInInspector] public enum PlayerState { Normal, PauseAfterLevelComplete, FallingIntoLevel, FiringShockwave };
    [HideInInspector] public PlayerState playerState = PlayerState.Normal;

    public bool isPlayerFalling {
        get {
            if (currentState.name.ToLower().Contains("fall")) { return true; }
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

    [HideInInspector] public Transform playerSpawnPoint;
    Transform player;

    public bool PlayerIsWithinLevelBoundsOnXZAxes {
        get {
            bool returnValue = false;

            // See if the player is over a floor tile.
            RaycastHit hit1;
            RaycastHit hit2;
            RaycastHit hit3;
            RaycastHit hit4;

            float colliderRadius = Services.playerController.GetComponent<CapsuleCollider>().radius;
            float distance = 200f;

            // If we didn't find anything, return false.
            if (!Physics.Raycast(Services.playerTransform.position + Services.playerTransform.forward * colliderRadius * 0.9f, Vector3.down, out hit1, distance, (1 << 20 | 1 << 24))) { return false; }
            if (!Physics.Raycast(Services.playerTransform.position + Services.playerTransform.forward * -colliderRadius * 0.9f, Vector3.down, out hit2, distance, (1 << 20 | 1 << 24))) { return false; }
            if (!Physics.Raycast(Services.playerTransform.position + Services.playerTransform.forward * colliderRadius * 0.9f, Vector3.up, out hit3, distance, (1 << 20 | 1 << 24))) { return false; }
            if (!Physics.Raycast(Services.playerTransform.position + Services.playerTransform.forward * -colliderRadius * 0.9f, Vector3.up, out hit4, distance, (1 << 20 | 1 << 24))) { return false; }

            // If both things hit something and it was the floor, we're all good baby!
            if ((hit1.transform.name.ToLower().Contains("floor") && hit2.transform.name.ToLower().Contains("floor")) ||
                (hit3.transform.name.ToLower().Contains("floor") && hit4.transform.name.ToLower().Contains("floor"))) {
                returnValue = true;
            }

            // If it wasn't the floor, return false.
            else {
                return false;
            }

            return returnValue;

        }
    }

    private void Awake() {
        playerSpawnPoint = GameObject.Find("Player Spawn Point").transform;
        savedGravity = Physics.gravity;
        normalPlayerBounciness =
            FindObjectOfType<PlayerController>().
            GetComponent<CapsuleCollider>().
            material.bounciness;
    }

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameStarted>(GameStartedHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameStarted>(GameStartedHandler);
    }

    protected override void Update() {
        base.Update();
        if (timesMissedLevel > 2) {
            Services.uiManager.landOnLevelScreen.SetActive(true);
        }
    }

    public void InstantiateShockwave(GameObject prefab, float gunRate, float duration) {
        // Begin tweening the time scale towards slow-motion. (Also lower music pitch.)
        Services.timeScaleManager.TweenTimeScale(0.1f, duration * 0.4f);

        Services.musicManager.PitchDownMusicForSlowMotion(duration);

        // Re-enable gun and begin tweening its burst rate to quick-fire. (This allows the player to fire more quickly during slow motion.
        Services.gun.canShoot = true;
        Services.gun.fireOnEveryMouseDown = true;
        DOTween.To
            (() => Services.gun.burstsPerSecondSloMoModifierCurrent, x => Services.gun.burstsPerSecondSloMoModifierCurrent = x, gunRate, duration * 0.4f).SetEase(Ease.InQuad).SetUpdate(true);

        Services.specialMoveManager.canShoot = true;

        Vector3 shockwavePosition = Services.playerTransform.position;
        shockwavePosition.y = 0f;
        Instantiate(prefab, shockwavePosition, Quaternion.identity);
    }

    public void SetUpFallingVariables() {
        // In case the game is currently running in slow motion, return to full speed.
        if (!Services.timeScaleManager.IsAtFullSpeed) { Services.timeScaleManager.ReturnToFullSpeed(1f); }

        // If the player is not currently set to falling state, set them to that state.
        if (Services.playerController.state != PlayerController.State.SpeedFalling) {
            Services.playerController.state = PlayerController.State.Falling;
        }

        Services.gun.canShoot = false;
        Services.specialMoveManager.canShoot = false;

        Services.playerGameObject.GetComponent<Collider>().material.bounciness = 0f;
        Services.healthManager.forceInvincibility = false;

        // Drain color palette.
        //Services.colorPaletteManager.LoadFallingSequencePalette();

        // Place the player in the correct spot above the level and reset their rotation.
        Services.playerTransform.position = playerSpawnPoint.position;
        Services.playerController.skipRotationForThisFrame = true;
        Vector3 newPlayerRotation = Services.playerTransform.rotation.eulerAngles;
        newPlayerRotation.y = 0f;
        Services.playerTransform.rotation = Quaternion.identity;

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

    public void LevelCompletedHandler(GameEvent gameEvent) {
        fallingTrigger.isTriggerSet = true;
    }

    public void GameStartedHandler(GameEvent gameEvent) {
        BeginFalling();
    }
}
