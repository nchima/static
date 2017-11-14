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


    private void Start() {
        playerSpawnPoint = GameObject.Find("Player Spawn Point").transform;
        savedGravity = Physics.gravity;
        normalPlayerBounciness =
            FindObjectOfType<PlayerController>().
            GetComponent<CapsuleCollider>().
            material.bounciness;
    }


    protected override void Update() {
        base.Update();

        // Handle falling.
        //if (playerState != PlayerState.Normal) {
        //    switch (playerState) {
        //        case PlayerState.PauseAfterLevelComplete:
        //            PauseAfterLevelComplete();
        //            break;
        //        case PlayerState.FallingIntoLevel:
        //            FallIntoLevel();
        //            break;
        //        case PlayerState.FiringShockwave:
        //            FireShockwave();
        //            break;
        //    }
        //}
    }


    void PauseAfterLevelComplete() {
        fallingSequenceTimer += Time.deltaTime;
        //Debug.Log(fallingTimer);

        if (fallingSequenceTimer >= pauseAfterLevelCompleteLength)
        {
            GameManager.instance.ReturnToFullSpeed();
            player.GetComponent<PlayerController>().state = PlayerController.State.Falling;
            player.GetComponent<Collider>().material.bounciness = 0f;
            Physics.gravity = savedGravity;
            isSpeedFallActive = false;
            GameManager.instance.forceInvincibility = false;

            // Generate new level.
            if (!GameManager.instance.dontChangeLevel && GameManager.levelManager.isLevelCompleted /* && !startMidFall*/) { Debug.Log("Generating");  GameManager.instance.LoadNextLevel(); }

            // Place the player in the correct spot above the level.
            player.transform.position = new Vector3(player.transform.position.x, playerSpawnPoint.position.y, player.transform.position.z);

            // Re-enable the floor's collision (since it is disabled when the player completes a level.)
            GameManager.instance.SetFloorCollidersActive(true);

            // Update billboards.
            GameManager.instance.GetComponent<BatchBillboard>().FindAllBillboards();

            // Set up variables for falling.
            playerTouchedDown = false;
            savedRegularMoveSpeed = player.GetComponent<PlayerController>().maxAirSpeed;
            player.GetComponent<PlayerController>().maxAirSpeed = playerMoveSpeedWhenFalling;
            playerState = PlayerState.FallingIntoLevel;

            // Begin rotating player camera to face down.
            GameObject.Find("Cameras").transform.DOLocalRotate(new Vector3(90f, 0f, 0f), 0.75f, RotateMode.Fast);

            // Begin falling sequence.
            GameManager.instance.FreezeSpecialBarDecay(true);
        }
    }


    void FallIntoLevel() {

        // Player can activate speed fall by pressing fire.
        if (!isSpeedFallActive && Input.GetButtonDown("Fire1")) {
            ActivateSpeedFall();
        }

        if (player.transform.position.y <= 600f) {
            GameManager.instance.scoreManager.HideLevelCompleteScreen();
        }

        // See if the player has touched down.
        if (Physics.Raycast(player.transform.position, Vector3.down, 7f)) {

            GameManager.instance.scoreManager.HideLevelCompleteScreen();
            GameManager.instance.CountEnemies();

            // Set up bonus time for next level.
            GameManager.instance.DetermineBonusTime();

            GameManager.specialBarManager.freezeDecay = false;

            player.transform.position = new Vector3(player.transform.position.x, 2.11f, player.transform.position.z);

            // Begin rotating camera back to regular position.
            player.transform.Find("Cameras").transform.DOLocalRotate(new Vector3(0f, 0f, 0f), lookUpSpeed * 0.6f, RotateMode.Fast);

            // Reset movement variables.
            //player.GetComponent<Rigidbody>().isKinematic = true;
            player.GetComponent<PlayerController>().state = PlayerController.State.Normal;
            player.GetComponent<PlayerController>().maxAirSpeed = savedRegularMoveSpeed;

            fallingSequenceTimer = 0f;

            if (isSpeedFallActive) InstantiateShockwave(shockwavePrefab, GameManager.instance.gun.burstsPerSecondModifierMax);

            playerState = PlayerState.FiringShockwave;
        }
    }


    void FireShockwave() {
        fallingSequenceTimer += Time.deltaTime;
        if (fallingSequenceTimer >= lookUpSpeed)
        {
            GameManager.instance.ReturnToFullSpeed();

            GameManager.instance.gun.canShoot = true;

            // Allow enemies to start attacking.
            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy")) {
                enemy.GetComponent<Enemy>().willAttack = true;
            }

            // Destroy any obstacles that the player is touching.
            Collider[] overlappingSolids = Physics.OverlapCapsule(
                player.transform.position,
                player.transform.position + Vector3.down * 10f,
                player.GetComponent<CapsuleCollider>().radius,
                1 << 8);

            for (int i = 0; i < overlappingSolids.Length; i++) {
                if (overlappingSolids[i].tag == "Obstacle") {
                    overlappingSolids[i].GetComponent<Obstacle>().DestroyByPlayerFalling();
                }
            }

            // Begin moving obstacles to their full height.
            GameObject.Find("Obstacles").transform.DOMoveY(0f, 0.18f, false);

            Physics.gravity = savedGravity;

            isSpeedFallActive = false;

            GameManager.instance.forceInvincibility = false;

            player.GetComponent<Collider>().material.bounciness = normalPlayerBounciness;

            playerState = PlayerState.Normal;
        }
    }


    public void InstantiateShockwave(GameObject prefab, float gunRate) {
        // Begin tweening the time scale towards slow-motion. (Also lower music pitch.)
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.1f, 0.1f).SetEase(Ease.InQuad).SetUpdate(true);
        FindObjectOfType<MusicManager>().GetComponent<AudioSource>().DOPitch(0.1f, 0.1f).SetUpdate(true);

        // Re-enable gun and begin tweening its burst rate to quick-fire. (This allows the player to fire more quickly during slow motion.
        GameManager.instance.gun.canShoot = true;
        DOTween.To
            (() => GameManager.instance.gun.burstsPerSecondModifier, x => GameManager.instance.gun.burstsPerSecondModifier = x, gunRate, 0.1f).SetEase(Ease.InQuad).SetUpdate(true);

        Vector3 shockwavePosition = GameManager.player.transform.position;
        shockwavePosition.y = 0f;
        Instantiate(prefab, shockwavePosition, Quaternion.identity);
    }


    public void ActivateSpeedFall() {
        if (GameManager.player.GetComponent<PlayerController>().state == PlayerController.State.SpeedFalling) { return; }
        //player.GetComponent<Rigidbody>().isKinematic = false;
        GameManager.player.GetComponent<Rigidbody>().AddForce(Vector3.down * 600f, ForceMode.VelocityChange);
        GameManager.player.GetComponent<PlayerController>().state = PlayerController.State.SpeedFalling;
        //Physics.gravity *= speedFallGravityMultipier;
        isSpeedFallActive = true;
        GameManager.instance.forceInvincibility = true;
    }


    public void BeginFalling() {
        fallingTrigger.isTriggerSet = true;
    }


    public void BeginFallingInstant() {
        TransitionToState(GetComponentInChildren<FallIntoLevelState>());
    }
}
