using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TankEnemyState_Shooting : State {

    [SerializeField] GameObject shotPrefab;
    [SerializeField] GameObject geometry;
    [SerializeField] Transform shotOrigin;
    [SerializeField] GameObject chargeParticles;
    [SerializeField] float preShotDelay = 1f;
    [SerializeField] float postShotDelay = 1f;

    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);
        StartCoroutine(ShootingCoroutine(stateController));
    }

    public override void End(StateController stateController) {
    }

    IEnumerator ShootingCoroutine(StateController stateController) {
        TankEnemy controller = stateController as TankEnemy;

        // Lock this enemy in place.
        controller.m_NavMeshAgent.enabled = false;

        // 'Puff up' and rotate more quickly as I charge my shot.
        geometry.transform.DOScale(1.1f, preShotDelay * 0.9f);

        // Begin animation.
        controller.animationController.StartOpenCrystalAnimation(preShotDelay * 0.1f);

        // Particle sytem
        foreach (ParticleSystem particleSystem in chargeParticles.GetComponentsInChildren<ParticleSystem>()) {
            particleSystem.Stop();
            ParticleSystem.MainModule mm = particleSystem.main;
            mm.duration = preShotDelay * 0.9f;
            particleSystem.Play();
        }

        // Color stuff.. Maybe I'll want to bring this back some day?
        //foreach (MeshRenderer mr in myGeometry.GetComponentsInChildren<MeshRenderer>()) mr.material.DOColor(chargingLaserColor, preShotDelay).SetEase(Ease.Linear);
        //foreach (MeshRenderer mr in myGeometry.GetComponentsInChildren<MeshRenderer>()) mr.material.DOColor(chargingLaserColor, "_EmissionColor", preShotDelay).SetEase(Ease.Linear);
        //DOTween.To(() => baseEmissionColor, x => baseEmissionColor = x, chargingLaserColor, preShotDelay * 0.9f).SetEase(Ease.Linear).SetUpdate(true);
        //DOTween.To(() => attackingColorCurrent, x => attackingColorCurrent = x, chargingLaserColor, preShotDelay * 0.9f).SetEase(Ease.Linear).SetUpdate(true);
        //attackingColorCurrent = Color.Lerp(attackingColorCurrent, attackingColorMax, 0.9f * preShotDelay * 0.9f * Time.deltaTime);

        yield return new WaitForSeconds(preShotDelay);

        /* THE SHOT IS FIRED */

        GameObject newShot = Instantiate(shotPrefab, shotOrigin.position, Quaternion.identity);
        newShot.GetComponent<EnemyShot>().firedEnemy = gameObject;

        // Get small again as the laser is fired.
        geometry.transform.DOScale(1f, 0.1f);

        // More color stuff...
        //while (baseEmissionColor != originalEmissionColor) baseEmissionColor = originalEmissionColor;
        //while (baseColor != originalColor) baseColor = originalColor;
        //attackingColorCurrent = originalColor;

        // Start closing animation.
        controller.animationController.StartCloseCrystalAnimation(preShotDelay * 1.5f);

        yield return new WaitForSeconds(postShotDelay);

        // Unlock position.
        controller.m_NavMeshAgent.enabled = true;

        controller.ResetShotTimer();
        GetComponent<TriggerTransition>().isTriggerSet = true;

        yield return null;
    }
}
