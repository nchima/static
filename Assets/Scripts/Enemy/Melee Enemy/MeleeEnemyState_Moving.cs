using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MeleeEnemyState_Moving : State {

    [SerializeField] float moveSpeed;

    float changeDirectionTimer = 0f;

    // MOVING
    float flankingAngle = 70f;
    FloatRange changeDirectionFrequencyRange = new FloatRange(5f, 8f);
    float nextDirectionChangeTime;


    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);
        MeleeEnemy controller = stateController as MeleeEnemy;

        controller.m_NavMeshAgent.isStopped = false;
        GetNewFlankingAngle();
        nextDirectionChangeTime = changeDirectionFrequencyRange.Random;
    }


    public override void Run(StateController stateController) {
        base.Run(stateController);

        MeleeEnemy controller = stateController as MeleeEnemy;

        float tempMoveSpeed = moveSpeed;

        // Move faster if we can see the player.
        if (!controller.canSeePlayer) { tempMoveSpeed *= 0.5f; }

        // See if it's time to change our move direction.
        changeDirectionTimer += Time.deltaTime;
        if (changeDirectionTimer >= nextDirectionChangeTime) {
            GetNewFlankingAngle();
            nextDirectionChangeTime = changeDirectionFrequencyRange.Random;
            changeDirectionTimer = 0f;
        }

        // Get the direction towards the player, then modify it by our flanking angle.
        Vector3 directionTowardsPlayer = Services.playerTransform.position - controller.transform.position;
        directionTowardsPlayer.y = 0f;
        directionTowardsPlayer = Quaternion.AngleAxis(flankingAngle, Vector3.up) * directionTowardsPlayer;

        // Modify direction to avoid obstacles.
        RaycastHit hit;

        // Raycast to look for solid objects in front of us.
        if (Physics.Raycast(controller.transform.position, directionTowardsPlayer, out hit, 10f, (1 << 8) | (1 << 24))) {

            // Directions perpendicular left and right from the detected wall.
            Vector3 direction1 = Quaternion.Euler(0f, 90f, 0f) * hit.normal;
            Vector3 direction2 = Quaternion.Euler(0f, -90f, 0f) * hit.normal;

            // Travel in the perpendicular direction that forms an obtuse angle with our forward direction.
            if (Vector3.Angle(direction1, directionTowardsPlayer) < Vector3.Angle(direction2, directionTowardsPlayer)) {
                directionTowardsPlayer = direction1;
            } else {
                directionTowardsPlayer = direction2;
            }

            // If this new direction will still lead us into a wall, give up and change direction completely.
            if (Physics.Raycast(controller.transform.position, directionTowardsPlayer, 20f, (1 << 8) | (1 << 24))) {
                GetNewFlankingAngle();
                return;
            }
        }

        Quaternion newRotation = Quaternion.LookRotation(directionTowardsPlayer, Vector3.up);
        controller.transform.rotation = newRotation;
        //controller.transform.DORotate(Quaternion.LookRotation(directionTowardsPlayer).eulerAngles, 1f);

        controller.m_NavMeshAgent.SetDestination(controller.transform.position + directionTowardsPlayer.normalized * 5f);
        Vector3 nextPosition = controller.transform.position + controller.m_NavMeshAgent.desiredVelocity.normalized * tempMoveSpeed * Time.deltaTime;
        //Vector3 velocity = (Vector3.Normalize(navMeshAgent.desiredVelocity) + Vector3.Normalize(gameManager.playerVelocity)*2.5f) * moveSpeed * Time.deltaTime;
        //Vector3 nextPosition = transform.position + velocity;

        controller.GetComponent<Rigidbody>().MovePosition(nextPosition);
    }

    public override void End(StateController stateController) {
    }

    void GetNewFlankingAngle() {
        flankingAngle = Random.Range(15f, 30f);
        if (Random.value >= 0.5f) flankingAngle *= -1;
    }
}
