using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using DG.Tweening;

public class GameManager : MonoBehaviour {

    // DEBUG STUFF
    [HideInInspector] public bool invincible;
    [SerializeField] public bool forceInvincibility;
    [SerializeField] float invincibilityTime = 0.5f;
    float invincibilityTimer = 0f;

    // AUDIO
    [SerializeField] private AudioSource levelWinAudio;   // The audio source that plays when the player completes a level.

    // USED FOR LEVEL GENERATION
    public int levelNumber = 0;    // The current level.
    int numberOfEnemies = 4;    // The number of enemies that spawned in the current level.
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
    [HideInInspector] public float currentSine;
    public float oscSpeed = 0.3f;
    [SerializeField] float bulletHitSineIncrease = 0.01f;
    float sineTime = 0.0f;

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

        GameObject.Find("FPSController").GetComponent<FirstPersonController>().enabled = false;
        foreach(Gun gun in FindObjectsOfType<Gun>())
        {
            gun.enabled = false;
        }
    }


    private void Start()
    {
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
    }


    private void Update()
    {
        // Debug Stuff
        if (Input.GetKeyDown(KeyCode.M)) GetComponentInChildren<MusicManager>().dontPlayMusic = !GetComponentInChildren<MusicManager>().dontPlayMusic;

        // Keep track of player velocity.
        playerVelocity = (player.transform.position - playerPositionLast) / Time.deltaTime;
        playerPositionLast = player.transform.position;

        // Update sine
        sineTime += Time.deltaTime;
        currentSine = Mathf.Sin(sineTime * oscSpeed);

        //currentSine = Mathf.Lerp(currentSine, (Input.GetAxis("Horizontal") * MyMath.Map(Mathf.Abs(Input.GetAxis("Vertical")), 0f, 1f, 1f, 0.5f)), 0.5f);

        //if (Input.GetAxis("Horizontal") != 0) currentSine += Input.GetAxis("Horizontal") * 0.05f;

        //else currentSine = Mathf.Lerp(currentSine, 0f, 0.05f);
        //else if (currentSine > 0) currentSine -= 0.04f;

        currentSine = Mathf.Clamp(currentSine, -1f, 1f);

        //currentSine = MyMath.Map(player.transform.rotation.eulerAngles.y, 0f, 360f, -1f, 1);

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

        if (gun.shotgunChargeIsReady || gun.missilesAreReady)
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

            Physics.gravity = savedGravity;

            speedFallActivated = false;

            forceInvincibility = false;

            // Generate new level.
            levelNumber += 1;
            levelGenerator.Generate();

            // Set up bonus time for next level.
            scoreManager.DetermineBonusTime();

            // Set up variables for falling.
            playerTouchedDown = false;
            savedRegularMoveSpeed = player.GetComponent<FirstPersonController>().m_WalkSpeed;
            player.GetComponent<FirstPersonController>().m_WalkSpeed = playerMoveSpeedWhenFalling;

            // Begin rotating player camera to face down.
            player.transform.Find("FirstPersonCharacter").transform.DOLocalRotate(new Vector3(90f, 0f, 0f), 0.75f, RotateMode.Fast);

            // Begin falling sequence.
            specialBarManager.freezeDecay = true;
            playerState = PlayerState.FallingIntoLevel;
        }
    }


    void FallIntoLevel()
    {
        // Player can activate speed fall by pressing fire.
        if (!speedFallActivated && Input.GetButtonDown("Fire1"))
        {
            player.GetComponent<Rigidbody>().isKinematic = false;
            player.GetComponent<Rigidbody>().AddForce(Vector3.down * 600f, ForceMode.VelocityChange);
            Physics.gravity *= speedFallGravityMultipier;
            speedFallActivated = true;
            forceInvincibility = true;
        }

        if (player.transform.position.y <= 600f)
        {
            scoreManager.HideLevelCompleteScreen();
        }

        // See if the player has touched down.
        if (player.transform.position.y <= 2.2f)
        {
            scoreManager.HideLevelCompleteScreen();

            // Begin rotating camera back to regular position.
            player.transform.Find("FirstPersonCharacter").transform.DOLocalRotate(new Vector3(0f, 0f, 0f), lookUpSpeed*0.6f, RotateMode.Fast);

            // Reset movement variables.
            player.GetComponent<Rigidbody>().isKinematic = true;
            player.GetComponent<FirstPersonController>().m_WalkSpeed = savedRegularMoveSpeed;

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
                player.GetComponent<CharacterController>().radius, 
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
        scoreManager.PlayerKilledEnemy(enemyKillValue);
        specialBarManager.PlayerKilledEnemy();

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
        GameObject.Find("FPSController").GetComponent<FirstPersonController>().enabled = true;
        foreach (Gun gun in FindObjectsOfType<Gun>())
        {
            gun.enabled = true;
        }

        gameStarted = true;
    }


    public void RestartGame()
    {
        SceneManager.LoadScene("mainScene");
    }


    public void SpecialMoveReady()
    {
        
    }


    public void UpdateBillboards()
    {
        FindObjectOfType<BatchBillboard>().UpdateBillboards();
    }


    public bool PositionIsInLevelBoundaries(Vector3 position)
    {
        if (position.x > levelGenerator.levelSize / 2 ||
            position.x < -levelGenerator.levelSize / 2 ||
            position.z > levelGenerator.levelSize / 2 ||
            position.z < -levelGenerator.levelSize / 2)
        {
            return false;
        }

        else return true;
    }
}
