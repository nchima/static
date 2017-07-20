using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LaserEnemy : ShootingEnemy {

    bool shotFired = false;

    GameObject lastShot;    // A reference to the most recent shot that I fired.


    protected override void PrepareToMove()
    {
        base.PrepareToMove();

        if (immovable) immovable = false;
    }


    protected override void ReadyPreShoot()
    {
        Debug.Log("Derived class");

        ReturnToKinematic();
        immovable = true;

        shotTimer = new Timer(preShotDelay);

        navMeshAgent.isStopped = true;

        myGeometry.transform.DOScale(1.5f, preShotDelay);

        currentState = BehaviorState.PreShooting;
    }


    protected override void PreShoot()
    {
        // Fire a shot.
        if (!shotFired)
        {
            GameObject newShot = Instantiate(shotPrefab, new Vector3(transform.position.x, 2.4f, transform.position.z), Quaternion.identity);
            newShot.GetComponent<EnemyShot>().firedEnemy = gameObject;
            newShot.GetComponent<LaserShot>().preDamageDuration = preShotDelay;
            lastShot = newShot;
            shotFired = true;
        }

        shotTimer.Run();
        if (shotTimer.finished)
        {
            myGeometry.transform.DOScale(1f, 0.1f);

            currentState = BehaviorState.Shooting;
        }
    }


    protected override void Attack()
    {
        while (baseEmissionColor != originalEmissionColor) baseEmissionColor = originalEmissionColor;

        // Set the shot timer for the post shot delay.
        shotTimer = new Timer(postShotDelay);

        shotFired = false;

        currentState = BehaviorState.PostShooting;
    }


    protected override void Die()
    {
        base.Die();

        if (lastShot != null) lastShot.GetComponent<LaserShot>().ReadyPostDamage();
    }
}
