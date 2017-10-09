using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;

public class Gun : MonoBehaviour
{
    /* TWEAKABLE VARIABLES */

    // How many bullets are fired per shot.
    [SerializeField] int bulletsPerBurstMin = 2;
    [SerializeField] int bulletsPerBurstMax = 30;

    // How many shots can be fired per second.
    [SerializeField] float burstsPerSecondMin = 1f;
    [SerializeField] float burstsPerSecondMax = 5f;
    [HideInInspector] public float burstsPerSecondModifier = 1f;
    [SerializeField] public float burstsPerSecondModifierMax = 2f;   // Your rate of fire increases by this multiplier during slow motion.

    // Bullet spread.
    [SerializeField] float inaccuracyMin = 1f;
    [SerializeField] float inaccuracyMax = 10f;

    // How far bullets can travel.
    [SerializeField] float bulletRange = 500f;

    // Damage per bullet
    [SerializeField] int bulletDamageMin = 5;
    [SerializeField] int bulletDamageMax = 15;

    // Force per bullet
    [SerializeField] int minBulletsForce = 10;  // The number of bullets which must hit before force is applied.
    [SerializeField] float forcePerBullet = 0.2f;

    [SerializeField] float bulletRecoil = 0.2f; // Recoil per bullet

    // Bullet Color
    [SerializeField] Color bulletColor1;
    [SerializeField] Color bulletColor2;

    [SerializeField] GameObject crosshair;

    // How quickly the gun oscillates between extremes.
    [SerializeField] float oscSpeed = 0.3f;

    [HideInInspector] public bool canShoot = true;  // Used by other scripts to disable the gun at certain times.

    /* GENERAL SPECIAL MOVE STUFF */
    public float specialMoveSineRange = 0.1f;
    public bool shotgunChargeIsReady;
    public bool missilesAreReady;
    Color specialMoveReadyColor1 = Color.yellow;
    Color specialMoveReadyColor2 = Color.red;

    /* MISSILE STUFF */
    [SerializeField] float missileCooldown = 0.15f;  // Time in between firing individual missiles.
    [SerializeField] int missilesPerBurst = 20;
    int missilesFired = 0;
    float missileTimer;
    bool firingMissiles = false;
    bool firedMissiles = false;

    /* SHOTGUN CHARGE STUFF */
    bool isDoingShotgunCharge = false;

    // USED DURING SHOOTING
    int bulletsPerBurst;
    int bulletsHitThisBurst = 0;

    /* AUDIO STUFF */
    public AudioSource rifleAudioSource;
    [SerializeField] AudioClip[] rifleAudioClips;
    [SerializeField] AudioSource shotgunAudioSource;


    /* PREFABS */
    public GameObject bulletPrefab;
    public GameObject bulletStrikePrefab;
    [SerializeField] GameObject bulletStrikeEnemyPrefab;
    public AudioSource bulletStrikeAudio;
    public GameObject muzzleFlash;
    [SerializeField] private GameObject missilePrefab;

    /* REFERENCES */
    GameManager gameManager;
    Transform bulletSpawnTransform; // The point where bullets originate (ie the tip of the player's gun)
    Transform gunTipTransform;
    Animator animator;
    Animator parentAnimator;

    /* MISC */
    Quaternion baseBulletSpawnRotation;
    Vector3 nearestEnemyPosition;
    float timeSinceLastShot;
    GameObject[] bullets;    // Holds references to all bullets.
    int bulletIndex = 0;
    Color bulletColor = Color.yellow;  // The current color of the bullets.
    Vector3 originalPosition;   // The original position of the gun (used for recoil).
    Vector3 recoilPosition;


    void Start()
    {
        // Instantiate all bullet prefabs. (Just make 100 for now so I don't have to do math to figure out how many could potentially be on screen at once.)
        // Then, move them all to a far away place so the player doesn't see them.
        bullets = new GameObject[200];
        for (int i = 0; i < bullets.Length; i++)
        {
            bullets[i] = Instantiate(bulletPrefab);
            bullets[i].transform.position = new Vector3(0, -500, 0);
        }

        // Get the point from which bullets will spawn.
        bulletSpawnTransform = GameObject.Find("BulletSpawnPoint").transform;

        gunTipTransform = GameObject.Find("Tip").transform;

        // Get a reference to the score controller.
        gameManager = FindObjectOfType<GameManager>();

        animator = GetComponent<Animator>();
        parentAnimator = GetComponentInParent<Animator>();

        originalPosition = transform.localPosition;
        recoilPosition = new Vector3(0f, -3.68f, 10.68f);
    }


    void Update()
    {
        // Handle crosshair.
        //if (TestBurst())
        //    crosshair.GetComponent<CrossHairLines>().targetAllGoodAndStuff = true;
        //else
        //    crosshair.GetComponent<CrossHairLines>().targetAllGoodAndStuff = false;

        // Update gun animation state
        animator.SetFloat("Gun State", gameManager.currentSine);

        bulletColor = Color.Lerp(bulletColor1, bulletColor2, MyMath.Map(gameManager.currentSine, -1f, 1f, 0f, 1f));

        // Run shot timer.
        timeSinceLastShot += Time.deltaTime;

        // Reset special moves if sine is in middle of bar.
        if (gameManager.currentSine < 0.1f && gameManager.currentSine > -0.1f)
        {
            firedMissiles = false;
        }

        /* Firing special moves */
        foreach(MeshRenderer mr in GetComponentsInChildren<MeshRenderer>())
        {
            if (shotgunChargeIsReady || missilesAreReady) mr.material.color = Color.Lerp(specialMoveReadyColor1, specialMoveReadyColor2, Random.Range(0f, 1f));
            else mr.material.color = Color.black;
        }

        //if (Input.GetButton("Fire2"))
        //{
        //    missilesFired = 0;
        //    missileTimer = 0f;
        //    firingMissiles = true;
        //}

        if (canShoot && (Input.GetButton("Fire2") || Input.GetAxisRaw("Fire2") != 0))
        {
            if (missilesAreReady && !firingMissiles && !firedMissiles)
            {
                gameManager.PlayerUsedSpecialMove();
                missilesFired = 0;
                missileTimer = 0f;
                firingMissiles = true;
            }

            else if (shotgunChargeIsReady)
            {
                gameManager.PlayerUsedSpecialMove();
                isDoingShotgunCharge = true;
                gameManager.BeginShotgunCharge();
            }
        }

        // Perform special move if necessary.
        if (firingMissiles) FireMissiles();
        else if (isDoingShotgunCharge) DoShotgunCharge();

        /* Firing normal bullets */
        if (Input.GetButton("Fire1") || Input.GetAxisRaw("Fire1") != 0) FireBurst();
    }


    void FireMissiles()
    {
        if (!firingMissiles) return;

        if (missileTimer >= missileCooldown)
        {
            missilesFired++;
            missileTimer = 0;
            Instantiate(missilePrefab, gunTipTransform.position, gameManager.player.transform.rotation);
        }

        else
        {
            missileTimer += Time.deltaTime;
        }

        if (missilesFired >= missilesPerBurst)
        {
            firingMissiles = false;
            firedMissiles = true;
        }
    }


    void DoShotgunCharge()
    {
        if (Input.GetButtonUp("Fire2") || Input.GetAxisRaw("Fire2") == 0)
        {
            isDoingShotgunCharge = false;
            gameManager.CompleteShotgunCharge();
        }
    }


    bool TestBurst()
    {
        // Get new firing variables based on current oscillation.
        bulletsPerBurst = Mathf.RoundToInt(MyMath.Map(gameManager.currentSine, -1f, 1f, bulletsPerBurstMax, bulletsPerBurstMin));
        float burstsPerSecond = MyMath.Map(gameManager.currentSine, -1f, 1f, burstsPerSecondMin, burstsPerSecondMax) * burstsPerSecondModifier;
        float inaccuracy = MyMath.Map(gameManager.currentSine, -1f, 1f, inaccuracyMax, inaccuracyMin);
        
        bulletsHitThisBurst = 0;

        // Fire the specified number of test bullets.
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            // Rotate bullet spawner to get the direction of the next bullet.
            bulletSpawnTransform.localRotation = Quaternion.Euler(new Vector3(90 + Random.insideUnitCircle.x * inaccuracy, 0, Random.insideUnitCircle.y * inaccuracy));

            // Raycast to see if the bullet hit an object and to see where it hit.
            RaycastHit hit;
            if (Physics.SphereCast(bulletSpawnTransform.position, 0.4f, bulletSpawnTransform.up, out hit, bulletRange, (1 << 8) | (1 << 13) | (1 << 14)))
            {
                // If the bullet hit an enemy...
                if (hit.collider.tag == "Enemy")
                {
                    bulletsHitThisBurst++;
                }
            }
        }

        if (bulletsHitThisBurst >= bulletsPerBurst - 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    // Firing a burst of bullets.
    public void FireBurst()
    {
        // Get new firing variables based on current oscillation.
        bulletsPerBurst = Mathf.RoundToInt(MyMath.Map(gameManager.currentSine, -1f, 1f, bulletsPerBurstMax, bulletsPerBurstMin));
        float burstsPerSecond = MyMath.Map(gameManager.currentSine, -1f, 1f, burstsPerSecondMin, burstsPerSecondMax) * burstsPerSecondModifier;
        float inaccuracy = MyMath.Map(gameManager.currentSine, -1f, 1f, inaccuracyMax, inaccuracyMin);

        rifleAudioSource.clip = rifleAudioClips[Random.Range(0, rifleAudioClips.Length)];
        rifleAudioSource.pitch = MyMath.Map(gameManager.currentSine, -1f, 1f, 0.2f, 1f);
        rifleAudioSource.volume = MyMath.Map(gameManager.currentSine, -1f, 1f, 0.2f, 1f);

        shotgunAudioSource.pitch = MyMath.Map(gameManager.currentSine, -1f, 1f, 0.8f, 2f);
        shotgunAudioSource.volume = MyMath.Map(gameManager.currentSine, -1f, 1f, 1f, 0.2f);

        // Make sure enough time has passed since the last shot.
        if (!canShoot || timeSinceLastShot < 1 / burstsPerSecond) { return; }

        bulletsHitThisBurst = 0;

        // Play shooting sound.
        rifleAudioSource.Play();
        shotgunAudioSource.Play();

        transform.parent.SendMessage("IncreaseShake", 0.01f);

        // Show muzzle flash.
        GameObject _muzzleFlash = Instantiate(muzzleFlash);
        _muzzleFlash.transform.position = gunTipTransform.position;
        _muzzleFlash.transform.rotation = gunTipTransform.rotation;
        _muzzleFlash.transform.localScale = new Vector3(
            MyMath.Map(gameManager.currentSine, -1f, 1f, 0.5f, 0.3f),
            MyMath.Map(gameManager.currentSine, -1f, 1f, 0.5f, 0.3f),
            MyMath.Map(gameManager.currentSine, -1f, 1f, 0.5f, 0.3f)
            );

        // Get a new bullet color based on current sine
        bulletColor = Color.Lerp(bulletColor1, bulletColor2, MyMath.Map(gameManager.currentSine, -1f, 1f, 0f, 1f));

        /* SEE IF WE NEED TO AIM UP OR DOWN AT ENEMIES */

        // Box cast forward in a line across the center of the screen to grab all enemies in the player's line of fire.
        float boxCastLength = 500f;
        float boxCastHeight = 75f;
        float boxCastWidth = 1f;
        RaycastHit[] boxCastHits = Physics.BoxCastAll(
            GameManager.instance.player.transform.position + GameManager.instance.player.transform.forward * 2f,
            new Vector3(boxCastWidth, boxCastHeight, 1f),
            GameManager.instance.player.transform.forward,
            GameManager.instance.player.transform.rotation,
            boxCastLength,
            (1 << 14)
            );

        // Figure out which of those enemies is closest to the player.
        nearestEnemyPosition = GameManager.instance.player.transform.position + GameManager.instance.player.transform.forward * 100f;
        foreach (RaycastHit hit in boxCastHits)
        {
            // For the enemies's position, use the center of its renderer.
            Vector3 thisEnemyPosition = hit.collider.GetComponentInChildren<MeshRenderer>().bounds.center;

            // See if the distance to this enemy is less than the distance to the previous nearest enemy.
            if (Vector3.Distance(GameManager.instance.player.transform.position, thisEnemyPosition) 
                < Vector3.Distance(GameManager.instance.player.transform.position, nearestEnemyPosition))
            {
                nearestEnemyPosition = thisEnemyPosition;
            }
        }

        Debug.DrawLine(bulletSpawnTransform.position, nearestEnemyPosition, Color.cyan, 5f);

        // Rotate the base transform of the bullet spawner towards the nearest enemy's position.
        baseBulletSpawnRotation = Quaternion.LookRotation(
            Vector3.Normalize(nearestEnemyPosition - bulletSpawnTransform.position),
            Vector3.up
            //GameManager.instance.player.transform.up
            );

        Debug.DrawRay(bulletSpawnTransform.position, (baseBulletSpawnRotation * Vector3.forward) - bulletSpawnTransform.position * 100f, Color.yellow, 5f);

        // Fire the specified number of bullets.
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            FireBullet(inaccuracy);
        }

        // Add recoil to player controller.
        gameManager.player.GetComponent<PlayerController>().AddRecoil(bulletRecoil * bulletsPerBurst);

        timeSinceLastShot = 0f;

        //Debug.Break();
    }

    // Firing an individual bullet.
    void FireBullet(float inaccuracy)
    {
        // Rotate bullet spawner to get the direction of the next bullet.
        bulletSpawnTransform.LookAt(nearestEnemyPosition);
        bulletSpawnTransform.localRotation = Quaternion.Euler(
            new Vector3(bulletSpawnTransform.localRotation.eulerAngles.x + Random.insideUnitCircle.y * inaccuracy, Random.insideUnitCircle.x * inaccuracy, 0)
            );

        // Declare variables for bullet size & position.
        float bulletScale;
        Vector3 bulletPosition;

        // Whether we should play the bullet hit sound.
        bool playAudio = false;

        // Raycast to see if the bullet hit an object and to see where it hit.
        RaycastHit hit;
        if (Physics.SphereCast(bulletSpawnTransform.position, 0.4f, bulletSpawnTransform.forward, out hit, bulletRange, (1 << 8) | (1 << 13) | (1 << 14)))
        {
            Debug.DrawRay(bulletSpawnTransform.position, bulletSpawnTransform.forward * 100f, Color.red, 1f);
            // Show particle effect
            Instantiate(bulletStrikePrefab, hit.point, Quaternion.identity);

            // The new bullet's y scale will be half of the ray's length
            bulletScale = hit.distance / 2;

            // The new bullet's position will be halfway down the ray
            bulletPosition = bulletSpawnTransform.forward.normalized * (hit.distance / 2);

            // If the bullet hit an enemy...
            if (hit.transform.GetComponent<Enemy>() != null)
            {
                bulletsHitThisBurst++;

                Instantiate(bulletStrikeEnemyPrefab, hit.point, Quaternion.LookRotation(Vector3.up));

                if (bulletsPerBurst >= minBulletsForce)
                {
                    hit.collider.GetComponent<Enemy>().BecomePhysicsObject(0.5f);
                    //hit.collider.GetComponent<Rigidbody>().AddForceAtPosition(
                    //    Vector3.Normalize(hit.point - GameManager.instance.player.transform.position)*forcePerBullet,
                    //    hit.point,
                    //    ForceMode.Impulse);
                    hit.collider.GetComponent<Rigidbody>().AddExplosionForce(forcePerBullet, hit.point, 1f, 0.01f, ForceMode.Impulse);
                }

                // Tell the enemy it was hurt.
                hit.collider.GetComponent<Enemy>().HP -= Random.Range(bulletDamageMin, bulletDamageMax);

                // Tell the score controller that the player hit an enemy with a bullet.
                gameManager.BulletHitEnemy();

                // We only want to play the bullet strike sound once, not once for every bullet that hit an enemy. So set a bool which tells the sound to play
                // later on.
                playAudio = true;
            }

            else if (hit.collider.name.Contains("Homing Shot"))
            {
                hit.collider.gameObject.GetComponent<HomingShot>().GotShot(hit.point);
            }
        }

        // If the bullet did not strike anything give it a generic size and position.
        else
        {
            bulletScale = bulletRange / 2;
            bulletPosition = bulletSpawnTransform.up.normalized * (bulletRange / 2);
        }

        // If a bullet hit an enemy, play the bullet strike audio.
        if (playAudio)
        {
            bulletStrikeAudio.Play();
        }

        // Set bullet color
        bullets[bulletIndex].GetComponentInChildren<MeshRenderer>().material.color = bulletColor;
        bullets[bulletIndex].GetComponentInChildren<MeshRenderer>().material.SetColor("_EmissionColor", bulletColor);

        // Fire bullet.
        bullets[bulletIndex].GetComponent<PlayerBullet>().GetFired(
            bulletSpawnTransform.position + bulletPosition,
            bulletSpawnTransform.rotation,
            new Vector3(bulletPrefab.transform.localScale.x, bulletPrefab.transform.localScale.y, bulletScale)
        );

        // Get a new bullet index.
        bulletIndex += 1;
        if (bulletIndex >= 100)
        {
            bulletIndex = 0;
        }
    }
}