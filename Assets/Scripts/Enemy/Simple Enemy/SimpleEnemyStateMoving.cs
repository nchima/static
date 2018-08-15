using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemyStateMoving : State {

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float moveRandomness;
    [SerializeField] FloatRange moveDistanceRange;

    const float MAX_MOVE_TIME = 5f;    // This is mostly used as a failsafe to make sure that I don't get stuck trying to reach an unreachable point.
    float moveTimer;

    Vector3 targetPosition; // The point which I haven chosen as my next destination.


    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);
        ChooseNewDestination(stateController);
    }


    public override void Run(StateController stateController) {
        base.Run(stateController);
        SimpleEnemy controller = stateController as SimpleEnemy;

        // Move towards target position
        Vector3 nextPosition = transform.position + Vector3.Normalize(controller.navMeshAgent.desiredVelocity) * moveSpeed * Time.deltaTime;
        controller.GetComponent<Rigidbody>().MovePosition(nextPosition);

        // If we've reached the target position, or the move has timed out, get a new target position.
        moveTimer += Time.deltaTime;
        if (Vector3.Distance(nextPosition, targetPosition) < 2f || moveTimer > 5f) {
            ChooseNewDestination(controller);
            return;
        }
    }


    private void ChooseNewDestination(StateController stateController) {
        SimpleEnemy controller = stateController as SimpleEnemy;

        // Make sure my navMeshAgent is enabled.
        if (!controller.navMeshAgent.enabled) controller.navMeshAgent.enabled = true;

        // Get a random point within a circle around the player.
        targetPosition = Services.playerTransform.position + Random.insideUnitSphere * moveRandomness;
        targetPosition.y = transform.position.y;

        // Get a direction to that point.
        Vector3 direction = targetPosition - transform.position;
        direction.Normalize();

        // Scale the direction to a random magnitude.
        targetPosition = transform.position + direction * Random.Range(moveDistanceRange.min, moveDistanceRange.max);
        targetPosition.y = transform.position.y;

        controller.navMeshAgent.SetDestination(targetPosition);

        moveTimer = 0f;
    }
}