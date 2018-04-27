using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {

    [SerializeField] protected float stayDuration = 6f; // How long this pickup stays in the world before disappearing.

    protected float kickForce = 300f;   // How fast this pickup begins moving toward the player.
    protected float moveForce = 200f;   // How quickly this pickup accelerates toward the player.

    float stayTimer = 0f;

    enum State { Hover, MoveToPlayer }
    State state = State.Hover;

    Vector3 directionToPlayer { get { return Vector3.Normalize(Services.playerTransform.position - transform.position); } }

    private void Update() {
        if (state == State.Hover) {
            // Hover or whatever.
            stayTimer += Time.deltaTime;
            if (stayTimer >= stayDuration) { Destroy(gameObject); }
        } else if (state == State.MoveToPlayer) {
            // Move towards the player or whatever.
            GetComponent<Rigidbody>().AddForce(directionToPlayer * moveForce * Time.deltaTime, ForceMode.Acceleration);
        }
    }

    protected virtual void GetAbsorbed() {
        // Show some particles or something, IDK, play some sound whatever fine.
        Destroy(gameObject);
    }

    public void BeginMovingTowardsPlayer() {
        GetComponent<Rigidbody>().AddForce(directionToPlayer * kickForce, ForceMode.Impulse);
        state = State.MoveToPlayer;
    }

    public void OnTriggerStayChild(Collider other) {
        // If the player crossed into my radius, start moving towards them.
        if (state == State.Hover && other.gameObject == Services.playerGameObject) {
            BeginMovingTowardsPlayer();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        // If I touch the player, get absorbed or whatever.
        if (collision.collider.gameObject == Services.playerGameObject) {
            GetAbsorbed();
        }
    }
}
