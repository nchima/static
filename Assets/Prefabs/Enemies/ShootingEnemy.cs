using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShootingEnemy : Enemy {

    // USED FOR SHOOTING
    [SerializeField] protected float shotTimerMin = 0.7f;   // The minimum amount of time in between shots.
    [SerializeField] protected float shotTimerMax = 5f;   // The maximum amount of time in between shots.
    [SerializeField] protected float preShotDelay = 0.7f; // How long I pause motionless before firing a shot.
    [SerializeField] protected float postShotDelay = 0.4f;    // How long I pause motionless after firing a shot.
    [SerializeField] protected GameObject shotPrefab;
    [SerializeField] bool requireLineOfSight;
    protected Timer shotTimer;    // Keeps track of how long it's been since I last fired a shot.


    // BEHAVIOR STATES
    protected enum BehaviorState { PreparingToMove, Moving, PreShooting, Shooting, PostShooting };
    protected BehaviorState currentState;

    new void Start ()
    {
        base.Start();

        // Not sure why I'm doing this - look into it later. (Does it have to do with the enemies firing shots at the wrong point during their animation?)
        myAnimator.speed = 1.0f / preShotDelay;

        // Get a random time to fire the next shot.
        shotTimer = new Timer(Random.Range(shotTimerMin, shotTimerMax));

        currentState = BehaviorState.PreparingToMove;
    }
	
	new void Update ()
    {
        base.Update();

        switch (currentState)
        {
            case BehaviorState.PreparingToMove:
                PrepareToMove();
                break;
            case BehaviorState.Moving:
                Move();
                break;
            case BehaviorState.PreShooting:
                PreShoot();
                break;
            case BehaviorState.Shooting:
                Attack();
                    break;
            case BehaviorState.PostShooting:
                PostShoot();
                break;
        }
    }

    protected virtual void PrepareToMove()
    {
        if (!willMove) return;

        if (navMeshAgent.isStopped) navMeshAgent.isStopped = false;

        // Get a random point in a circle around the player.
        Vector3 nearPlayer = playerTransform.position + Random.insideUnitSphere * moveRandomness;
        nearPlayer.y = transform.position.y;

        // Get a direction to that point
        Vector3 direction = nearPlayer - transform.position;
        direction.Normalize();

        // Scale that direction to a random magnitude
        targetPosition = transform.position + direction * Random.Range(moveDistanceMin, moveDistanceMax);

        targetPosition.y = transform.position.y;

        navMeshAgent.SetDestination(targetPosition);

        currentState = BehaviorState.Moving;
    }

    void Move()
    {
        // See if it's time to shoot at the player
        shotTimer.Run();
        if (shotTimer.finished && willAttack)
        {
            // See if we have line-of-sight to the player.
            if (requireLineOfSight)
            {
                if (canSeePlayer)
                {
                    ReadyPreShoot();
                    return;
                }

                else
                {
                    shotTimer = new Timer(Random.Range(shotTimerMin, shotTimerMax));
                    currentState = BehaviorState.PreparingToMove;
                    return;
                }
            }

            else
            {
                shotTimer = new Timer(Random.Range(shotTimerMin, shotTimerMax));
                ReadyPreShoot();
                return;
            }
        }

        // Move towards target position
        //Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        Vector3 newPosition = transform.position + Vector3.Normalize(navMeshAgent.desiredVelocity) * moveSpeed * Time.deltaTime;
        myRigidbody.MovePosition(newPosition);
        //myRigidbody.MovePosition(transform.position + navMeshAgent.desiredVelocity);

        // If we've reached the target position, find a new target position
        moveTimer += Time.deltaTime;
        if (Vector3.Distance(newPosition, targetPosition) < 2f || moveTimer > 5f)
        {
            moveTimer = 0f;
            currentState = BehaviorState.PreparingToMove;
            return;
        }
    }

    protected virtual void ReadyPreShoot()
    {
        // Set timer for pre shot delay
        shotTimer = new Timer(preShotDelay);

        navMeshAgent.isStopped = true;

        // Begin the charging up animation.
        myAnimator.SetTrigger("ChargeUp");
        //attackingColorCurrent = Color.Lerp(attackingColorCurrent, attackingColorMax, 0.8f * Time.deltaTime);
        DOTween.To(() => attackingColorCurrent, x => attackingColorCurrent = x, attackingColorMax, preShotDelay * 0.8f).SetEase(Ease.InCubic).SetUpdate(true);
        transform.Find("Geometry").DOLookAt(playerTransform.position, preShotDelay * 0.8f).SetEase(Ease.InCubic);

        currentState = BehaviorState.PreShooting;
    }

    protected virtual void PreShoot()
    {
        // Do nothing and wait for charge-up animation to finish.
        //attackingColorCurrent = Color.Lerp(attackingColorCurrent, attackingColorMax, 0.1f);
    }

    protected virtual void Attack()
    {
        // Fire a shot.
        GameObject newShot = Instantiate(shotPrefab, new Vector3(transform.position.x, 1.75f, transform.position.z), Quaternion.identity);
        newShot.GetComponent<EnemyShot>().firedEnemy = gameObject;

        DOTween.To(() => attackingColorCurrent, x => attackingColorCurrent = x, originalColor, postShotDelay * 0.1f).SetEase(Ease.Linear).SetUpdate(true);

        // Set the shot timer for the post shot delay.
        shotTimer = new Timer(postShotDelay);

        currentState = BehaviorState.PostShooting;
    }

    void PostShoot()
    {
        // Se if we've waited long enough.
        shotTimer.Run();
        if (shotTimer.finished)
        {
            // Determite how long until the next bullet is fired.
            shotTimer = new Timer(Random.Range(shotTimerMin, shotTimerMax));

            currentState = BehaviorState.PreparingToMove;
        }
    }

    void ChargeUpAnimationFinished()
    {
        currentState = BehaviorState.Shooting;
    }
}
