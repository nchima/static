using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : StateController {

    // HEALTH AND DYING
    [SerializeField] int maxHealth;
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
                _currentHealth = value;
            }
        }
    }
    [SerializeField] AudioSource hurtAudio;
    protected bool isAlive = true;
    [SerializeField] GameObject deathParticles;

    // SCORING
    public int scoreKillValue;  // How many points the player gets for killing this enemy.
    [SerializeField] float specialKillValue = 0.3f;
    public float bonusTimeAdded;    // How much time this enemy adds to the bonus timer at level generation.

    // MISC UTILITY
    public bool canSeePlayer {
        get {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, GameManager.player.transform.position - transform.position, out hit, 200f, 1 << 8 | 1 << 16)) {
                if (hit.transform == GameManager.player.transform) return true;
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

    private void Awake() {
        _currentHealth = maxHealth;
    }

    protected virtual void Die() {
        // isAlive is used to make sure that this function is not called more than once.
        if (!isAlive) { return; }

        Instantiate(deathParticles, transform.position, Quaternion.identity);
        GameManager.instance.PlayerKilledEnemy(scoreKillValue, specialKillValue);
        isAlive = false;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        // Make sure we die instantly if, for some reason, we fall off the level.
        if (other.name.ToLower().Contains("player fall catcher")) {
            Die();
        }
    }
}
