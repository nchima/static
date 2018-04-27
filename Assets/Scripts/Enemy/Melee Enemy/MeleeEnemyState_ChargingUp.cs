using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MeleeEnemyState_ChargingUp : State {

    public float chargeUpDuration;
    float timer;

    float rotationSpeedCurrent = 0;
    [SerializeField] float rotationSpeedMax;

    [SerializeField] GameObject chargeParticles;

    Color attackingColorCurrent;
    Color attackingColorMax;


    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);
        MeleeEnemy controller = stateController as MeleeEnemy;

        timer = 0;

        // Stop moving.
        controller.m_NavMeshAgent.isStopped = true;

        // Begin rotating more quickly.
        controller.TweenRotationSpeed(controller.rotationSpeedMax, chargeUpDuration * 0.75f);

        // Change color.
        // attackingColorCurrent = Color.Lerp(attackingColorCurrent, attackingColorMax, 0.75f * Time.deltaTime);
        controller.TweenAttackColor(controller.attackingColor, chargeUpDuration * 0.75f);

        // Play animation
        controller.m_AnimationController.StartChargeWindupAnimation(chargeUpDuration * 0.75f);

        // Particle sytem
        foreach (ParticleSystem particleSystem in chargeParticles.GetComponentsInChildren<ParticleSystem>()) {
            particleSystem.Stop();
            ParticleSystem.MainModule mm = particleSystem.main;
            mm.duration = chargeUpDuration * 0.9f;
            particleSystem.Play();
        }

        // Change audio values.
        controller.humAudioSource.DOPitch(3f, chargeUpDuration * 0.9f).SetEase(Ease.InQuad);
        controller.humAudioSource.DOFade(1f, chargeUpDuration * 0.9f);
    }


    public override void Run(StateController stateController) {
        base.Run(stateController);

        timer += Time.deltaTime;
        if (timer <= chargeUpDuration * 0.7f) {
            // Turn towards player.
            stateController.transform.DORotate(Quaternion.LookRotation(Services.playerTransform.position - transform.position).eulerAngles, 0.5f);
        }
    }


    public override void End(StateController stateController) {
    }
}
