using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using System;

public class LaserEnemyStateDash : State {

    [SerializeField] float turnDuration = 0.5f; // How quickly I turn towards the dash direction.
    [SerializeField] float dashDistance = 5f;
    [SerializeField] float dashDuration = 2f;
    [SerializeField] float optimalDistanceFromPlayer = 15f;


    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);
        StartCoroutine(Dash(stateController));
    }


    public override void End(StateController stateController) {
        LaserEnemy controller = stateController as LaserEnemy;
    }


    IEnumerator Dash(StateController stateController) {
        LaserEnemy controller = stateController as LaserEnemy;

        Vector3 nextPosition = ChooseNextPosition(stateController);

        // Turn towards new position.
        controller.transform.DOLookAt(nextPosition, turnDuration).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(turnDuration + 0.3f);

        // Dash to new position.
        controller.transform.DOMove(nextPosition, dashDuration).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(dashDuration);

        // PROBLEM AREA: not sure if it'll work.
        controller.timesDashed++;
        if (controller.timesDashed >= controller.timesToDash) {
            // Go to shooting state.
            GetComponent<LaserEnemyTransitionFromDash>().isTriggerSet = true;
        }
        else {
            // Redo dashing state.
            GetComponent<TriggerTransition>().isTriggerSet = true;
        }

        yield return null;
    }


    Vector3 ChooseNextPosition(StateController stateController) {
        LaserEnemy controller = stateController as LaserEnemy;

        for (int i = 0; i < 100; i++) {
            // Get a vector towards or away from the player based on whether we are within optimal distance.
            Vector3 moveDirection = Vector3.zero;
            if (Vector3.Distance(controller.transform.position, GameManager.player.transform.position) > optimalDistanceFromPlayer) {
                moveDirection = GameManager.player.transform.position - controller.transform.position;
            } else {
                moveDirection = controller.transform.position - GameManager.player.transform.position;
            }

            // Make sure the move direction is parallel to the ground, then scale it.
            moveDirection.y = 0f;
            moveDirection = moveDirection.normalized * dashDistance;

            // Rotate the move direction within a random range.
            moveDirection = Quaternion.Euler(0, UnityEngine.Random.Range(-90f, 90f), 0) * moveDirection;

            // Check to see if there is a path on the navmesh to the new position.
            Vector3 newPosition = controller.transform.position + moveDirection;
            NavMeshHit navMeshHit;
            if (NavMesh.Raycast(controller.transform.position, newPosition, out navMeshHit, 0)) {
                continue;
            }

            return newPosition;
        }

        // Failsafe:
        Debug.LogError("Laser enemy could not find a suitable position to move to.");
        return controller.transform.position;
    }
}
