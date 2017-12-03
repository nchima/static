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
    [HideInInspector] public bool invincible;
    [SerializeField] public bool forceInvincibility;
    [SerializeField] float invincibilityTime = 0.5f;
    float invincibilityTimer = 0f;

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

    // USED FOR IDLE TIMER
    [SerializeField] float idleResetTime = 20f;
    float timeSinceLastInput = 0f;
    public bool gameStarted = false;

    // RANDOM USEFUL STUFF
    Vector3 playerPositionLast;
    [HideInInspector] public Vector3 playerVelocity;

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
    [HideInInspector] public GenerateNoise noiseGenerator;
    [HideInInspector] public static GameObject player;
    [SerializeField] GameObject gunSliderBorder;


    void Awake() {
        // Get various references.
        instance = this;
        player = GameObject.Find("Player");
        player.GetComponent<PlayerController>().enabled = false;
        scoreManager = GetComponent<ScoreManager>();
        specialBarManager = GetComponentInChildren<SpecialBarManager>();
        healthManager = GetComponent<HealthManager>();
        levelGenerator = GetComponent<LevelGenerator>();
        levelManager = GetComponentInChildren<LevelManager>();
        fallingSequenceManager = GetComponentInChildren<FallingSequenceManager>();
        sfxManager = GetComponentInChildren<SFXManager>();
        musicManager = GetComponentInChildren<MusicManager>();
        gun = FindObjectOfType<Gun>();
        gunValueManager = GetComponentInChildren<GunValueManager>();
        noiseGenerator = GetComponent<GenerateNoise>();

        // Disable gun.
        foreach (Gun gun in FindObjectsOfType<Gun>()) {
            gun.enabled = false;
        }
    }


    private void Start() {
        // Set up the current number of enemies.
        currentEnemyAmt = FindObjectsOfType<Enemy>().Length;

        scoreManager.DetermineBonusTime();

        levelManager.LoadLevel(levelManager.currentLevelNumber);

        // Pause everything for the main menu.
        foreach (Enemy enemy in FindObjectsOfType<Enemy>()) {
            enemy.enabled = false;
        }
    }


    private void Update() {
        // Keep track of player velocity.
        playerVelocity = (player.transform.position - playerPositionLast) / Time.deltaTime;
        playerPositionLast = player.transform.position;

        // See if a special move is ready to be fired.
        bool sineInPosition = GunValueManager.currentValue <= -1f + gun.specialMoveSineRange || GunValueManager.currentValue >= 1f - gun.specialMoveSineRange;
        if (sineInPosition) {
            //Debug.Log("Sine in position.");
            gunSliderBorder.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.red, Color.yellow, Random.value);
            //Debug.Log(gunSliderBorder.GetComponent<MeshRenderer>().material);
        }

        else
        {
            gunSliderBorder.GetComponent<MeshRenderer>().material.color = Color.black;
        }

        //gun.shotgunChargeIsReady = GunValueManager.currentGunValue <= 0f && specialBarManager.barIsFull;
        //gun.missilesAreReady = GunValueManager.currentGunValue >= 0f && specialBarManager.barIsFull;

        if ((gun.shotgunChargeIsReady || gun.missilesAreReady)) {
            specialBarManager.FlashBar();
        }

        //if (specialBarManager.barIsFull) {
        //    specialBarManager.FlashBar();
        //}

        //else gunSliderBorder.GetComponent<MeshRenderer>().material.color = Color.black;

        // Run idle timer.
        if (gameStarted)
        {
            if (timeSinceLastInput >= idleResetTime)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            // Check all joystick buttons.
            bool buttonPressed = false;
            for (int i = 0; i < 20; i++)
            {
                if (Input.GetKeyDown("joystick 1 button " + i.ToString()))
                {
                    buttonPressed = true;
                }
            }

            // See if any other buttons or keys have been pressed.
            if (Input.anyKeyDown || buttonPressed || Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                timeSinceLastInput = 0f;
            }

            timeSinceLastInput += Time.deltaTime;
        }

        // Check invincibility frames.
        if (forceInvincibility) invincible = true;
        invincibilityTimer = Mathf.Clamp(invincibilityTimer, 0f, invincibilityTime);
        if (invincibilityTimer > 0)
        {
            invincible = true;
            invincibilityTimer -= Time.deltaTime;
        }
        else if (!forceInvincibility) invincible = false;
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
        FindObjectOfType<MusicManager>().GetComponent<AudioSource>().DOPitch(1f, 1f).SetUpdate(true);
    }


    public void PlayerKilledEnemy(int enemyKillValue) {
        // Add score and special bar values.
        scoreManager.PlayerKilledEnemy(enemyKillValue);
        specialBarManager.AddValue(enemyKillValue);

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
        currentEnemyAmt = FindObjectsOfType<Enemy>().Length;
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
        if (invincible) return;
        invincibilityTimer += invincibilityTime;

        scoreManager.GetHurt();
        specialBarManager.PlayerWasHurt();

        healthManager.playerHealth -= 4;

        // If health is now less than zero, trigger a game over.
        if (healthManager.playerHealth <= 0)
        {
            ShowGameOverScreen();
        }
    }


    public void ShowHighScores() {
        scoreManager.LoadScoresForHighScoreScreen();

        gameOverScreen.gameObject.SetActive(false);
        nameEntryScreen.gameObject.SetActive(false);
        highScoreScreen.gameObject.SetActive(true);
    }


    public void BulletHitEnemy() {
        //if (gunMethod == GunMethod.TuningBased && (currentSine < currentIdealRange.min || currentSine > currentIdealRange.max)) return;
        scoreManager.BulletHitEnemy();
        specialBarManager.AddValue(0.01f);
        //sineTime += bulletHitSineIncrease;
    }


    void ShowGameOverScreen() {
        gameOverScreen.SetActive(true);
    }


    public void StartGame() {
        CountEnemies();

        // Unpause enemies in the background.
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            enemy.enabled = true;
            enemy.willAttack = true;
        }

        // Enable player movement and shooting.
        GameObject.Find("Player").GetComponent<PlayerController>().enabled = true;
        foreach (Gun gun in FindObjectsOfType<Gun>())
        {
            gun.enabled = true;
        }

        gameStarted = true;
    }


    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
