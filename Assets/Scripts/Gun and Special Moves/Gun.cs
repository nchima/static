using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;
using System.Collections.Generic;   

public class Gun : MonoBehaviour {

    /* INSPECTOR */
    public Weapon shotgunWeapon;
    public Weapon sniperRifle;
    public Weapon machineGun;
    public Weapon pulseWeapon;
    public Weapon sawWeapon;
    public Weapon rocketWeapon;

    public Weapon upperWeapon;
    public Weapon middleWeapon;
    public Weapon lowerWeapon;

    public IntRange bulletsPerBurstRange = new IntRange(2, 50);   // How many bullets are fired per shot.
    [SerializeField] AnimationCurve bulletsPerBurstCurve;

    [SerializeField] FloatRange burstsPerSecondRange = new FloatRange(2f, 9f);  // How many shots can be fired per second.
    [SerializeField] AnimationCurve burstsPerSecondCurve;

    [SerializeField] FloatRange bulletSpreadRange = new FloatRange(1f, 10f);
    [SerializeField] AnimationCurve bulletSpreadCurve;

    [SerializeField] FloatRange bulletSpeedRange = new FloatRange(300f, 600f);

    [SerializeField] FloatRange bulletThicknessRange = new FloatRange(0.1f, 0.3f);
    [SerializeField] AnimationCurve bulletThicknessCurve;

    [SerializeField] int bulletDamage = 1;
    [SerializeField] float bulletRecoil = 0.2f;

    [SerializeField] Color bulletColor1;
    [SerializeField] Color bulletColor2;

    [SerializeField] public float burstsPerSecondSloMoModifierMax = 2f;   // Modifies rate of fire during slow motion sequence.

    [HideInInspector] public float burstsPerSecondSloMoModifierCurrent = 1f;

    // USED DURING SHOOTING
    public int BulletsPerBurst {
        //get { return Mathf.RoundToInt(CombineWeaponValue(upperWeapon.bulletsPerBurst.maxValue, lowerWeapon.bulletsPerBurst.maxValue)); }
        get { return Mathf.RoundToInt(MyMath.Map(bulletsPerBurstCurve.Evaluate(GunValueManager.currentValue), 0f, 1f, bulletsPerBurstRange.min, bulletsPerBurstRange.max)); }
    }
    int bulletsHitThisBurst = 0;
    [HideInInspector] public bool canShoot = true;  // Used by other scripts to disable the gun at certain times.

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

    /* REFERENCES */
    [HideInInspector] public Transform bulletSpawnTransform; // The point where bullets originate (ie the tip of the player's gun)
    [HideInInspector] public Transform tip;
    [SerializeField] private Text upperWeaponText;
    [SerializeField] private Text lowerWeaponText;
    GameObject screen;

    /* MISC */
    float timeSinceLastShot;
    GameObject[] bullets;    // Holds references to all bullets.
    Vector3 originalPosition;   // The original position of the gun (used for recoil).
    Vector3 recoilPosition;
    ScreenShake[] screenShakes;

    private void Awake() {
        screenShakes = FindObjectsOfType<ScreenShake>();
    }

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.GameStarted>(GameStartedHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.GameStarted>(GameStartedHandler);
    }

    void Start() {
        // Get the point from which bullets will spawn.
        bulletSpawnTransform = GameObject.Find("BulletSpawnPoint").transform;
        tip = GameObject.Find("Tip").transform;

        screen = GameObject.Find("Screen");

        originalPosition = transform.localPosition;
        recoilPosition = new Vector3(0f, -3.68f, 10.68f);

        upperWeaponText.text = upperWeapon.displayName;
        lowerWeaponText.text = lowerWeapon.displayName;
    }

    void Update() {
        timeSinceLastShot += Time.deltaTime;
        if (InputManager.fireButton) { FireBurst(); }
    }

    float CombineWeaponValue(float value1, float value2) {
        if (upperWeapon == lowerWeapon) {
            return (value1 + value2) * (1.25f - Mathf.Abs(0 - GunValueManager.currentValue));
        }
        else {
            return Mathf.Lerp(value2, value1, GunValueManager.MapTo(0f, 1f));
            if (value1 > value2) { return value1; }
            else { return value2; }
        }
    }

    // IDK if this is necessary but um...
    float CombineWeaponValue(float lower, float middle, float upper) {
        if (GunValueManager.currentValue < 0) {
            return Mathf.Lerp(lower, middle, MyMath.Map(GunValueManager.currentValue, -1f, 0f, 0f, 1f));
        }
        else {
            return Mathf.Lerp(lower, middle, GunValueManager.currentValue);
        }
    }

    public void FireBurst() {
        if (!Services.gameManager.isGameStarted) { return; }

        // OLD SYSTEM:
        // Get new firing variables based on current oscillation.
        //float burstsPerSecond = MyMath.Map(GunValueManager.currentValue, -1f, 1f, burstsPerSecondRange.min, burstsPerSecondRange.max) * burstsPerSecondSloMoModifierCurrent;
        float burstsPerSecond = MyMath.Map(burstsPerSecondCurve.Evaluate(GunValueManager.currentValue), 0f, 1f, burstsPerSecondRange.min, burstsPerSecondRange.max) * burstsPerSecondSloMoModifierCurrent;
        float bulletSpread = MyMath.Map(bulletSpreadCurve.Evaluate(GunValueManager.currentValue), 0f, 1f, bulletSpreadRange.min, bulletSpreadRange.max);
        //float accuracy = MyMath.Map(GunValueManager.currentValue, -1f, 1f, bulletSpreadRange.max, bulletSpreadRange.min);

        // NEW SYSTEM:
        //burstsPerSecond = CombineWeaponValue(upperWeapon.burstsPerSecond.maxValue, lowerWeapon.burstsPerSecond.maxValue);
        //bulletSpread = CombineWeaponValue(upperWeapon.bulletSpread.maxValue, lowerWeapon.bulletSpread.maxValue);

        // Make sure enough time has passed since the last shot.
        if (!canShoot || timeSinceLastShot < 1 / burstsPerSecond) { return; }

        // Handle audio. [MOVE THIS TO WEAPON SCRIPT LATER]
        //rifleAudioSource.clip = rifleAudioClips[Random.Range(0, rifleAudioClips.Length)];
        rifleAudioSource.pitch = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.2f, 1f);
        rifleAudioSource.volume = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.2f, 1f);

        shotgunAudioSource.pitch = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.8f, 2f);
        shotgunAudioSource.volume = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 1f, 0.2f);

        // Play shooting sound.
        rifleAudioSource.Play();
        shotgunAudioSource.Play();

        bulletsHitThisBurst = 0;

        GameEventManager.instance.FireEvent(new GameEvents.PlayerFiredGun());

        // Handle screen shake
        foreach (ScreenShake screenShake in screenShakes) {
            float newShake = MyMath.Map(BulletsPerBurst, bulletsPerBurstRange.min, bulletsPerBurstRange.max, 0.025f, 0.2f);
            if (bulletsPerBurstRange.min == bulletsPerBurstRange.max) { newShake = 0.1f; }
            screenShake.SetShake(newShake, (1 / burstsPerSecond) * 0.6f);
        }

        // Show muzzle flash.
        GameObject _muzzleFlash = Instantiate(muzzleFlash, Services.playerTransform);
        _muzzleFlash.transform.position = tip.position;
        _muzzleFlash.transform.rotation = tip.rotation;
        _muzzleFlash.transform.localScale = new Vector3(
            MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.5f, 0.3f),
            MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.5f, 0.3f),
            MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.5f, 0.3f)
            );

        // Auto aim for weak points.
        Vector3 autoAimPoint = Services.playerTransform.position + Services.playerTransform.forward * 1000f;
        autoAimPoint = AutoAim("Weak Point", 0.15f);

        // If we couldn't hit a weak point, auto aim for enemies instead.
        if (!Physics.Raycast(tip.position, autoAimPoint - tip.position, 1000f, 1 << 28)) {
            autoAimPoint = AutoAim("Enemy", 0.1f);
        }

        // Fire the specified number of bullets.
        for (int i = 0; i < BulletsPerBurst; i++) {
            FireBullet(autoAimPoint, bulletSpread);
        }

        // Add recoil to player controller.
        //Services.playerController.AddRecoil(bulletRecoil * bulletsPerBurst);

        timeSinceLastShot = 0f;
    }

    // Firing an individual bullet.
    void FireBullet(Vector3 target, float inaccuracy) {
        // Rotate bullet spawner to get the direction of the next bullet.
        bulletSpawnTransform.LookAt(target);
        bulletSpawnTransform.localRotation = Quaternion.Euler(
            new Vector3(
                bulletSpawnTransform.localRotation.eulerAngles.x + Random.insideUnitCircle.y * inaccuracy, 
                bulletSpawnTransform.localRotation.eulerAngles.y + Random.insideUnitCircle.x * inaccuracy,
                0)
            );

        Debug.DrawRay(bulletSpawnTransform.position, bulletSpawnTransform.forward * 1000f, Color.yellow);

        //Debug.Break();

        ObjectPooler bulletPooler = GameObject.Find("Pooled Bullets").GetComponent<ObjectPooler>();
        GameObject newBullet = bulletPooler.GrabObject();

        // Fire bullet.

        // This is the cool weapon based system which I now hate
        //BulletInfo newBulletInfo = new BulletInfo();
        //newBulletInfo.spawnPosition = bulletSpawnTransform.position + Vector3.up * CombineWeaponValue(upperWeapon.tipHeightOffset.maxValue, lowerWeapon.tipHeightOffset.maxValue);
        //newBulletInfo.direction = bulletSpawnTransform.forward;
        //newBulletInfo.trailThickness = CombineWeaponValue(upperWeapon.trailThickness.maxValue, lowerWeapon.trailThickness.maxValue);
        //newBulletInfo.speed = CombineWeaponValue(upperWeapon.bulletSpeed.maxValue, lowerWeapon.bulletSpeed.maxValue);
        //newBulletInfo.maxDistance = CombineWeaponValue(upperWeapon.maxDistance.maxValue, lowerWeapon.maxDistance.maxValue);
        //newBulletInfo.headScale = CombineWeaponValue(upperWeapon.bulletHeadScale.maxValue, lowerWeapon.bulletHeadScale.maxValue);
        //newBulletInfo.explosionPower = CombineWeaponValue(upperWeapon.explosionPower.maxValue, lowerWeapon.explosionPower.maxValue);
        //newBulletInfo.color = Color.Lerp(bulletColor1, bulletColor2, MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0f, 1f));

        // This is the old, less cool system which I now love again
        BulletInfo newBulletInfo = new BulletInfo();
        newBulletInfo.spawnPosition = bulletSpawnTransform.position + Random.insideUnitSphere * 0.5f;
        newBulletInfo.direction = bulletSpawnTransform.forward;
        newBulletInfo.trailThickness = MyMath.Map(bulletThicknessCurve.Evaluate(GunValueManager.currentValue), 0f, 1f, bulletThicknessRange.min, bulletThicknessRange.max);
        newBulletInfo.speed = 1000f;
        newBulletInfo.maxDistance = 5000f;
        newBulletInfo.headScale = 0f;
        newBulletInfo.explosionPower = 0f;
        newBulletInfo.color = Color.Lerp(bulletColor1, bulletColor2, MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0f, 1f));

        newBullet.GetComponent<PlayerBullet>().GetFired(newBulletInfo);
    }

    Vector3 AutoAim(string tag, float bandSize) {
        
        // Set autoaim to be straight ahead of the player.
        Vector3 autoAimPoint = Services.playerTransform.position + Services.playerTransform.transform.forward * 1000f;

        // Check all gameobjects with the given tag:
        foreach (GameObject thisObject in GameObject.FindGameObjectsWithTag(tag)) {
            
            // See if this object is near the middle of the screen.
            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(thisObject.transform.position);
            bool inXRange = viewportPosition.x >= 0.5f - bandSize * 0.5f && viewportPosition.x <= 0.5f + bandSize * 0.5f;
            bool inYRange = viewportPosition.y >= 0f && viewportPosition.y <= 1f;
            bool inZRange = viewportPosition.z >= 0f;

            //Debug.Log("x: " + inXRange + ", y: " + inYRange + ", z: " + inZRange);

            if (inXRange && inYRange && inZRange) {
                // For the enemy's position, use the center of its renderer.
                Vector3 thisPosition = thisObject.GetComponent<Collider>().bounds.center;

                // If this position is behind an obstacle, ignore it.
                RaycastHit hit;
                if (Physics.Raycast(tip.position, Vector3.Normalize(thisPosition - tip.position), out hit, 1000f, 1 << 8 | 1 << 14 | 1 << 23 | 1 << 28)) {
                    if (!hit.collider.GetComponent<Enemy>() && hit.collider.tag != "Weak Point") {
                        continue;
                    }
                }

                // See if the distance to this object is less than the distance to the previous nearest object.
                if (Vector3.Distance(Services.playerTransform.position, thisPosition) < Vector3.Distance(Services.playerTransform.position, autoAimPoint)) {
                    autoAimPoint = thisPosition;
                }
            }
        }

        return autoAimPoint;
    }

    public void GameStartedHandler(GameEvent gameEvent) {
        this.enabled = true;
    }

    public enum WeaponPosition { Upper, Lower }
    public void SwitchWeapon(WeaponPosition weaponPosition, Weapon newWeapon) {
        if (weaponPosition == WeaponPosition.Upper) {
            upperWeapon = newWeapon;
            upperWeaponText.text = newWeapon.displayName;
            //Services.uiManager.crosshair.GetComponent<CrossHair>().upperWeaponPositions = newWeapon.CrosshairVectors;
        }
        else if (weaponPosition == WeaponPosition.Lower) {
            lowerWeapon = newWeapon;
            lowerWeaponText.text = newWeapon.displayName;
            //Services.uiManager.crosshair.GetComponent<CrossHair>().lowerWeaponPositions = newWeapon.CrosshairVectors;
        }
    }

    public class BulletInfo {
        public Vector3 spawnPosition;
        public Vector3 direction;
        public float trailThickness;
        public float speed;
        public float maxDistance;
        public float headScale;
        public float explosionPower;
        public Color color;
    }
}