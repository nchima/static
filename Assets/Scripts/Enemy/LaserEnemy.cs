using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LaserEnemy : ShootingEnemy {

    [SerializeField] Transform laserOrigin;
    bool shotFired = false;
    GameObject lastShot;    // A reference to the most recent shot that I fired.
    Color chargingLaserColor = new Color(0.5f, 1f, 1f);


    
    protected override void PrepareToMove()
    {
        base.PrepareToMove();

        if (immovable) immovable = false;
    }


    protected override void ReadyPreShoot()
    {
        ReturnToKinematic();
        immovable = true;

        shotTimer = new Timer(preShotDelay);

        //navMeshAgent.isStopped = true;
        navMeshAgent.enabled = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

        myGeometry.transform.DOScale(1.5f, preShotDelay);
        //foreach (MeshRenderer mr in myGeometry.GetComponentsInChildren<MeshRenderer>()) mr.material.DOColor(chargingLaserColor, preShotDelay).SetEase(Ease.Linear);
        //foreach (MeshRenderer mr in myGeometry.GetComponentsInChildren<MeshRenderer>()) mr.material.DOColor(chargingLaserColor, "_EmissionColor", preShotDelay).SetEase(Ease.Linear);
        //DOTween.To(() => baseEmissionColor, x => baseEmissionColor = x, chargingLaserColor, preShotDelay * 0.9f).SetEase(Ease.Linear).SetUpdate(true);
        DOTween.To(() => attackingColorCurrent, x => attackingColorCurrent = x, chargingLaserColor, preShotDelay * 0.9f).SetEase(Ease.Linear).SetUpdate(true);
        DOTween.To(() => GetComponentInChildren<Rotator>().speedScale, x => GetComponentInChildren<Rotator>().speedScale = x, 5f, preShotDelay * 0.9f).SetEase(Ease.Linear).SetUpdate(true);
        //attackingColorCurrent = Color.Lerp(attackingColorCurrent, attackingColorMax, 0.9f * preShotDelay * 0.9f * Time.deltaTime);

        currentState = BehaviorState.PreShooting;
    }


    protected override void PreShoot()
    {
        // Fire a shot.
        if (!shotFired) {
            //Debug.Log("Laser enemy firing laser.");
            GameObject newShot = Instantiate(shotPrefab, laserOrigin.transform.position, Quaternion.identity);
            newShot.transform.parent = transform;
            newShot.GetComponent<EnemyShot>().firedEnemy = gameObject;
            newShot.GetComponent<LaserShot>().preDamageDuration = preShotDelay;
            lastShot = newShot;
            shotFired = true;
        }

        shotTimer.Run();
        if (shotTimer.finished) {
            myGeometry.transform.DOScale(1f, 0.1f);
            currentState = BehaviorState.Shooting;
        }
    }


    protected override void Attack()
    {
        //while (baseEmissionColor != originalEmissionColor) baseEmissionColor = originalEmissionColor;
        //while (baseColor != originalColor) baseColor = originalColor;
        attackingColorCurrent = originalColor;
        DOTween.To(() => GetComponentInChildren<Rotator>().speedScale, x => GetComponentInChildren<Rotator>().speedScale = x, 1f, preShotDelay * 0.1f).SetEase(Ease.Linear).SetUpdate(true);

        // Set the shot timer for the post shot delay.
        shotTimer = new Timer(postShotDelay);

        shotFired = false;

        currentState = BehaviorState.PostShooting;
    }


    protected override void PostShoot()
    {
        shotTimer.Run();
        if (shotTimer.finished)
        {
            // Determite how long until the next bullet is fired.
            shotTimer = new Timer(Random.Range(shotTimerMin, shotTimerMax));

            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            currentState = BehaviorState.PreparingToMove;
        }
    }


    protected override void Die()
    {
        base.Die();

        if (lastShot != null) lastShot.GetComponent<LaserShot>().ReadyPostDamage();
    }
}
