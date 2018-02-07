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
        Vector3 directionTowardsPlayer = GameManager.player.transform.position - transform.position;
        directionTowardsPlayer.y = 0f;
        directionTowardsPlayer = Quaternion.AngleAxis(flankingAngle, Vector3.up) * directionTowardsPlayer;

        transform.DORotate(Quaternion.LookRotation(directionTowardsPlayer).eulerAngles, 0.5f);

        controller.m_NavMeshAgent.SetDestination(transform.position + directionTowardsPlayer.normalized * 5f);
        Vector3 nextPosition = transform.position + controller.m_NavMeshAgent.desiredVelocity.normalized * tempMoveSpeed * Time.deltaTime;
        //Vector3 velocity = (Vector3.Normalize(navMeshAgent.desiredVelocity) + Vector3.Normalize(gameManager.playerVelocity)*2.5f) * moveSpeed * Time.deltaTime;
        //Vector3 nextPosition = transform.position + velocity;

        controller.GetComponent<Rigidbody>().MovePosition(nextPosition);
    }

    public override void End(StateController stateController) {
    }

    void GetNewFlankingAngle() {
        flankingAngle *= Random.Range(0.75f, 1f);
        if (Random.value >= 0.5f) flankingAngle *= -1;
    }
}
