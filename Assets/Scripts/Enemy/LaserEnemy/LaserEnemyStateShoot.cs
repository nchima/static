using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LaserEnemyStateShoot : State {

    [SerializeField] float preShotDelay;
    [SerializeField] float postShotDelay;
    [SerializeField] GameObject laserPrefab;
    [SerializeField] Transform laserOrigin;
    [SerializeField] Transform geometry;

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

        // Lock enemy in place.
        controller.m_NavMeshAgent.enabled = false;
        controller.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

        // Turn towards the player.
        controller.transform.DOLookAt(GameManager.player.transform.position, preShotDelay * 0.5f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(preShotDelay * 0.5f);

        // 'Puff up' and rotate more quickly as I charge my shot.
        geometry.transform.DOScale(1.5f, preShotDelay);
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
        newShot.transform.parent = controller.transform;
        newShot.GetComponent<EnemyShot>().firedEnemy = gameObject;
        newShot.GetComponent<LaserShot>().preDamageDuration = preShotDelay;
        lastShot = newShot;

        yield return new WaitForSeconds(preShotDelay);

        // Get small again as the laser is fired.
        geometry.transform.DOScale(1f, 0.1f);

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


        yield return new WaitForSeconds(postShotDelay);

        // Unlock position.
        controller.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        controller.m_NavMeshAgent.enabled = true;

        GetComponent<TriggerTransition>().isTriggerSet = true;

        yield return null;
    }
}
