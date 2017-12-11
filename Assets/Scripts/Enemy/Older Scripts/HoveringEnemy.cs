using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HoveringEnemy : ShootingEnemyOld {

    [SerializeField] float meanderMaximum = 10f;
    [SerializeField] float meanderNoiseSpeed = 0.01f;
    [SerializeField] float hoverHeight = 4f;
    PerlinNoise movementNoise;


    protected override void Start()
    {
        base.Start();
        movementNoise = new PerlinNoise(meanderNoiseSpeed);
    }


    protected override void PrepareToMove()
    {
        if (!willMove) return;

        // Get new noise variables.
        movementNoise = new PerlinNoise(meanderNoiseSpeed);

        currentState = BehaviorState.Moving;
    }


    protected override void Move()
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

        // Move towards player in a meandering pattern.
        Vector3 moveDirection = Vector3.Normalize(playerTransform.position - transform.position);
        moveDirection.x += movementNoise.MapValue(-meanderMaximum, meanderMaximum).x;

        // Move towards hover height on the y axis.
        if (transform.position.y > hoverHeight) moveDirection.y = -1;
        else moveDirection.y = 1;

        myRigidbody.MovePosition(transform.position + moveDirection.normalized * moveSpeed * Time.deltaTime);

        movementNoise.Iterate();
    }


    protected override void Attack()
    {
        // Fire a shot.
        GameObject newShot = Instantiate(shotPrefab, transform.position, Quaternion.identity);
        newShot.GetComponent<EnemyShot>().firedEnemy = gameObject;
        Vector3 shotDirection = Vector3.Normalize(playerTransform.position - newShot.transform.position);
        newShot.GetComponent<NormalShot>().direction = shotDirection;


        DOTween.To(() => attackingColorCurrent, x => attackingColorCurrent = x, originalColor, postShotDelay * 0.1f).SetEase(Ease.Linear).SetUpdate(true);

        // Set the shot timer for the post shot delay.
        shotTimer = new Timer(postShotDelay);

        currentState = BehaviorState.PostShooting;
    }
}
