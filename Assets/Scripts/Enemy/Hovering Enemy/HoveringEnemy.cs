using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HoveringEnemy : Enemy {

    public float hoverHeight = 4f;
    public HoveringEnemyAnimationController m_AnimationController;

    public Rigidbody m_Rigidbody { get { return GetComponent<Rigidbody>(); } }


    /*
    protected override void Move()
    {
        // See if it's time to shoot at the player
        //shotTimer.Run();
        //if (shotTimer.finished && willAttack)
        //{
        //    // See if we have line-of-sight to the player.
        //    if (requireLineOfSight)
        //    {
        //        if (canSeePlayer)
        //        {
        //            ReadyPreShoot();
        //            return;
        //        }

        //        else
        //        {
        //            shotTimer = new Timer(Random.Range(shotTimerMin, shotTimerMax));
        //            currentState = BehaviorState.PreparingToMove;
        //            return;
        //        }
        //    }

        //    else
        //    {
        //        shotTimer = new Timer(Random.Range(shotTimerMin, shotTimerMax));
        //        ReadyPreShoot();
        //        return;
        //    }
        //}

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

    */
}
