using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupTrail : MonoBehaviour {

    [SerializeField] float kickForce = 300f;   // How fast this pickup begins moving toward the player.
    [SerializeField] float moveForce = 500f;   // How quickly this pickup accelerates toward the player.

    enum State { Inactive, BeingAbsorbed }
    State state;

    Vector3 directionToPlayer { get { return Vector3.Normalize(Services.playerTransform.position - transform.position); } }
    float distanceToPlayer { get { return Vector3.Distance(transform.position, Services.playerTransform.position); } }


    private void Update() {
        if (state == State.BeingAbsorbed) {
            GetComponent<Rigidbody>().AddForce(directionToPlayer * moveForce * Time.deltaTime, ForceMode.Force);
            moveForce *= 1.5f;
            if (moveForce > 9999999f) { GetAbsorbed(); }
            if (distanceToPlayer <= 5f) {
                GetAbsorbed();
            }
        }
    }


    void GetAbsorbed() {
        Destroy(gameObject);
    }


    public void BeginMovingTowardsPlayer() {
        transform.parent = null;
        GetComponent<Rigidbody>().AddForce(Random.insideUnitSphere * kickForce, ForceMode.Impulse);
        state = State.BeingAbsorbed;
    }
}
