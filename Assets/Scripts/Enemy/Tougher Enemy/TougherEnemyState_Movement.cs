using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TougherEnemyState_Movement : State {

    float shotTimer = 0f;
    float shotCooldown = 0.75f;

    float chargeUpTimer = 0f;
    float chargeUpTime = 0.9f;

    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);
        TougherEnemy controller = stateController as TougherEnemy;

        Vector3 destination = Services.playerTransform.position;
        destination.y = controller.transform.position.y;
        controller.m_NavMeshAgent.destination = destination;

        shotTimer = 0f;
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
        TougherEnemy controller = stateController as TougherEnemy;

        Vector3 destination = Services.playerTransform.position;
        destination.y = controller.transform.position.y;
        controller.m_NavMeshAgent.SetDestination(destination);

        //Debug.DrawLine(controller.transform.position, controller.m_NavMeshAgent.des)

        // Turn towards desired direction. forget this shit!! >:-[
        //Vector3 desiredDirection = controller.m_NavMeshAgent.desiredVelocity.normalized;
        //Vector3 newForward = controller.transform.forward;
        //newForward = Vector3.RotateTowards(newForward, desiredDirection, controller.turningSpeed * Time.deltaTime, 0.0f);
        //controller.transform.forward = newForward;

        // Determine move speed
        float distanceToPlayer = Vector3.Distance(controller.transform.position, Services.playerTransform.transform.position);
        float moveSpeed = controller.distantMovementSpeed;
        if (distanceToPlayer <= controller.stoppingDistance) { moveSpeed = 0f; }
        else if (distanceToPlayer <= controller.shootingDistance) { moveSpeed = controller.shootingMovementSpeed; }

        controller.m_NavMeshAgent.speed = moveSpeed;

        // Move toward player
        Vector3 newPosition = controller.transform.position;
        newPosition += controller.transform.forward * moveSpeed * Time.deltaTime;
        newPosition.y = 3.2f;
        controller.m_Rigidbody.MovePosition(newPosition);

        // If the player is in range:
        if ((moveSpeed == controller.shootingMovementSpeed || moveSpeed == 0f) && controller.CanSeePlayer) {

            // Charge guns
            if (chargeUpTimer <= chargeUpTime) {
                chargeUpTimer += Time.deltaTime;
                return;
            }

            // Fire guns
            shotTimer += Time.deltaTime;
            if (shotTimer >= shotCooldown) {

                NormalShot shot1 = Instantiate(controller.shotPrefab, controller.shotOriginLeft.position, Quaternion.identity).GetComponent<NormalShot>();
                NormalShot shot2 = Instantiate(controller.shotPrefab, controller.shotOriginRight.position, Quaternion.identity).GetComponent<NormalShot>();

                shot1.Direction = controller.transform.forward;
                shot2.Direction = controller.transform.forward;

                shotTimer = 0f;
            }
        }

        else {
            chargeUpTimer = 0f;
        }
    }

    public override void End(StateController stateController) {
        base.End(stateController);
    }
}
