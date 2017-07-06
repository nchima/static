using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy {

	enum BehaviorState { MovingTowardsPlayer, ChargingUpAttack, Attacking, FinishingAttack };
    BehaviorState currentState;

    Vector3 flankSpot;
    float flankRange = 20f;

    // ATTACKING
    bool isAttacking; // Whether we are currently attacking.
    float attackRange = 7f; // We will begin attacking when we are this close to the player.
    float attackDistance { get { return attackRange * 4; } }    // This is how far we 'charge' forward during our attack.
    float distanceCharged = 0f; // Used to keep track of how far we have charged during our current attack.
    float attackSpeed { get { return moveSpeed * 8; } } // This is how quickly we travel during a charge attack.
    Vector3 attackFinishPoint;  // This is point towards which we charge. (No longer necessary?)
    float afterAttackPauseTime = 1f;  // How long we pause after attacking.

    float timer = 0f;

    new void Start()
    {
        base.Start();

        currentState = BehaviorState.MovingTowardsPlayer;

        // Pick a point on the player's flank
        flankSpot =
            GameManager.instance.player.transform.position + GameManager.instance.player.transform.right * Random.Range(-flankRange, flankRange);
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
        // Check if the player is near enough to attack.
        if (Vector3.Distance(playerTransform.position, transform.position) <= attackRange)
        {
            myAnimator.SetTrigger("ChargeUp");
            currentState = BehaviorState.ChargingUpAttack;
            timer = 0f;
            return;
        }

        navMeshAgent.SetDestination(playerTransform.position);
        Vector3 velocity = (Vector3.Normalize(navMeshAgent.desiredVelocity) + Vector3.Normalize(gameManager.playerVelocity)) * moveSpeed * Time.deltaTime;
        Vector3 nextPosition = transform.position + velocity;
        GetComponent<Rigidbody>().MovePosition(nextPosition);
    }

    void ChargeUpAttack()
    {
        // Do nothing and wait for animation to finish.
    }

    void Attack()
    {
        if (currentState != BehaviorState.Attacking)
        {
            Vector3 attackDirection = Vector3.Normalize(playerTransform.position - transform.position);
            attackFinishPoint = transform.position + attackDirection * (Vector3.Distance(transform.position, playerTransform.position) + attackDistance);
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
        }
    }

    void FinishAttack()
    {
        isAttacking = false;
        timer += Time.deltaTime;
        if (timer >= afterAttackPauseTime)
        {
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
