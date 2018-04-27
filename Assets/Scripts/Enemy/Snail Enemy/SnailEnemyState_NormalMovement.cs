using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailEnemyState_NormalMovement : State {

    [SerializeField] GameObject shotPrefab;
    [SerializeField] GameObject gun1, gun2, gun3;

    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float turnSpeed = 1f;
    [SerializeField] float visionConeAngle = 15f; // When the player is inside this cone, I will start firing bullets at them.

    bool playerInVisionCone;
    float shotCooldown = 1f;
    float shotTimer;


    public override void Run(StateController stateController) {
        base.Run(stateController);
        SnailEnemy controller = stateController as SnailEnemy;

        // Move forward
        controller.m_NavMeshAgent.SetDestination(Services.playerTransform.position);

        // Turn towards player.
        Quaternion lookAtPlayerRotation = Quaternion.LookRotation(controller.directionToPlayer);
        Quaternion newRotation = Quaternion.RotateTowards(controller.transform.rotation, lookAtPlayerRotation, turnSpeed * Time.deltaTime);
        controller.GetComponent<Rigidbody>().MoveRotation(newRotation);

        // See if player is in vision cone.
        playerInVisionCone = Vector3.Angle(controller.directionToPlayer, transform.forward) <= visionConeAngle;

        if (playerInVisionCone) {
            shotTimer += Time.deltaTime;

            if (shotTimer >= shotCooldown) {
                FireShotInDirection(gun1.transform.position, transform.forward, stateController);

                Vector3 shotDirection = transform.forward;
                shotDirection = Quaternion.Euler(0f, -visionConeAngle, 0f) * shotDirection;
                FireShotInDirection(gun2.transform.position, shotDirection.normalized, stateController);

                shotDirection = transform.forward;
                shotDirection = Quaternion.Euler(0f, visionConeAngle, 0f) * shotDirection;
                FireShotInDirection(gun3.transform.position, shotDirection.normalized, stateController);

                shotTimer = 0f;
            }
        }
    }


    public override void FixedRun(StateController stateController) {
        base.FixedRun(stateController);
        SnailEnemy controller = stateController as SnailEnemy;

        // Move forwards
        Vector3 velocity = controller.m_NavMeshAgent.desiredVelocity.normalized;
        velocity *= moveSpeed;
        controller.GetComponent<Rigidbody>().MovePosition(controller.transform.position + velocity * Time.deltaTime);
    }


    public override void End(StateController stateController) {
    }


    void FireShotInDirection(Vector3 position, Vector3 direction, StateController stateController) {
        GameObject newShot = Instantiate(shotPrefab);
        newShot.transform.position = position;
        newShot.GetComponent<NormalShot>().direction = direction;
        newShot.GetComponent<EnemyShot>().firedEnemy = stateController.gameObject;
    }

}
