using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LaserEnemyStateShoot : State {

    [SerializeField] float preShotDelay;
    [SerializeField] float postShotDelay;
    [SerializeField] GameObject laserPrefab;
    public Transform laserOrigin;
    [SerializeField] Transform geometry;
    [SerializeField] float inaccuracy = 45f;
    [SerializeField] GameObject weakPoint;

    GameObject lastShot;


    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);
        StartCoroutine(ShootLaser(stateController as LaserEnemy));
    }

    public override void Run(StateController stateController) {
        base.Run(stateController);
    }

    public override void End(StateController stateController) {
        LaserEnemy controller = stateController as LaserEnemy;
        controller.timesDashed = 0;
        controller.DetermineTimesToDash();
    }

    IEnumerator ShootLaser(LaserEnemy controller) {

        // Lock this enemy in place.
        controller.m_NavMeshAgent.enabled = false;
        controller.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

        // Get a direction in which to fire my shot.
        Vector3 targetPosition = new Vector3(GameManager.player.transform.position.x, laserOrigin.transform.position.y, GameManager.player.transform.position.z);
        Vector3 shotDirection = Vector3.Normalize(targetPosition - laserOrigin.transform.position);
        float tempInaccuracy = inaccuracy;
        if (PlayerController.velocity.magnitude < 10f) { tempInaccuracy = 0f; }
        shotDirection = Quaternion.Euler(0, Random.Range(-tempInaccuracy, tempInaccuracy), 0) * shotDirection;

        // Turn towards the firing direction.
        controller.transform.DOLookAt(controller.transform.position + shotDirection, preShotDelay * 0.5f).SetEase(Ease.InQuad);
        controller.animationController.StartShootAnimation(preShotDelay * 0.5f + preShotDelay);
        yield return new WaitForSeconds(preShotDelay * 0.5f);

        // 'Puff up' and rotate more quickly as I charge my shot.
        geometry.transform.DOScale(1.1f, preShotDelay * 0.9f);
        DOTween.To(() => geometry.GetComponentInChildren<Rotator>().speedScale, x => geometry.GetComponentInChildren<Rotator>().speedScale = x, 5f, preShotDelay * 0.9f)
            .SetEase(Ease.Linear)
            .SetUpdate(true);

        // Color stuff.. Maybe I'll want to bring this back some day?
        //foreach (MeshRenderer mr in myGeometry.GetComponentsInChildren<MeshRenderer>()) mr.material.DOColor(chargingLaserColor, preShotDelay).SetEase(Ease.Linear);
        //foreach (MeshRenderer mr in myGeometry.GetComponentsInChildren<MeshRenderer>()) mr.material.DOColor(chargingLaserColor, "_EmissionColor", preShotDelay).SetEase(Ease.Linear);
        //DOTween.To(() => baseEmissionColor, x => baseEmissionColor = x, chargingLaserColor, preShotDelay * 0.9f).SetEase(Ease.Linear).SetUpdate(true);
        //DOTween.To(() => attackingColorCurrent, x => attackingColorCurrent = x, chargingLaserColor, preShotDelay * 0.9f).SetEase(Ease.Linear).SetUpdate(true);
        //attackingColorCurrent = Color.Lerp(attackingColorCurrent, attackingColorMax, 0.9f * preShotDelay * 0.9f * Time.deltaTime);

        // Begin firing shot.
        GameObject newShot = Instantiate(laserPrefab, laserOrigin.position, Quaternion.identity);
        newShot.transform.parent = laserOrigin.transform;
        newShot.GetComponent<EnemyShot>().firedEnemy = gameObject;
        newShot.GetComponent<LaserShot>().preDamageDuration = preShotDelay;
        newShot.GetComponent<LaserShot>().damageDuration = postShotDelay;
        newShot.GetComponent<LaserShot>().GetShot(shotDirection, this);
        lastShot = newShot;

        // Reveal weak point.
        weakPoint.GetComponent<EnemyWeakPointGrower>().Grow(preShotDelay * 0.7f);

        yield return new WaitForSeconds(preShotDelay);

        /* THE LASER IS FIRED */

        // Get small again as the laser is fired.
        geometry.transform.DOScale(1f, 0.1f);

        controller.animationController.StartShootReleaseAnimation(0.01f);

        // More color stuff...
        //while (baseEmissionColor != originalEmissionColor) baseEmissionColor = originalEmissionColor;
        //while (baseColor != originalColor) baseColor = originalColor;
        //attackingColorCurrent = originalColor;

        // Start rotating less quickly.
        DOTween.To(
            () => geometry.GetComponentInChildren<Rotator>().speedScale,
            x => geometry.GetComponentInChildren<Rotator>().speedScale = x,
            1f,
            preShotDelay * 0.1f)
            .SetEase(Ease.Linear).
            SetUpdate(true);

        // Hide weak point.
        weakPoint.GetComponent<EnemyWeakPointGrower>().Shrink(postShotDelay * 0.1f);

        yield return new WaitForSeconds(postShotDelay);

        controller.animationController.EndShootAnimation(0.1f);

        // Unlock position.
        controller.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        controller.m_NavMeshAgent.enabled = true;

        GetComponent<TriggerTransition>().isTriggerSet = true;

        yield return null;
    }
}
