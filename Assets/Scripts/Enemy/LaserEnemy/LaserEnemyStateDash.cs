using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using System;

public class LaserEnemyStateDash : State {

    [SerializeField] float turnDuration = 0.5f; // How quickly I turn towards the dash direction.
    [SerializeField] float maxDashDistance = 20f;
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
        controller.animationController.StartDashWindupAnimation(turnDuration);
        controller.transform.DOLookAt(nextPosition, turnDuration).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(turnDuration + 0.3f);

        // Dash to new position.
        float dashAnimDuration = dashDuration * 0.8f;
        controller.animationController.StartDashReleaseAnimation(dashAnimDuration);
        controller.transform.DOMove(nextPosition, dashDuration).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(dashAnimDuration);

        controller.animationController.EndDashReleaseAnimation(0.5f);
        yield return new WaitForSeconds(dashDuration);

        // Go to shooting state or redo this state depending on whether we have dashed enough times.
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

        float currentDashDistance = maxDashDistance + maxDashDistance/100;
        float currentModifierAngle = 45f - (180 - 45) / 100;

        for (int i = 0; i < 100; i++) {
            maxDashDistance -= maxDashDistance / 100;
            currentModifierAngle += (180 - currentModifierAngle) / 100;

            // Get a vector towards or away from the player based on whether we are within optimal distance.
            Vector3 moveDirection = Vector3.zero;
            if (Vector3.Distance(controller.transform.position, GameManager.player.transform.position) > optimalDistanceFromPlayer) {
                moveDirection = GameManager.player.transform.position - controller.transform.position;
            } else {
                moveDirection = controller.transform.position - GameManager.player.transform.position;
            }

            // Make sure the move direction is parallel to the ground, then scale it.
            moveDirection.y = 0f;
            moveDirection = moveDirection.normalized * currentDashDistance;

            // Rotate the move direction within a random range.
            moveDirection = Quaternion.Euler(0, UnityEngine.Random.Range(-currentModifierAngle, currentModifierAngle), 0) * moveDirection;

            Vector3 newPosition = controller.transform.position + moveDirection;

            // Check to see if the new position is on the navmesh.
            NavMeshHit destinationNavMeshHit;
            if (!NavMesh.SamplePosition(newPosition, out destinationNavMeshHit, 5f, NavMesh.AllAreas)) {
                continue;
            }

            // Check to see if there is a path on the navmesh to the new position.
            NavMeshHit currentPositionNavMeshHit;
            NavMesh.SamplePosition(controller.transform.position, out currentPositionNavMeshHit, 5f, NavMesh.AllAreas);
            if (NavMesh.Raycast(currentPositionNavMeshHit.position, destinationNavMeshHit.position, out currentPositionNavMeshHit, NavMesh.AllAreas)) {
                Debug.DrawLine(controller.transform.position, destinationNavMeshHit.position, Color.red);
                continue;
            }

            Debug.Log("NavMeshHit: " + currentPositionNavMeshHit.hit);

            Debug.DrawLine(controller.transform.position, newPosition, Color.green);
            //Debug.Break();

            return newPosition;
        }

        // Failsafe:
        Debug.LogError("Laser enemy could not find a suitable position to move to.");
        return controller.transform.position;
    }
}
