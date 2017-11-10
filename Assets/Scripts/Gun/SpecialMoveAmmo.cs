using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialMoveAmmo : MonoBehaviour {

    float moveForce = 10f;
    float kickForce = 100f;
    [SerializeField] float ammoValue = 0.1f;
    [SerializeField] float stayDuration = 6f;
    float stayTimer = 0f;
    enum State { Hover, MoveToPlayer }
    State state = State.Hover;

    Vector3 directionToPlayer { get { return Vector3.Normalize(GameManager.instance.player.transform.position - transform.position); } }

    private void Update() {
        if (state == State.Hover) {
            // Hover or whatever.

            stayTimer += Time.deltaTime;
            if (stayTimer >= stayDuration) { Destroy(gameObject); }
        }

        else if (state == State.MoveToPlayer) {
            // Move towards the player or whatever.
            GetComponent<Rigidbody>().AddForce(directionToPlayer * moveForce * Time.deltaTime, ForceMode.Acceleration);
        }
    }

    void GetAbsorbed() {
        // Show some particles or something, IDK, play some sound whatever fine.
        GameManager.specialBarManager.PlayerAbsorbedAmmo(ammoValue);
        Destroy(gameObject);
    }

    public void OnTriggerEnterChild(Collider other) {
        // If the player crossed into my radius, start moving towards them.
        if (state == State.Hover && other.gameObject == GameManager.instance.player) {
            GetComponent<Rigidbody>().AddForce(directionToPlayer * kickForce, ForceMode.Impulse);
            state = State.MoveToPlayer;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        // If I touch the player, get absorbed or whatever.
        if (collision.collider.gameObject == GameManager.instance.player) {
            GetAbsorbed();
        }
    }
}
