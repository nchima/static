using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;
using System.Collections.Generic;   

public class Gun : MonoBehaviour {

    [SerializeField] IntRange bulletsPerBurstRange = new IntRange(2, 50);   // How many bullets are fired per shot.
    [SerializeField] FloatRange burstsPerSecondRange = new FloatRange(2f, 9f);  // How many shots can be fired per second.
    [SerializeField] FloatRange bulletSpreadRange = new FloatRange(1f, 10f);
    [SerializeField] FloatRange bulletSpeedRange = new FloatRange(300f, 600f);
    [SerializeField] FloatRange bulletThicknessRange = new FloatRange(0.1f, 0.3f);
    [SerializeField] FloatRange bulletExplosionRadiusRange = new FloatRange(0f, 5f);
    [SerializeField] IntRange explosionDamageRange = new IntRange(1, 5);
    [SerializeField] int bulletDamage = 1;
    [SerializeField] float bulletRecoil = 0.2f;
    [SerializeField] Color bulletColor1;
    [SerializeField] Color bulletColor2;

    // Modifies rate of fire during slow motion sequence.
    [HideInInspector] public float burstsPerSecondSloMoModifierCurrent = 1f;
    [SerializeField] public float burstsPerSecondSloMoModifierMax = 2f;   

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
    GameObject screen;

    /* MISC */
    Vector3 nearestEnemyPosition;
    float timeSinceLastShot;
    GameObject[] bullets;    // Holds references to all bullets.
    Vector3 originalPosition;   // The original position of the gun (used for recoil).
    Vector3 recoilPosition;
    ScreenShake[] screenShakes;

    private void Awake() {
        screenShakes = FindObjectsOfType<ScreenShake>();
    }


    void Start() {
        // Get the point from which bullets will spawn.
        bulletSpawnTransform = GameObject.Find("BulletSpawnPoint").transform;
        gunTipTransform = GameObject.Find("Tip").transform;

        // Get a reference to the score controller.
        gameManager = FindObjectOfType<GameManager>();

        screen = GameObject.Find("Screen");

        originalPosition = transform.localPosition;
        recoilPosition = new Vector3(0f, -3.68f, 10.68f);
    }

    void Update() {

        // Run shot timer.
        timeSinceLastShot += Time.deltaTime;

        // Reset special moves if sine is in middle of bar.
        if (GunValueManager.currentValue < 0.1f && GunValueManager.currentValue > -0.1f) {
            firedMissiles = false;
        }

        /* FIRING SPECIAL MOVES */

        // Make gun flash (this doesn't matter right now.)
        //foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>()) {
        //    if (shotgunChargeIsReady || missilesAreReady) mr.material.color = Color.Lerp(specialMoveReadyColor1, specialMoveReadyColor2, Random.Range(0f, 1f));
        //    else mr.material.color = Color.black;
        //}

        // See if the player has fired a special move & if so, initialize proper variables.
        if (GameManager.specialBarManager.bothBarsFull && (Input.GetButton("Fire2"))) {

            if (GunValueManager.currentValue >= 0f && !firingMissiles && !firedMissiles) {
                gameManager.PlayerUsedSpecialMove();
                missilesFired = 0;
                missileTimer = 0f;
                firingMissiles = true;
            } 
            
            else if (GunValueManager.currentValue < 0f) {
                gameManager.PlayerUsedSpecialMove();
                FindObjectOfType<ShotgunCharge>().BeginSequence();
            }
        }

        // Begin performing the special move.
        if (firingMissiles) FireMissiles();

        /* Firing normal bullets */
        if (Input.GetButton("Fire1") || Input.GetAxisRaw("Fire1") != 0) { FireBurst(); }
    }


    void FireMissiles() {
        if (!firingMissiles) return;

        if (missileTimer >= missileCooldown) {
            missilesFired++;
            missileTimer = 0;
            Instantiate(missilePrefab, gunTipTransform.position, GameManager.player.transform.rotation);
        } else {
            missileTimer += Time.deltaTime;
        }

        if (missilesFired >= missilesPerBurst) {
            firingMissiles = false;
            firedMissiles = true;
        }
    }

    public void FireBurst() {

        // Get new firing variables based on current oscillation.
        bulletsPerBurst = Mathf.RoundToInt(MyMath.Map(GunValueManager.currentValue, -1f, 1f, bulletsPerBurstRange.max, bulletsPerBurstRange.min));
        float burstsPerSecond = MyMath.Map(GunValueManager.currentValue, -1f, 1f, burstsPerSecondRange.min, burstsPerSecondRange.max) * burstsPerSecondSloMoModifierCurrent;
        float inaccuracy = MyMath.Map(GunValueManager.currentValue, -1f, 1f, bulletSpreadRange.max, bulletSpreadRange.min);

        // Make sure enough time has passed since the last shot.
        if (!canShoot || timeSinceLastShot < 1 / burstsPerSecond) { return; }

        // Tell the crosshair to vibrate more.
        FindObjectOfType<CrossHair>().AdjustShakeValueForShotFired();

        // Handle audio.
        rifleAudioSource.clip = rifleAudioClips[Random.Range(0, rifleAudioClips.Length)];
        rifleAudioSource.pitch = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.2f, 1f);
        rifleAudioSource.volume = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.2f, 1f);

        shotgunAudioSource.pitch = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.8f, 2f);
        shotgunAudioSource.volume = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 1f, 0.2f);

        bulletsHitThisBurst = 0;

        // Play shooting sound.
        rifleAudioSource.Play();
        shotgunAudioSource.Play();

        foreach (ScreenShake screenShake in screenShakes) {
            screenShake.SetShake(MyMath.Map(bulletsPerBurst, bulletsPerBurstRange.min, bulletsPerBurstRange.max, 0.025f, 0.2f), (1/burstsPerSecond) * 0.6f);
        }

        // Show muzzle flash.
        GameObject _muzzleFlash = Instantiate(muzzleFlash);
        _muzzleFlash.transform.position = gunTipTransform.position;
        _muzzleFlash.transform.rotation = gunTipTransform.rotation;
        _muzzleFlash.transform.localScale = new Vector3(
            MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.5f, 0.3f),
            MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.5f, 0.3f),
            MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.5f, 0.3f)
            );

        // Auto aim
        nearestEnemyPosition = GameManager.player.transform.position + GameManager.player.transform.forward * 1000f;
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy")) {
            float bandSize = 0.025f;

            // See if this enemy is near the middle of the screen.
            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(enemy.transform.position);
            if ((viewportPosition.x >= 0.5f-bandSize && viewportPosition.x <= 0.5f+bandSize) && (viewportPosition.y >= 0f && viewportPosition.y <= 1f)) {
                // For the enemies's position, use the center of its renderer.
                Vector3 thisEnemyPosition = enemy.GetComponentInChildren<MeshRenderer>().bounds.center;

                // See if the distance to this enemy is less than the distance to the previous nearest enemy.
                if (Vector3.Distance(GameManager.player.transform.position, thisEnemyPosition)
                    < Vector3.Distance(GameManager.player.transform.position, nearestEnemyPosition)) {
                    nearestEnemyPosition = thisEnemyPosition;
                }
            }
        }

        // Fire the specified number of bullets.
        for (int i = 0; i < bulletsPerBurst; i++) {
            FireBullet(inaccuracy);
        }

        // Add recoil to player controller.
        //GameManager.player.GetComponent<PlayerController>().AddRecoil(bulletRecoil * bulletsPerBurst);

        timeSinceLastShot = 0f;
    }
    
    // Firing an individual bullet.
    void FireBullet(float inaccuracy) {
        // Rotate bullet spawner to get the direction of the next bullet.
        bulletSpawnTransform.LookAt(nearestEnemyPosition);
        bulletSpawnTransform.localRotation = Quaternion.Euler(
            new Vector3(bulletSpawnTransform.localRotation.eulerAngles.x + Random.insideUnitCircle.y * inaccuracy, Random.insideUnitCircle.x * inaccuracy, 0)
            );

        ObjectPooler bulletPooler = GameObject.Find("Pooled Bullets").GetComponent<ObjectPooler>();
        GameObject newBullet = bulletPooler.GrabObject();

        // Fire bullet.
        newBullet.GetComponent<PlayerBullet>().GetFired(
            bulletSpawnTransform.position,
            bulletSpawnTransform.forward,
            MyMath.Map(GunValueManager.currentValue, -1f, 1f, bulletThicknessRange.min, bulletThicknessRange.max),
            MyMath.Map(GunValueManager.currentValue, -1f, 1f, bulletSpeedRange.max, bulletSpeedRange.min),
            MyMath.Map(GunValueManager.currentValue, -1f, 1f, bulletExplosionRadiusRange.min, bulletExplosionRadiusRange.max),
            MyMath.Map(GunValueManager.currentValue, -1f, 1f, explosionDamageRange.min, explosionDamageRange.max),
            Color.Lerp(bulletColor1, bulletColor2, MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0f, 1f))
        );
    }
}