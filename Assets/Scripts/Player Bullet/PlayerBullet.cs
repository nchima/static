using UnityEngine;
using System.Collections;

public class PlayerBullet : MonoBehaviour {

    [SerializeField] bool justBeInstant;

    [SerializeField] float maxDistance = 1000f;
    [SerializeField] float speed;

    [SerializeField] GameObject strikeEnemyPrefab;
    [SerializeField] GameObject strikeWeakPointPrefab;
    [SerializeField] GameObject strikeWallPrefab;

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


    private void Update() {
        FixedUpdateMove();
    }


    void FixedUpdateMove() {
        Vector3 nextPosition = transform.position + transform.forward * speed * Time.fixedDeltaTime;

        if (justBeInstant) { nextPosition = transform.position + transform.forward * 1000f; }

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

            // If we hit a something.
            if (hit.collider != null) {
                transform.position = hit.point;
                HandleHit(hit);
            } 
            
            else {
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

        Debug.DrawLine(previousPosition, hit.point, Color.red, 1.5f);

        return hit;
    }


    public void GetFired(Vector3 _position, Vector3 _direction, float _thickness, float _speed, Color _color) {
        transform.position = _position;
        transform.forward = _direction;
        thickness = _thickness;
        speed = _speed;
        travelledDistance = 0f;
        previousPosition = transform.position;
        headVisuals.transform.localScale = Vector3.one * MyMath.Map(GunValueManager.currentValue, -1f, 1f, headSizeRange.min, headSizeRange.max);
        m_Color = _color;
        headVisuals.GetComponentInChildren<Renderer>().material.SetColor("_Tint", m_Color);

        if (m_playerBulletTrail == null) {
            GameObject trail = new GameObject("Player Bullet Trail");
            trail.transform.parent = transform.parent;
            m_playerBulletTrail = trail.AddComponent<PlayerBulletTrail>();
        }

        m_playerBulletTrail.thickness = thickness;
        m_playerBulletTrail.m_Color = m_Color;

        FixedUpdateMove();
    }


    public void HandleHit(RaycastHit hit) {

        if (hit.collider.GetComponent<EnemyOld>() != null || hit.collider.GetComponent<Enemy>() != null) {
            Instantiate(strikeEnemyPrefab, hit.point, Quaternion.LookRotation(Vector3.up));

            if (hit.collider.GetComponent<EnemyOld>() != null) { hit.collider.GetComponent<EnemyOld>().HP -= 1; }
            else if (hit.collider.GetComponent<Enemy>() != null) { hit.collider.GetComponent<Enemy>().currentHealth -= 1; }

            if (!(shotgunCharge.currentState is ShotgunChargeState_FinalAttack)) { Services.specialBarManager.AddValue(0.01f); }

            if (hit.collider.GetComponent<EnemyOld>() != null) { Services.sfxManager.PlayBulletEnemyHitSoundOld(hit.collider.GetComponent<EnemyOld>()); }
            else { Services.sfxManager.PlayBulletHitEnemySound(hit.collider.GetComponent<Enemy>()); }
        } 
        
        //else if (hit.collider.name.Contains("Homing Shot")) {
        //    hit.collider.gameObject.GetComponent<HomingShot>().GotShot(hit.point);
        //} 

        else if (hit.collider.name.Contains("Weak Point")) {
            Instantiate(strikeWeakPointPrefab, hit.point, Quaternion.LookRotation(Vector3.up));

            Services.sfxManager.PlayBulletHitWeakPointSound();

            if (hit.collider.GetComponent<EnemyWeakPointGrower>() != null) { hit.collider.GetComponent<EnemyWeakPointGrower>().YouHurtMyDad(2); }
            Services.sfxManager.PlayBulletHitEnemySound(hit.collider.transform.parent.parent.GetComponent<Enemy>());
        }
        
        else {
            Instantiate(strikeWallPrefab, hit.point, Quaternion.LookRotation(Vector3.up));
        }

        EndBulletsExistence();
    }


    private void EndBulletsExistence() {
        GetComponent<PooledObject>().GetReturned();
    }
}