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

    // USED FOR SINE TRACKER
    public enum GunMethod { TimeBased, MovementBasedQuick, MovementBasedGradual, FloorBased, RotationBased, TuningBased };
    public GunMethod gunMethod;
    [HideInInspector] public float currentSine;
    public float oscSpeed = 0.3f;
    [SerializeField] float bulletHitSineIncrease = 0.01f;
    float sineTime = 0.0f;

    // Used only for tuning method:
    [SerializeField] float idealTuningSize = 0.3f;  // How 'big' the range will be in which the gun is considered to be ideally tuned.
    FloatRange currentIdealRange;   // A gun value in this range will be considered 'in tune'.
    float tuningSpeed = 0.01f;  // How quickly the player can tune the gun. (Not used in case of mouse)
    float newTuningTimer = 0f;
    FloatRange newTuningTimeRange = new FloatRange(7f, 16f);
    float nextTuningTime;

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
    [HideInInspector] public Gun gun;
    [HideInInspector] public GenerateNoise noiseGenerator;
    [HideInInspector] public static GameObject player;
    [SerializeField] GameObject gunSliderBorder;


    void Awake()
    {
        instance = this;

        // Pause everything for the main menu.
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            enemy.enabled = false;
        }

        player = GameObject.Find("Player");
        player.GetComponent<PlayerController>().enabled = false;

        foreach (Gun gun in FindObjectsOfType<Gun>())
        {
            gun.enabled = false;
        }
    }


    private void Start()
    {
        // Set up the current number of enemies.
        currentEnemyAmt = FindObjectsOfType<Enemy>().Length;

        //currentSine = Mathf.Sin(Time.time * oscSpeed);
        currentSine = Mathf.Sin(sineTime);

        // Get references
        scoreManager = GetComponent<ScoreManager>();
        specialBarManager = GetComponent<SpecialBarManager>();
        healthManager = GetComponent<HealthManager>();
        levelGenerator = GetComponent<LevelGenerator>();
        levelManager = GetComponentInChildren<LevelManager>();
        fallingSequenceManager = GetComponentInChildren<FallingSequenceManager>();
        musicManager = GetComponentInChildren<MusicManager>();
        gun = FindObjectOfType<Gun>();
        noiseGenerator = GetComponent<GenerateNoise>();

        scoreManager.DetermineBonusTime();

        levelManager.LoadLevel(levelManager.currentLevelNumber);

        if (gunMethod == GunMethod.TuningBased)
        {
            FindObjectOfType<CrossHair>().tuningTarget.gameObject.SetActive(true);
            GetNewTuningTarget();
        }
    }


    private void Update()
    {
        // Keep track of player velocity.
        playerVelocity = (player.transform.position - playerPositionLast) / Time.deltaTime;
        playerPositionLast = player.transform.position;

        UpdateGunSine();

        // See if a special move is ready to be fired.
        bool sineInPosition = currentSine <= -1f + gun.specialMoveSineRange || currentSine >= 1f - gun.specialMoveSineRange;
        if (sineInPosition)
        {
            //Debug.Log("Sine in position.");
            gunSliderBorder.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.red, Color.yellow, Random.value);
            //Debug.Log(gunSliderBorder.GetComponent<MeshRenderer>().material);
        }

        else
        {
            gunSliderBorder.GetComponent<MeshRenderer>().material.color = Color.black;
        }

        gun.shotgunChargeIsReady = currentSine <= 0f && specialBarManager.barIsFull;
        gun.missilesAreReady = currentSine >= 0f && specialBarManager.barIsFull;

        if ((gun.shotgunChargeIsReady || gun.missilesAreReady))
        {
            specialBarManager.FlashBar();
        }

        if (gunMethod == GunMethod.TuningBased && specialBarManager.barIsFull)
        {
            specialBarManager.FlashBar();
        }

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


    public void LoadNextLevel()
    {
        levelManager.LoadNextLevel();
    }


    public void PlayerUsedSpecialMove() {
        specialBarManager.PlayerUsedSpecialMove();
        if (gunMethod == GunMethod.TuningBased) GetNewTuningTarget();
    }


    public void BeginShotgunCharge() {
        invincible = true;
        player.GetComponentInChildren<ShotgunCharge>().BeginCharge();
        player.GetComponent<PlayerController>().state = PlayerController.State.ShotgunCharge;
    }


    public void CompleteShotgunCharge() {
        player.GetComponent<PlayerController>().state = PlayerController.State.Normal;
        player.GetComponentInChildren<ShotgunCharge>().EndCharge();
    }


    public void ReturnToFullSpeed()
    {
        // Begin tweening time scale, gun burst rate, and music pitch back to normal.
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
        DOTween.To(() => gun.burstsPerSecondModifier, x => gun.burstsPerSecondModifier = x, 1f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
        FindObjectOfType<MusicManager>().GetComponent<AudioSource>().DOPitch(1f, 1f).SetUpdate(true);
    }


    public void PlayerKilledEnemy(int enemyKillValue)
    {
        // Add score and special bar values.
        scoreManager.PlayerKilledEnemy(enemyKillValue);
        specialBarManager.PlayerKilledEnemy();

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


    public void CountEnemies()
    {
        currentEnemyAmt = FindObjectsOfType<Enemy>().Length;
        currentEnemyAmt = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }


    public void FreezeSpecialBarDecay(bool value)
    {
        specialBarManager.freezeDecay = value;
    }


    public void DetermineBonusTime()
    {
        scoreManager.DetermineBonusTime();
    }


    public void SetFloorCollidersActive(bool value)
    {
        levelManager.SetFloorCollidersActive(value);
    }


    public void PlayerWasHurt()
    {
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


    public void ShowHighScores()
    {
        scoreManager.LoadScoresForHighScoreScreen();

        gameOverScreen.gameObject.SetActive(false);
        nameEntryScreen.gameObject.SetActive(false);
        highScoreScreen.gameObject.SetActive(true);
    }


    public void BulletHitEnemy()
    {
        //if (gunMethod == GunMethod.TuningBased && (currentSine < currentIdealRange.min || currentSine > currentIdealRange.max)) return;
        scoreManager.BulletHitEnemy();
        specialBarManager.BulletHitEnemy();
        //sineTime += bulletHitSineIncrease;
    }


    void ShowGameOverScreen()
    {
        gameOverScreen.SetActive(true);
    }


    public void StartGame()
    {
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


    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void SpecialMoveReady()
    {
        
    }

    [SerializeField] GameObject wiper;
    float previousSine = 0f;
    void UpdateGunSine()
    {
        // Update sine
        if (gunMethod == GunMethod.TimeBased)
        {
            sineTime += Time.deltaTime;
            currentSine = Mathf.Sin(sineTime * oscSpeed);
        }

        else if (gunMethod == GunMethod.MovementBasedQuick)
            currentSine = Mathf.Lerp(currentSine, (Input.GetAxis("Horizontal") * MyMath.Map(Mathf.Abs(Input.GetAxis("Vertical")), 0f, 1f, 1f, 0.5f)), 0.5f);

        else if (gunMethod == GunMethod.MovementBasedGradual)
        {
            if (Input.GetAxis("Horizontal") != 0) currentSine += Input.GetAxis("Horizontal") * 0.05f;
            else currentSine = Mathf.Lerp(currentSine, 0f, 0.05f);
            //else if (currentSine > 0) currentSine -= 0.04f;
            currentSine = Mathf.Clamp(currentSine, -1f, 1f);
        }

        else if (gunMethod == GunMethod.RotationBased)
            currentSine = MyMath.Map(player.transform.rotation.eulerAngles.y, 0f, 360f, -1f, 1);

        else if (gunMethod == GunMethod.FloorBased)
        {
            // Raycast from player downwards to floor.
            RaycastHit hit;
            if (Physics.Raycast(player.transform.position, Vector3.down, out hit, 500f, 1 << 20))
            {
                //Debug.Log("Down ray: " + hit.collider.name);
                if (hit.collider.GetComponent<FloorTile>() != null)
                {
                    currentSine = hit.collider.GetComponent<FloorTile>().shotgunValue;
                }
            }
        }

        else if (gunMethod == GunMethod.TuningBased)
        {
            // Control tuning via my penis.
            //if (Input.GetButton("Gun Tuning"))
            //{
            //currentSine += Input.GetAxis("Mouse X") * 0.1f;

            //currentSine += Input.GetAxis("Vertical") * -0.05f;

            currentSine += Input.GetAxis("Mouse Y") * 0.1f;
            currentSine = Mathf.Clamp(currentSine, -1f, 1f);
            //}

            if (Mathf.Abs(previousSine - currentSine) >= 0.01f) {
                wiper.transform.localScale = new Vector3(
                    MyMath.Map(Mathf.Abs(previousSine - currentSine), 0f, 0.5f, 0f, 10f),
                    wiper.transform.localScale.y,
                    wiper.transform.localScale.z
                );
            } else {
                wiper.transform.localScale = new Vector3(
                        0f,
                        wiper.transform.localScale.y,
                        wiper.transform.localScale.z
                    );
            }

            wiper.transform.localPosition = new Vector3(
                wiper.transform.localPosition.x,
                MyMath.Map(currentSine, -1f, 1f, -25f, 11f),
                wiper.transform.localPosition.z
                );

            previousSine = currentSine;

            newTuningTimer += Time.deltaTime;
            if (newTuningTimer > nextTuningTime)
            {
                GetNewTuningTarget();
                nextTuningTime = newTuningTimeRange.Random;
                newTuningTimer = 0f;
            }
        }

        musicManager.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.SetFloat("FilterCutoff", (currentSine + 1f) * 11000f + 200f);
        musicManager.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer
            .SetFloat("FilterCutoff", MyMath.Map(currentSine, -1f, 1f, 10000f, 20000f));

        player.GetComponent<PlayerController>().SetFieldOfView(currentSine);
    }


    void GetNewTuningTarget()
    {
        float newIdealCenter = Random.Range(-1 + idealTuningSize*0.5f, 1 - idealTuningSize*0.5f);
        currentIdealRange = new FloatRange(newIdealCenter - idealTuningSize * 0.5f, newIdealCenter + idealTuningSize * 0.5f);

        //FindObjectOfType<CrossHair>().UpdateTuningTarget(newIdealCenter);
    }


    public void UpdateBillboards()
    {
        FindObjectOfType<BatchBillboard>().FindAllBillboards();
    }


    public bool PositionIsInLevelBoundaries(Vector3 position)
    {
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
