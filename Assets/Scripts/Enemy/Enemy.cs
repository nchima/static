using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : StateController {

    // HEALTH AND DYING
    public int maxHealth;
    int _currentHealth;
    public virtual int currentHealth {
        get {
            return _currentHealth;
        }

        set {
            // See if I should die.
            if (value <= 0) {
                Die();
            } else {
                //hurtAudio.Play();
                GetHurt();
                TestPainChance();
                _currentHealth = value;
            }
        }
    }
    [SerializeField] AudioSource hurtAudio;
    protected bool isAlive = true;
    [SerializeField] GameObject deathParticles;

    [SerializeField] protected float painChance = 0.5f;
    static float painTime = 0.4f;

    // SCORING
    public int scoreKillValue;  // How many points the player gets for killing this enemy.
    [SerializeField] float specialKillValue = 0.3f;
    public float bonusTimeAdded;    // How much time this enemy adds to the bonus timer at level generation.

    // MISC UTILITY
    public bool canSeePlayer {
        get {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Services.playerTransform.position - transform.position, out hit, 200f, 1 << 8 | 1 << 16)) {
                if (hit.transform == Services.playerTransform) return true;
            }

            return false;
        }
    }
    protected bool isGrounded {
        get {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f) && hit.collider.name == "Floor") {
                return true;
            } else {
                return false;
            }
        }
    }
    [HideInInspector] public bool isAIPaused;
    Coroutine painCoroutine;

    // REFERENCES
    public NavMeshAgent m_NavMeshAgent { get { return GetComponent<NavMeshAgent>(); } }
    [SerializeField] protected GameObject myGeometry;


    protected virtual void Start() {
        _currentHealth = maxHealth;
    }


    protected override void Update() {
        base.Update();
    }


    protected virtual void Die() {
        // isAlive is used to make sure that this function is not called more than once.
        if (!isAlive) { return; }

        Instantiate(deathParticles, transform.position, Quaternion.identity);
        GameEventManager.instance.FireEvent(new GameEvents.PlayerKilledEnemy(scoreKillValue, specialKillValue, this));
        isAlive = false;
        Destroy(gameObject);
    }


    public bool IsPointOnNavMesh(Vector3 inputPoint) {
        // Check to see if the new position is on the navmesh.
        NavMeshHit destinationNavMeshHit;
        if (!NavMesh.SamplePosition(inputPoint, out destinationNavMeshHit, 10f, NavMesh.AllAreas)) {
            return false;
        }

        // Check to see if there is a path on the navmesh to the new position.
        NavMeshHit currentPositionNavMeshHit;
        NavMesh.SamplePosition(transform.position, out currentPositionNavMeshHit, 5f, NavMesh.AllAreas);
        if (!NavMesh.Raycast(currentPositionNavMeshHit.position, destinationNavMeshHit.position, out currentPositionNavMeshHit, NavMesh.AllAreas)) {
            return false;
        }

        return true;
    }


    void GetHurt() {
        if (myGeometry.GetComponent<EnemyAnimationController>() == null) {
            Debug.Log("Enemy animation controller not found.");
            return;
        }

        myGeometry.GetComponent<EnemyAnimationController>().GetHurt();
    }


    void TestPainChance() {
        if (Random.value <= painChance) {
            if (painCoroutine != null) { StopCoroutine(painCoroutine); }
            painCoroutine = StartCoroutine(PainCoroutine());
        }
    }


    IEnumerator PainCoroutine() {
        //SetAIActive(false);
        yield return new WaitForSeconds(painTime);
        //SetAIActive(true);
        yield return null;
    }


    public void SetAIActive(bool value) {
        ReturnToInitialState();
        isAIPaused = !value;
    }


    private void OnTriggerEnter(Collider other) {
        // Make sure we die instantly if, for some reason, we fall off the level.
        if (other.name.ToLower().Contains("player fall catcher")) {
            Die();
        }
    }
}
