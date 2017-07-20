using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using DG.Tweening;

public class GameManager : MonoBehaviour {

    // DEBUG STUFF
    [SerializeField] bool godMode;

    // AUDIO
    [SerializeField] private AudioSource levelWinAudio;   // The audio source that plays when the player completes a level.

    // USED FOR LEVEL GENERATION
    public int levelNumber = 0;    // The current level.
    int numberOfEnemies = 4;    // The number of enemies that spawned in the current level.
    public int currentEnemyAmt;    // The number of enemies currently alive in this level.
    public LevelGenerator levelGenerator;  // A reference to the level generator script.

    // USED FOR FALLING INTO THE NEXT LEVEL
    enum PlayerState { Normal, PauseAfterLevelComplete, FallingIntoLevel, FiringShockwave };
    PlayerState playerState = PlayerState.Normal;
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
    public float currentSine;
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
    HealthManager healthManager;
    Gun gun;
    [HideInInspector] public GameObject player;


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

        // Get references
        floor = GameObject.Find("Floor").transform;
        scoreManager = GetComponent<ScoreManager>();
        healthManager = GetComponent<HealthManager>();
        levelGenerator = GetComponent<LevelGenerator>();
        gun = FindObjectOfType<Gun>();
        player = GameObject.Find("FPSController");
    }


    private void Update()
    {
        // Keep track of player velocity.
        playerVelocity = (player.transform.position - playerPositionLast) / Time.deltaTime;
        playerPositionLast = player.transform.position;

        // Update sine
        sineTime += Time.deltaTime;
        currentSine = Mathf.Sin(sineTime * oscSpeed);
        //currentSine = Mathf.Sin(sineTime);
        //currentSine = Mathf.Lerp(currentSine, (Input.GetAxis("Horizontal") * MyMath.Map(Mathf.Abs(Input.GetAxis("Vertical")), 0f, 1f, 1f, 0.5f)), 0.1f);
        //if (Input.GetAxis("Horizontal") != 0) currentSine += Input.GetAxis("Horizontal") * 0.05f;
        //else currentSine = Mathf.Lerp(currentSine, 0f, 0.05f);
        //else if (currentSine > 0) currentSine -= 0.04f;
        //currentSine = Mathf.Clamp(currentSine, -1f, 1f);
        //currentSine = MyMath.Map(player.transform.rotation.eulerAngles.y, 0f, 360f, -1f, 1);

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


    void PauseAfterLevelComplete()
    {
        fallingSequenceTimer += Time.deltaTime;
        //Debug.Log(fallingTimer);

        if (fallingSequenceTimer >= pauseAfterLevelCompleteLength)
        {
            // Generate new level.
            levelNumber += 1;
            levelGenerator.Generate();

            // Set up variables for falling.
            playerTouchedDown = false;
            gun.canShoot = false;
            savedRegularMoveSpeed = player.GetComponent<FirstPersonController>().m_WalkSpeed;
            player.GetComponent<FirstPersonController>().m_WalkSpeed = playerMoveSpeedWhenFalling;
            savedGravity = Physics.gravity;

            // Begin rotating player camera to face down.
            player.transform.Find("FirstPersonCharacter").transform.DOLocalRotate(new Vector3(90f, 0f, 0f), 0.2f, RotateMode.Fast);

            // Begin falling sequence.
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
            godMode = true;
        }

        // See if the player has touched down.
        if (player.transform.position.y <= 2.2f)
        {
            // Begin rotating camera back to regular position.
            player.transform.Find("FirstPersonCharacter").transform.DOLocalRotate(new Vector3(0f, 0f, 0f), lookUpSpeed*0.6f, RotateMode.Fast);

            // Begin tweening the time scale towards slow-motion. (Also lower music pitch.)
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.1f, 0.1f).SetEase(Ease.InQuad).SetUpdate(true);
            FindObjectOfType<MusicManager>().GetComponent<AudioSource>().DOPitch(0.1f, 0.1f).SetUpdate(true);

            // Re-enable gun and begin tweening its burst rate to quick-fire. (This allows the player to fire more quickly during slow motion.
            gun.canShoot = true;
            DOTween.To(() => gun.burstsPerSecondModifier, x => gun.burstsPerSecondModifier = x, gun.burstsPerSecondModifierMax, 0.1f).SetEase(Ease.InQuad).SetUpdate(true);

            // Reset movement variables.
            player.GetComponent<Rigidbody>().isKinematic = true;
            player.GetComponent<FirstPersonController>().m_WalkSpeed = savedRegularMoveSpeed;

            // Fire shockwave.
            if (speedFallActivated)
            {
                Vector3 shockwavePosition = player.transform.position;
                shockwavePosition.y = 0f;
                Instantiate(shockwavePrefab, shockwavePosition, Quaternion.identity);
            }

            fallingSequenceTimer = 0f;
            playerState = PlayerState.FiringShockwave;
        }
    }


    void FireShockwave()
    {
        fallingSequenceTimer += Time.deltaTime;
        if (fallingSequenceTimer >= lookUpSpeed)
        {
            // Allow enemies to start attacking.
            foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                enemy.GetComponent<Enemy>().willAttack = true;
            }

            // Begin tweening time scale, gun burst rate, and music pitch back to normal.
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
            DOTween.To(() => gun.burstsPerSecondModifier, x => gun.burstsPerSecondModifier = x, 1f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
            FindObjectOfType<MusicManager>().GetComponent<AudioSource>().DOPitch(1f, 1f).SetUpdate(true);

            // Destroy any obstacles that the player is touching.
            Collider[] overlappingSolids = Physics.OverlapCapsule(player.transform.position, -player.transform.up * 10f, player.GetComponent<CharacterController>().radius, 1 << 8);
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

            godMode = false;

            playerState = PlayerState.Normal;
        }
    }


    public void KilledEnemy()
    {
        // See if the player has killed all the enemies in this level. If so, change the level.
        scoreManager.KilledEnemy();
        currentEnemyAmt -= 1;

        if (currentEnemyAmt <= 0)
        {
            LevelBeaten();
        }
    }


    public void LevelBeaten()
    {
        levelWinAudio.Play();

        scoreManager.LevelBeaten();

        // Disable the floor's collider so the player falls through it.
        floor.GetComponent<Collider>().enabled = false;

        // Initiate falling sequence.
        fallingSequenceTimer = 0f;
        playerState = PlayerState.PauseAfterLevelComplete;

        // Generate a new level.
        //levelGenerator.Invoke("Generate", 1.4f);
    }


    public void PlayerHurt()
    {
        if (godMode) return;

        scoreManager.GetHurt();

        healthManager.playerHealth -= 1;

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


    public void BulletHit()
    {
        scoreManager.BulletHit();
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
