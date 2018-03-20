using UnityEngine;
using System.Collections;

public class PlayerBullet : MonoBehaviour {

    [SerializeField] bool justBeInstant;

    [SerializeField] float maxDistance = 1000f;
    [SerializeField] float speed;

    [SerializeField] GameObject strikeEnemyPrefab;
    [SerializeField] GameObject strikeWeakPointPrefab;
    [SerializeField] GameObject strikeWallPrefab;

    [SerializeField] GameObject explosionPrefab;
    float explosionRadius;
    float explosionDamage;

    [SerializeField] GameObject headVisuals;
    [SerializeField] FloatRange headSizeRange = new FloatRange(0f, 0.75f);

    PlayerBulletTrail m_playerBulletTrail;

    public Color m_Color;
    public float thickness = 0.2f;
    Vector3 previousPosition = Vector3.zero;
    float travelledDistance = 0f;

    ShotgunCharge shotgunCharge;


    private void Awake() {
        shotgunCharge = FindObjectOfType<ShotgunCharge>();
    }


    private void FixedUpdate() {
        FixedUpdateMove();
    }


    void FixedUpdateMove() {
        Vector3 nextPosition = transform.position + transform.forward * speed * Time.fixedDeltaTime;

        if (justBeInstant) { nextPosition = transform.position + transform.forward * 1000f; }

        // Raycast first ignoring regular enemy layers in order to see if we are aimed at a weak point.
        //Debug.Log((1 << 8) | (1 << 28));
        RaycastHit hit = SphereCastOnLayer(nextPosition, (1 << 8) | (1 << 28));

        if (hit.collider != null && hit.collider.name.Contains("Weak Point")) {
            transform.position = hit.point;
            HandleHit(hit);
        } else {
            // Raycast from my previous position to my new position to see if I hit an enemy's regular surface.
            hit = SphereCastOnLayer(nextPosition, (1 << 8) | (1 << 13) | (1 << 14) | (1 << 23));

            if (hit.collider != null) {
                //Debug.Log(hit.collider.name + " was hit");
                transform.position = hit.point;
                HandleHit(hit);
            } else {
                transform.position = nextPosition;
                travelledDistance += Vector3.Distance(transform.position, previousPosition);
                if (travelledDistance >= maxDistance) { EndBulletsExistence(); }
            }
        }

        float modifier = 0.5f;
        m_playerBulletTrail.AddSegment(transform.position + Random.insideUnitSphere * modifier, previousPosition + Random.insideUnitSphere * modifier, PlayerController.currentVelocity);

        previousPosition = transform.position;
    }


    RaycastHit SphereCastOnLayer(Vector3 toPosition, int layerBitmask) {
        RaycastHit hit;

        Debug.DrawLine(previousPosition, toPosition, Color.red);

        bool spherecast = Physics.SphereCast(
            previousPosition,
            thickness,
            transform.forward,
            out hit,
            Vector3.Distance(toPosition, previousPosition),
            layerBitmask
        );

        return hit;
    }


    public void GetFired(Vector3 _position, Vector3 _direction, float _thickness, float _speed, float _explosionRadius, float _explosionDamage, Color _color) {
        transform.position = _position;
        transform.forward = _direction;
        thickness = _thickness;
        speed = _speed;
        travelledDistance = 0f;
        previousPosition = transform.position;
        explosionRadius = _explosionRadius;
        explosionDamage = _explosionDamage;
        headVisuals.transform.localScale = Vector3.one * MyMath.Map(GunValueManager.currentValue, -1f, 1f, headSizeRange.min, headSizeRange.max);
        m_Color = _color;
        headVisuals.GetComponentInChildren<Renderer>().material.SetColor("_Tint", m_Color);

        if (m_playerBulletTrail == null) {
            GameObject newObject = new GameObject();
            m_playerBulletTrail = newObject.AddComponent<PlayerBulletTrail>();
        }

        m_playerBulletTrail.thickness = thickness;
        m_playerBulletTrail.m_Color = m_Color;
    }


    public void HandleHit(RaycastHit hit) {

        if (hit.collider.GetComponent<EnemyOld>() != null || hit.collider.GetComponent<Enemy>() != null) {
            Instantiate(strikeEnemyPrefab, hit.point, Quaternion.LookRotation(Vector3.up));

            if (hit.collider.GetComponent<EnemyOld>() != null) { hit.collider.GetComponent<EnemyOld>().HP -= 1; }
            else if (hit.collider.GetComponent<Enemy>() != null) { hit.collider.GetComponent<Enemy>().currentHealth -= 1; }

            if (!(shotgunCharge.currentState is ShotgunChargeState_FinalAttack)) { GameManager.specialBarManager.AddValue(0.01f); }

            if (hit.collider.GetComponent<EnemyOld>() != null) { GameManager.sfxManager.PlayBulletEnemyHitSoundOld(hit.collider.GetComponent<EnemyOld>()); }
            else { GameManager.sfxManager.PlayBulletHitEnemySound(hit.collider.GetComponent<Enemy>()); }
        } 
        
        else if (hit.collider.name.Contains("Homing Shot")) {
            hit.collider.gameObject.GetComponent<HomingShot>().GotShot(hit.point);
        } 

        else if (hit.collider.name.Contains("Weak Point")) {
            //Debug.Log("Bullet struck enemy weak point!");
            Instantiate(strikeWeakPointPrefab, hit.point, Quaternion.LookRotation(Vector3.up));

            GameManager.sfxManager.PlayBulletHitWeakPointSound();

            if (hit.collider.transform.parent.parent.GetComponent<Enemy>() != null) { hit.collider.transform.parent.parent.GetComponent<Enemy>().currentHealth -= 50; }
            GameManager.sfxManager.PlayBulletHitEnemySound(hit.collider.transform.parent.parent.GetComponent<Enemy>());
        }
        
        else {
            Instantiate(strikeWallPrefab, hit.point, Quaternion.LookRotation(Vector3.up));
        }

        // Instantiate explosion
        //if (explosionRadius > 0f) {
        //    GameObject newExplosion = Instantiate(explosionPrefab, hit.point, Quaternion.identity);
        //    newExplosion.GetComponent<Explosion>().explosionRadius = explosionRadius;
        //    newExplosion.GetComponent<Explosion>().SetColor(m_Color);
        //    newExplosion.GetComponent<Explosion>().damageMin = 1;
        //    newExplosion.GetComponent<Explosion>().damageMax = (int) explosionDamage;
        //}

        EndBulletsExistence();
    }


    private void EndBulletsExistence() {
        GetComponent<PooledObject>().GetReturned();
    }
}