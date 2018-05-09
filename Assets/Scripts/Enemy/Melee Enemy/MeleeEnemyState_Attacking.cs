using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MeleeEnemyState_Attacking : State {

    [SerializeField] GameObject meshParticleObject;
    public float attackDistance;
    [SerializeField] float attackSpeed;
    [SerializeField] float afterAttackPauseTime = 2f;

    [HideInInspector] public float distanceCharged;

    Vector3 attackFinishPoint;


    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);
        MeleeEnemy controller = stateController as MeleeEnemy;

        controller.attackAudioSource.Play();
        controller.humAudioSource.DOFade(controller.originalHumVolume, 1f);
        controller.humAudioSource.DOPitch(controller.originalHumPitch, 0.5f);

        // Get the point to charge to.
        attackFinishPoint = controller.transform.position + 
            controller.transform.forward * (Vector3.Distance(controller.transform.position, Services.playerTransform.position) + attackDistance);

        // Start particles.
        ParticleSystem.EmissionModule em = meshParticleObject.GetComponent<ParticleSystem>().emission;
        em.enabled = true;

        // Play animation.
        controller.m_AnimationController.StartChargeWindupAnimation(0.1f);

        distanceCharged = 0f;

        StartCoroutine(AttackCoroutine(controller));
    }


    public override void Run(StateController stateController) {
        base.Run(stateController);

        // Move forward.

        // Make sure particles rotate with mesh.
        ParticleSystem.MainModule mm = meshParticleObject.GetComponent<ParticleSystem>().main;
        mm.startRotation = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
    }


    IEnumerator AttackCoroutine(MeleeEnemy controller) {

        yield return new WaitUntil(() => {
            if (distanceCharged >= attackDistance) {
                return true;
            }

            else {
                Vector3 nextPosition = Vector3.MoveTowards(controller.transform.position, attackFinishPoint, attackSpeed * Time.deltaTime);
                distanceCharged += Vector3.Distance(controller.transform.position, nextPosition);
                //controller.GetComponent<Rigidbody>().MovePosition(nextPosition);
                controller.transform.position = nextPosition;
                controller.transform.LookAt(attackFinishPoint);
                return false;
            }
        });

        // Stop particles.
        ParticleSystem.EmissionModule em = meshParticleObject.GetComponent<ParticleSystem>().emission;
        em.enabled = false;

        // Stop rotating as quickly.
        controller.TweenRotationSpeed(0f, 1f);
        controller.ReturnToOriginalAttackColor(afterAttackPauseTime * 0.9f);

        // Exit charge animation.s
        controller.m_AnimationController.EndChargeReleaseAnimation(afterAttackPauseTime * 0.8f);

        // Wait for a few seconds before resuming normal movement.
        yield return new WaitForSeconds(afterAttackPauseTime);

        GetComponent<TriggerTransition>().isTriggerSet = true;

        yield return null;
    }


    public override void End(StateController stateController) {
        base.End(stateController);
        MeleeEnemy controller = stateController as MeleeEnemy;
        controller.geometryParent.transform.DOLocalRotate(Vector3.zero, 0.5f);
    }
}
