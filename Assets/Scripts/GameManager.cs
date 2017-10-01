using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using DG.Tweening;

public class GameManager : MonoBehaviour {

    // DEBUG STUFF
    [SerializeField] bool dontChangeLevel;
    [SerializeField] bool startMidFall;
    [HideInInspector] public bool invincible;
    [SerializeField] public bool forceInvincibility;
    [SerializeField] float invincibilityTime = 0.5f;
    float invincibilityTimer = 0f;

    // AUDIO
    [SerializeField] private AudioSource levelWinAudio;   // The audio source that plays when the player completes a level.

    // USED FOR LEVEL GENERATION
    public int levelNumber = 0;    // The current level.
    int numberOfEnemies = 2;    // The number of enemies that spawned in the current level.
    public int currentEnemyAmt;    // The number of enemies currently alive in this level.
    public LevelGenerator levelGenerator;  // A reference to the level generator script.

    // USED FOR FALLING INTO THE NEXT LEVEL
    [HideInInspector] public enum PlayerState { Normal, PauseAfterLevelComplete, FallingIntoLevel, FiringShockwave };
    [HideInInspector] public PlayerState playerState = PlayerState.Normal;
    float pauseAfterLevelCompleteLength = 1.5f;
    float fallingSequenceTimer;
    float lookUpSpeed = 0.25f;
    float playerMoveSpeedWhenFalling = 50f;
    float savedRegularMoveSpeed;
    Vector3 savedGravity;
    float speedFallGravityMultipier = 10f;
    bool speedFallActivated = false;
    [SerializeField] GameObject shockwavePrefab;
    [HideInInspector] public bool playerTouchedDown = false;
    Transform playerSpawnPoint;

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
    public static GameManager instance;
    Transform floor;    // The floor of the game environment.
    ScoreManager scoreManager;
    SpecialBarManager specialBarManager;
    HealthManager healthManager;
    [HideInInspector] public Gun gun;
    [HideInInspector] public GenerateNoise noiseGenerator;
    [HideInInspector] public GameObject player;
    [SerializeField] GameObject gunSliderBorder;


    void Awake()
    {
        instance = this;

        // Pause everything for the main menu.
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<Enemy>().enabled = false;
        }

        GameObject.Find("FPSController").GetComponent<PlayerController>().enabled = false;
        foreach(Gun gun in FindObjectsOfType<Gun>())
        {
            gun.enabled = false;
        }
    }


    private void Start()
    {
        playerSpawnPoint = GameObject.Find("Player Spawn Point").transform;

        // Set up the current number of enemies.
        currentEnemyAmt = numberOfEnemies;

        //currentSine = Mathf.Sin(Time.time * oscSpeed);
        currentSine = Mathf.Sin(sineTime);

        savedGravity = Physics.gravity;

        // Get references
        floor = GameObject.Find("Floor").transform;
        scoreManager = GetComponent<ScoreManager>();
        specialBarManager = GetComponent<SpecialBarManager>();
        healthManager = GetComponent<HealthManager>();
        levelGenerator = GetComponent<LevelGenerator>();
        gun = FindObjectOfType<Gun>();
        noiseGenerator = GetComponent<GenerateNoise>();
        player = GameObject.Find("FPSController");

        scoreManager.DetermineBonusTime();

        if (gunMethod == GunMethod.TuningBased)
        {
            FindObjectOfType<CrossHair>().tuningTarget.gameObject.SetActive(true);
            GetNewTuningTarget();
        }

        if (startMidFall)
        {
            fallingSequenceTimer = 0f;
            playerState = PlayerState.PauseAfterLevelComplete;

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

        gun.shotgunChargeIsReady = currentSine <= -1f + gun.specialMoveSineRange && specialBarManager.barIsFull;
        gun.missilesAreReady = currentSine >= 1f - gun.specialMoveSineRange && specialBarManager.barIsFull;

        if ((gun.shotgunChargeIsReady || gun.missilesAreReady))
        {
            specialBarManager.FlashBar();
        }

        if (gunMethod == GunMethod.TuningBased && specialBarManager.barIsFull)
        {
            gun.missilesAreReady = true;
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

        // Handle falling.
        if (playerState != PlayerState.Normal)
        {
            switch (playerState)
            {
                case PlayerState.PauseAfterLevelComplete:
                    PauseAfterLevelComplete();
                    break;
                case PlayerState.FallingIntoLevel:
                    FallIntoLevel();
                    break;
                case PlayerState.FiringShockwave:
                    FireShockwave();
                    break;
            }
        }
    }


    public void PlayerUsedSpecialMove()
    {
        specialBarManager.PlayerUsedSpecialMove();

        if (gunMethod == GunMethod.TuningBased) GetNewTuningTarget();
    }


    public void BeginShotgunCharge()
    {
        invincible = true;
        player.GetComponentInChildren<ShotgunCharge>().BeginCharge();
        player.GetComponent<FirstPersonController>().isDoingShotgunCharge = true;
    }


    public void CompleteShotgunCharge()
    {
        player.GetComponent<FirstPersonController>().isDoingShotgunCharge = false;
        player.GetComponentInChildren<ShotgunCharge>().EndCharge();
    }


    void PauseAfterLevelComplete()
    {
        fallingSequenceTimer += Time.deltaTime;
        //Debug.Log(fallingTimer);

        if (fallingSequenceTimer >= pauseAfterLevelCompleteLength)
        {
            ReturnToFullSpeed();

            player.GetComponent<PlayerController>().state = PlayerController.State.Falling;

            Physics.gravity = savedGravity;

            speedFallActivated = false;

            forceInvincibility = false;

            // Generate new level.
            if (!dontChangeLevel)
            {
                levelNumber += 1;
                levelGenerator.Generate();
            }

            // Place the player in the correct spot above the level.
            player.transform.position = new Vector3(player.transform.position.x, playerSpawnPoint.position.y, player.transform.position.z);

            // Re-enable the floor's collision (since it is disabled when the player completes a level.)
            floor.GetComponent<Collider>().enabled = true;

            // Update billboards.
            GameObject.Find("Game Manager").GetComponent<BatchBillboard>().UpdateBillboards();

            // Set up bonus time for next level.
            scoreManager.DetermineBonusTime();

            // Set up variables for falling.
            playerTouchedDown = false;
            savedRegularMoveSpeed = player.GetComponent<PlayerController>().maxSpeed;
            player.GetComponent<PlayerController>().maxSpeed = playerMoveSpeedWhenFalling;
            playerState = PlayerState.FallingIntoLevel;

            // Begin rotating player camera to face down.
            player.transform.Find("FirstPersonCharacter").transform.DOLocalRotate(new Vector3(90f, 0f, 0f), 0.75f, RotateMode.Fast);

            // Begin falling sequence.
            specialBarManager.freezeDecay = true;
        }
    }


    void FallIntoLevel()
    {
        // Player can activate speed fall by pressing fire.
        if (!speedFallActivated && Input.GetButtonDown("Fire1"))
        {
            //player.GetComponent<Rigidbody>().isKinematic = false;
            player.GetComponent<Rigidbody>().AddForce(Vector3.down * 600f, ForceMode.VelocityChange);
            Physics.gravity *= speedFallGravityMultipier;
            speedFallActivated = true;
            forceInvincibility = true;
        }

        if (player.transform.position.y <= 600f)
        {
            scoreManager.HideLevelCompleteScreen();
            floor.GetComponent<Collider>().enabled = true;
        }

        // See if the player has touched down.
        if (player.transform.position.y <= 2.2f)
        {
            scoreManager.HideLevelCompleteScreen();

            player.transform.position = new Vector3(player.transform.position.x, 2.11f, player.transform.position.z);

            // Begin rotating camera back to regular position.
            player.transform.Find("FirstPersonCharacter").transform.DOLocalRotate(new Vector3(0f, 0f, 0f), lookUpSpeed*0.6f, RotateMode.Fast);

            // Reset movement variables.
            //player.GetComponent<Rigidbody>().isKinematic = true;
            player.GetComponent<PlayerController>().state = PlayerController.State.Normal;
            player.GetComponent<PlayerController>().maxSpeed = savedRegularMoveSpeed;

            fallingSequenceTimer = 0f;

            if (speedFallActivated) InstantiateShockwave(shockwavePrefab, gun.burstsPerSecondModifierMax);

            specialBarManager.freezeDecay = true;

            playerState = PlayerState.FiringShockwave;
        }
    }


    public void InstantiateShockwave(GameObject prefab, float gunRate)
    {
        // Begin tweening the time scale towards slow-motion. (Also lower music pitch.)
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.1f, 0.1f).SetEase(Ease.InQuad).SetUpdate(true);
        FindObjectOfType<MusicManager>().GetComponent<AudioSource>().DOPitch(0.1f, 0.1f).SetUpdate(true);

        // Re-enable gun and begin tweening its burst rate to quick-fire. (This allows the player to fire more quickly during slow motion.
        gun.canShoot = true;
        DOTween.To(() => gun.burstsPerSecondModifier, x => gun.burstsPerSecondModifier = x, gunRate, 0.1f).SetEase(Ease.InQuad).SetUpdate(true);

        Vector3 shockwavePosition = player.transform.position;
        shockwavePosition.y = 0f;
        Instantiate(prefab, shockwavePosition, Quaternion.identity);
    }


    public void ReturnToFullSpeed()
    {
        // Begin tweening time scale, gun burst rate, and music pitch back to normal.
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
        DOTween.To(() => gun.burstsPerSecondModifier, x => gun.burstsPerSecondModifier = x, 1f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
        FindObjectOfType<MusicManager>().GetComponent<AudioSource>().DOPitch(1f, 1f).SetUpdate(true);
    }


    void FireShockwave()
    {
        fallingSequenceTimer += Time.deltaTime;
        if (fallingSequenceTimer >= lookUpSpeed)
        {
            ReturnToFullSpeed();

            gun.canShoot = true;

            // Allow enemies to start attacking.
            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                enemy.GetComponent<Enemy>().willAttack = true;
            }

            // Destroy any obstacles that the player is touching.
            Collider[] overlappingSolids = Physics.OverlapCapsule(
                player.transform.position, 
                player.transform.position + Vector3.down * 10f, 
                player.GetComponent<CapsuleCollider>().radius, 
                1 << 8);

            for (int i = 0; i < overlappingSolids.Length; i++)
            {
                if (overlappingSolids[i].tag == "Obstacle")
                {
                    overlappingSolids[i].GetComponent<Obstacle>().DestroyByPlayerFalling();
                }
            }

            // Begin moving obstacles to their full height.
            GameObject.Find("Obstacles").transform.DOMoveY(0f, 0.18f, false);

            Physics.gravity = savedGravity;

            speedFallActivated = false;

            forceInvincibility = false;

            playerState = PlayerState.Normal;
        }
    }


    public void PlayerKilledEnemy(int enemyKillValue)
    {
        // See if the player has killed all the enemies in this level. If so, change the level.
        //if (gunMethod == GunMethod.TuningBased && (currentSine < currentIdealRange.min || currentSine > currentIdealRange.max)) { }
        //else
        //{
            scoreManager.PlayerKilledEnemy(enemyKillValue);
            specialBarManager.PlayerKilledEnemy();
        //}

        currentEnemyAmt -= 1;
        //Debug.Log("Current enemy amount: " + currentEnemyAmt + ". Time: " + Time.time);

        if (currentEnemyAmt <= 0)
        {
            LevelComplete();
        }
    }


    public void LevelComplete()
    {
        if (playerState == PlayerState.PauseAfterLevelComplete) return;
        if (dontChangeLevel) return;

        levelWinAudio.Play();

        scoreManager.LevelComplete();

        gun.canShoot = false;

        //if (healthManager.playerHealth < 5) healthManager.playerHealth++;

        // Disable the floor's collider so the player falls through it.
        floor.GetComponent<Collider>().enabled = false;

        // Initiate falling sequence.
        fallingSequenceTimer = 0f;
        playerState = PlayerState.PauseAfterLevelComplete;
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
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<Enemy>().enabled = true;
            enemy.GetComponent<Enemy>().willAttack = true;
        }

        // Enable player movement and shooting.
        GameObject.Find("FPSController").GetComponent<PlayerController>().enabled = true;
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

            newTuningTimer += Time.deltaTime;
            if (newTuningTimer > nextTuningTime)
            {
                GetNewTuningTarget();
                nextTuningTime = newTuningTimeRange.Random;
                newTuningTimer = 0f;
            }
        }

        player.GetComponent<PlayerController>().SetFieldOfView(currentSine);
    }


    void GetNewTuningTarget()
    {
        float newIdealCenter = Random.Range(-1 + idealTuningSize*0.5f, 1 - idealTuningSize*0.5f);
        currentIdealRange = new FloatRange(newIdealCenter - idealTuningSize * 0.5f, newIdealCenter + idealTuningSize * 0.5f);

        FindObjectOfType<CrossHair>().UpdateTuningTarget(newIdealCenter);
    }


    public void UpdateBillboards()
    {
        FindObjectOfType<BatchBillboard>().UpdateBillboards();
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
