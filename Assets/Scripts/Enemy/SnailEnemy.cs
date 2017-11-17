using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailEnemy : Enemy {

    [SerializeField] float visionCone = 15f; // When the player is inside this cone, I will start firing bullets at them.

    Vector3 directionToPlayer { get { return Vector3.Scale(Vector3.Normalize(GameManager.player.transform.position - transform.position), new Vector3(1f, 0f, 1f)); } }

    bool playerInVisionCone;
    float shotCooldown = 1f;
    float shotTimer;

    float turnSpeed = 1f;   // How quickly I turn;

    [SerializeField] GameObject shotPrefab;
    [SerializeField] GameObject gun1, gun2, gun3;

    new void Update()
    {
        base.Update();

        if (willMove)
        {
            // Move forward
            navMeshAgent.SetDestination(GameManager.player.transform.position);

            // Turn towards player.
            Quaternion lookAtPlayerRotation = Quaternion.LookRotation(directionToPlayer);
            GetComponent<Rigidbody>().MoveRotation(Quaternion.Slerp(transform.rotation, lookAtPlayerRotation, turnSpeed * Time.deltaTime));
        }


        // See if player is in vision cone.
        playerInVisionCone = Vector3.Angle(directionToPlayer, transform.forward) <= visionCone;

        if (playerInVisionCone)
        {
            shotTimer += Time.deltaTime;
            if (shotTimer >= shotCooldown)
            {
                FireNormalShotInDirection(gun1.transform.position, transform.forward);

                Vector3 shotDirection = transform.forward;
                shotDirection = Quaternion.Euler(0f, -visionCone, 0f) * shotDirection;
                FireNormalShotInDirection(gun2.transform.position, shotDirection.normalized);

                shotDirection = transform.forward;
                shotDirection = Quaternion.Euler(0f, visionCone, 0f) * shotDirection;
                FireNormalShotInDirection(gun3.transform.position, shotDirection.normalized);

                shotTimer = 0f;
            }
        }
    }


    private void FixedUpdate()
    {
        if (willMove)
        {
            // Move forwards
            Vector3 velocity = navMeshAgent.desiredVelocity.normalized;
            velocity *= moveSpeed;
            GetComponent<Rigidbody>().MovePosition(transform.position + velocity * Time.deltaTime);
        }
    }


    void FireNormalShotInDirection(Vector3 position, Vector3 direction)
    {
        GameObject newShot = Instantiate(shotPrefab);
        newShot.transform.position = position;
        newShot.GetComponent<NormalShot>().direction = direction;
        newShot.GetComponent<EnemyShot>().firedEnemy = gameObject;
    }

    public override void BecomePhysicsObject(float duration) {
        return;
    }
}
