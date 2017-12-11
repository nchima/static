using UnityEngine;
using System.Collections;

public class PlayerBullet : MonoBehaviour {

    [SerializeField] float maxDistance = 1000f;
    [SerializeField] float speed;

    [SerializeField] GameObject strikeEnemyPrefab;
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



    private void FixedUpdate() {
        Vector3 nextPosition = transform.position + transform.forward * speed * Time.deltaTime;

        // Raycast from my previous position to my new position to see if I hit anything.
        RaycastHit hit;

        bool spherecast = Physics.SphereCast(
            previousPosition,
            thickness,
            transform.forward,
            out hit,
            Vector3.Distance(nextPosition, previousPosition),
            (1 << 8) | (1 << 13) | (1 << 14) | (1 << 23));

        if (hit.collider != null) {
            //Debug.Log(hit.collider.name + " was hit");
            transform.position = hit.point;
            HandleHit(hit);
        } else {
            transform.position = nextPosition;
            travelledDistance += Vector3.Distance(transform.position, previousPosition);
            if (travelledDistance >= maxDistance) { EndBulletsExistence(); }
        }

        m_playerBulletTrail.AddSegment(transform.position, previousPosition);

        previousPosition = transform.position;
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

        if (hit.transform.GetComponent<EnemyOld>() != null) {
            Instantiate(strikeEnemyPrefab, hit.point, Quaternion.LookRotation(Vector3.up));
            hit.collider.GetComponent<EnemyOld>().HP -= 1;
            if (!(FindObjectOfType<ShotgunCharge>().currentState is ShotgunChargeState_FinalAttack)) { GameManager.specialBarManager.AddValue(0.01f); }
            GameManager.sfxManager.PlayBulletHitEnemySound(hit.collider.GetComponent<EnemyOld>());
        } 
        
        else if (hit.collider.name.Contains("Homing Shot")) {
            hit.collider.gameObject.GetComponent<HomingShot>().GotShot(hit.point);
        } 
        
        else {
            Instantiate(strikeWallPrefab, hit.point, Quaternion.LookRotation(Vector3.up));
        }

        // Instantiate explosion
        if (explosionRadius > 0f) {
            GameObject newExplosion = Instantiate(explosionPrefab, hit.point, Quaternion.identity);
            newExplosion.GetComponent<Explosion>().explosionRadius = explosionRadius;
            newExplosion.GetComponent<Explosion>().SetColor(m_Color);
            newExplosion.GetComponent<Explosion>().damageMin = 1;
            newExplosion.GetComponent<Explosion>().damageMax = (int) explosionDamage;
        }

        EndBulletsExistence();
    }


    private void EndBulletsExistence() {
        GetComponent<PooledObject>().GetReturned();
    }
}