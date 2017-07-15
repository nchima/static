using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MeleeEnemy : Enemy {

	enum BehaviorState { MovingTowardsPlayer, ChargingUpAttack, Attacking, FinishingAttack };
    BehaviorState currentState;

    float flankingAngle = 45f;

    // ATTACKING
    bool isAttacking; // Whether we are currently attacking.
    float attackRange = 15f; // We will begin attacking when we are this close to the player.
    float attackDistance { get { return attackRange * 3; } }    // This is how far we 'charge' forward during our attack.
    float distanceCharged = 0f; // Used to keep track of how far we have charged during our current attack.
    float attackSpeed { get { return moveSpeed * 8; } } // This is how quickly we travel during a charge attack.
    Vector3 attackFinishPoint;  // This is point towards which we charge. (No longer necessary?)
    float afterAttackPauseTime = 1f;  // How long we pause after attacking.

    float timer = 0f;

    new void Start()
    {
        base.Start();

        // Choose whether to flank the player on their left or their right.
        if (Random.value >= 0.5f) flankingAngle *= -1;

        currentState = BehaviorState.MovingTowardsPlayer;
    }

    private void Update()
    {
        switch (currentState)
        {
            case BehaviorState.MovingTowardsPlayer:
                MoveTowardsPlayer();
                break;
            case BehaviorState.ChargingUpAttack:
                ChargeUpAttack();
                break;
            case BehaviorState.Attacking:
                Attack();
                break;
            case BehaviorState.FinishingAttack:
                FinishAttack();
                break;
        }
    }


    void MoveTowardsPlayer()
    {
        float tempMoveSpeed = moveSpeed;
        if (!canSeePlayer)
        {
            tempMoveSpeed *= 0.5f;
        }

        // Check if the player is near enough to attack.
        if (Vector3.Distance(playerTransform.position, transform.position) <= attackRange)
        {
            if (willAttack)
            {
                navMeshAgent.isStopped = true;
                myAnimator.SetTrigger("ChargeUp");
                currentState = BehaviorState.ChargingUpAttack;
                timer = 0f;
                return;
            }

            else
            {
                navMeshAgent.isStopped = true;
                currentState = BehaviorState.FinishingAttack;
                return;
            }
        }

        //Debug.Log("Melee enemy moving towards player.");

        // Get the direction towards the player, then modify it by our flanking angle.
        Vector3 directionTowardsPlayer = playerTransform.position - transform.position;
        directionTowardsPlayer.y = 0f;
        directionTowardsPlayer = Quaternion.AngleAxis(flankingAngle, Vector3.up) * directionTowardsPlayer;

        transform.DORotate(Quaternion.LookRotation(directionTowardsPlayer).eulerAngles, 0.5f);

        navMeshAgent.SetDestination(transform.position + directionTowardsPlayer.normalized * 5f);
        Vector3 nextPosition = transform.position + navMeshAgent.desiredVelocity.normalized * tempMoveSpeed * Time.deltaTime;
        //Vector3 velocity = (Vector3.Normalize(navMeshAgent.desiredVelocity) + Vector3.Normalize(gameManager.playerVelocity)*2.5f) * moveSpeed * Time.deltaTime;
        //Vector3 nextPosition = transform.position + velocity;

        GetComponent<Rigidbody>().MovePosition(nextPosition);
    }

    void ChargeUpAttack()
    {
        transform.DORotate(Quaternion.LookRotation(playerTransform.position - transform.position).eulerAngles, 0.5f);
        // Do nothing and wait for animation to finish.
    }

    void Attack()
    {
        if (currentState != BehaviorState.Attacking)
        {
            Vector3 attackDirection = Vector3.Normalize(playerTransform.position - transform.position);
            attackFinishPoint = transform.position + attackDirection * (Vector3.Distance(transform.position, playerTransform.position) + attackDistance);
            transform.LookAt(attackFinishPoint);
            Debug.DrawLine(transform.position, attackFinishPoint, Color.green, 10f);
            isAttacking = true;

            currentState = BehaviorState.Attacking;
        }

        //Debug.Log("Attacking");

        // Charge towards the player.
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, attackFinishPoint, attackSpeed * Time.deltaTime);
        distanceCharged += Vector3.Distance(transform.position, nextPosition);
        GetComponent<Rigidbody>().MovePosition(nextPosition);

        // See if I've charged far enough or a timer has run out (in case I hit a wall and can't finish my charge).
        timer += Time.deltaTime;

        if (distanceCharged >= attackDistance || timer >= 1)
        {
            distanceCharged = 0f;
            timer = 0f;
            currentState = BehaviorState.FinishingAttack;
            return;
        }
    }

    void FinishAttack()
    {
        isAttacking = false;
        timer += Time.deltaTime;
        if (timer >= afterAttackPauseTime)
        {
            Debug.Log("Melee enemy resetting.");
            if (Random.value >= 0.5f) flankingAngle *= -1;
            navMeshAgent.isStopped = false;
            currentState = BehaviorState.MovingTowardsPlayer;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // See if we hit the player.
        if (collision.collider.tag == "Player")
        {
            gameManager.PlayerHurt();
        }
    }
}
