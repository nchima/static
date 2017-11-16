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

    float thickness = 0.2f;
    Color m_Color;
    float travelledDistance = 0f;
    Vector3 previousPosition = Vector3.zero;
    static ObjectPooler meshPooler;


    private void Awake() {
        if (meshPooler == null) {
            if (!GameObject.Find("Pooled Bullet Meshes").GetComponent<ObjectPooler>()) {
                Debug.LogError("Could not find bullet mesh pooler!");
                Debug.Break();
                return;
            } else {
                meshPooler = GameObject.Find("Pooled Bullet Meshes").GetComponent<ObjectPooler>();
            }
        }
    }


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
            (1 << 8) | (1 << 13) | (1 << 14));

        if (hit.collider != null) {
            transform.position = hit.point;
            HandleHit(hit);
        } else {
            transform.position = nextPosition;
            travelledDistance += Vector3.Distance(transform.position, previousPosition);
            if (travelledDistance >= maxDistance) { EndBulletsExistence(); }
        }

        DrawTrail();

        previousPosition = transform.position;
    }


    void DrawTrail() {
        GameObject newTrailPiece = meshPooler.GrabObject();
        newTrailPiece.GetComponent<PlayerBulletMesh>().SetTransformByEndPoints(previousPosition, transform.position, thickness);
        newTrailPiece.GetComponent<PlayerBulletMesh>().SetColor(m_Color);
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
        headVisuals.transform.localScale = Vector3.one * MyMath.Map(GameManager.instance.currentSine, -1f, 1f, headSizeRange.min, headSizeRange.max);
        m_Color = _color;
        headVisuals.GetComponentInChildren<Renderer>().material.SetColor("_Tint", m_Color);
    }


    public void HandleHit(RaycastHit hit) {

        if (hit.transform.GetComponent<Enemy>() != null) {
            Instantiate(strikeEnemyPrefab, hit.point, Quaternion.LookRotation(Vector3.up));
            hit.collider.GetComponent<Enemy>().HP -= 1;
            GameManager.instance.BulletHitEnemy();
            GameManager.sfxManager.PlayBulletHitEnemySound(hit.collider.GetComponent<Enemy>());
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
            //newExplosion.GetComponent<Explosion>().
        }

        EndBulletsExistence();
    }


    private void EndBulletsExistence() {
        GetComponent<PooledObject>().GetReturned();
    }
}