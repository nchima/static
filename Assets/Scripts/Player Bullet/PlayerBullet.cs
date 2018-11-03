using UnityEngine;
using System.Collections;

public class PlayerBullet : MonoBehaviour {

    [SerializeField] bool justBeInstant;

    [SerializeField] GameObject strikeEnemyPrefab;
    [SerializeField] GameObject strikeWeakPointPrefab;
    [SerializeField] GameObject strikeWallPrefab;
    [SerializeField] GameObject explosionPrefab;

    [SerializeField] GameObject headVisuals;

    float maxDistance;
    float speed;
    public Color m_Color;
    float trailThickness;
    float explosionPower;

    PlayerBulletTrail m_playerBulletTrail;
    Vector3 previousPosition = Vector3.zero;
    float travelledDistance = 0f;

    bool isActive;

    ShotgunCharge shotgunCharge;

    private void Awake() {
        shotgunCharge = FindObjectOfType<ShotgunCharge>();
    }

    private void FixedUpdate() {
        Move();
    }

    void Move() {
        if (!isActive) { return; }

        Vector3 nextPosition = transform.position + transform.forward * speed * Time.fixedDeltaTime;

        headVisuals.transform.Rotate(Vector3.forward, 10f * Time.fixedDeltaTime);

        //if (justBeInstant) { nextPosition = transform.position + transform.forward * 1000f; }

        //Debug.DrawLine(transform.position, nextPosition, Color.red, 1f);

        // Raycast first ignoring regular enemy layers in order to see if we are aimed at a weak point.
        //Debug.Log((1 << 8) | (1 << 28));
        RaycastHit hit = SphereCastOnLayer(nextPosition, (1 << 8) | (1 << 28));

        // If we did hit a weak point:
        if (hit.collider != null && hit.collider.name.Contains("Weak Point")) {
            transform.position = hit.point;
            HandleHit(hit);
        } 
        
        // If we did not hit a weak point.
        else {
            // Raycast from my previous position to my new position to see if I hit an enemy's regular surface.
            hit = SphereCastOnLayer(nextPosition, (1 << 8) | (1 << 13) | (1 << 14) | (1 << 23));

            // If we hit something.
            if (hit.collider != null) {
                transform.position = hit.point;
                HandleHit(hit);
            } 
            
            else {
                transform.position = nextPosition;
                travelledDistance += Vector3.Distance(transform.position, previousPosition);
                if (travelledDistance >= maxDistance) {
                    AddTrailSegmentAtCurrentPosition();
                    RemoveFromPlay();
                }
            }
        }

        AddTrailSegmentAtCurrentPosition();
        previousPosition = transform.position;
    }

    void AddTrailSegmentAtCurrentPosition() {
        float modifier = 0f;
        m_playerBulletTrail.AddSegment(transform.position + Random.insideUnitSphere * modifier, previousPosition + Random.insideUnitSphere * modifier, PlayerController.currentVelocity);
    }

    RaycastHit SphereCastOnLayer(Vector3 toPosition, int layerBitmask) {
        RaycastHit hit;

        bool raycast = Physics.Raycast(
            previousPosition,
            transform.forward,
            out hit,
            Vector3.Distance(toPosition, previousPosition),
            layerBitmask
        );

        //bool spherecast = Physics.SphereCast(
        //    previousPosition,
        //    thickness,
        //    transform.forward,
        //    out hit,
        //    Vector3.Distance(toPosition, previousPosition),
        //    layerBitmask
        //);

        if (hit.point != Vector3.zero) {
            Debug.DrawLine(previousPosition, hit.point, Color.red, 1.5f);
        }

        return hit;
    }

    public void GetFired(Gun.BulletInfo bulletInfo) {

        // Initialize all weapon type variables
        transform.position = bulletInfo.spawnPosition;
        transform.forward = bulletInfo.direction;
        trailThickness = bulletInfo.trailThickness;
        speed = bulletInfo.speed;
        maxDistance = bulletInfo.maxDistance;
        explosionPower = bulletInfo.explosionPower;
        m_Color = bulletInfo.color;

        // Reset tracking variables
        travelledDistance = 0f;
        previousPosition = transform.position;

        // Handle head visuals
        headVisuals.transform.localScale = Vector3.one * bulletInfo.headScale;
        headVisuals.GetComponentInChildren<Renderer>().material.SetColor("_Tint", m_Color);

        // Grab a bullet trail object if we don't already have one
        if (m_playerBulletTrail == null) {
            GameObject trail = new GameObject("Player Bullet Trail");
            //trail.transform.parent = transform.parent;
            m_playerBulletTrail = trail.AddComponent<PlayerBulletTrail>();
        }

        // Initialize trail variables
        m_playerBulletTrail.thickness = trailThickness;
        m_playerBulletTrail.m_Color = m_Color;

        // Set active
        isActive = true;
    }

    public void HandleHit(RaycastHit hit) {

        // If this bullet hit an enemy
        if (hit.collider.GetComponent<EnemyOld>() != null || hit.collider.GetComponent<Enemy>() != null) {
            Instantiate(strikeEnemyPrefab, hit.point, Quaternion.LookRotation(Vector3.up));

            Services.taserManager.PlayerShotEnemy();

            if (hit.collider.GetComponent<EnemyOld>() != null) { hit.collider.GetComponent<EnemyOld>().HP -= 1; }
            else if (hit.collider.GetComponent<Enemy>() != null) { hit.collider.GetComponent<Enemy>().currentHealth -= 1; }

            if (!(shotgunCharge.currentState is ShotgunChargeState_FinalAttack)) { Services.specialBarManager.AddValue(0.01f); }

            if (hit.collider.GetComponent<EnemyOld>() != null) { Services.sfxManager.PlayBulletEnemyHitSoundOld(hit.collider.GetComponent<EnemyOld>()); }
            else { Services.sfxManager.PlayBulletHitEnemySound(hit.collider.GetComponent<Enemy>()); }
        } 
        
        //else if (hit.collider.name.Contains("Homing Shot")) {
        //    hit.collider.gameObject.GetComponent<HomingShot>().GotShot(hit.point);
        //} 

        // If this bullet hit an enemy's weak point
        else if (hit.collider.name.Contains("Weak Point")) {
            Instantiate(strikeWeakPointPrefab, hit.point, Quaternion.LookRotation(Vector3.up));

            Services.taserManager.PlayerShotEnemy();

            Services.sfxManager.PlayBulletHitWeakPointSound();

            if (hit.collider.GetComponent<EnemyWeakPointGrower>() != null) { hit.collider.GetComponent<EnemyWeakPointGrower>().YouHurtMyDad(2); }
            Services.sfxManager.PlayBulletHitEnemySound(hit.collider.transform.parent.parent.GetComponent<Enemy>());
        }
        
        // If this bullet hit an inanimate object
        else {
            Instantiate(strikeWallPrefab, hit.point, Quaternion.LookRotation(Vector3.up));
        }

        SpawnExplosion(hit.point);
        RemoveFromPlay();
    }

    void SpawnExplosion(Vector3 position) {
        if (explosionPower < 0.2f) { return; }
        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity).GetComponent<Explosion>();
        explosion.radius = MyMath.Map(explosionPower, 0f, 1f, 0f, 40f);
    }

    private void RemoveFromPlay() {
        GetComponent<PooledObject>().ReturnToPool();
    }
}