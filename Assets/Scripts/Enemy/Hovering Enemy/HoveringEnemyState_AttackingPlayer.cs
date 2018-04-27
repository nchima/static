using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoveringEnemyState_AttackingPlayer : State {

    [SerializeField] float bristleAnimationDuration = 0.1f;
    [SerializeField] float bristleDuration = 1f;
    [SerializeField] float attackAnimationDuration = 0.34f;
    [SerializeField] float attackDuration = 1f;
    [SerializeField] float attackWithdrawAnimationDuration = 0.7f;
    [SerializeField] float postAttackPauseDuration = 1f;
    [SerializeField] BoxCollider hitBox;

    [SerializeField] AudioSource droneAudio;
    [SerializeField] AudioSource bristleAudio;
    [SerializeField] AudioSource attackAudio;


    public override void Initialize(StateController stateController) {
        // Turn towards player.
        Vector3 lookTarget = Services.playerTransform.position;
        lookTarget.y = stateController.transform.position.y;
        stateController.transform.LookAt(lookTarget);

        // Make sure there is not an obstacle in the way.
        RaycastHit hit;
        if (Physics.SphereCast(stateController.transform.position, stateController.GetComponent<SphereCollider>().radius, transform.forward, out hit, 10f, 1 << 8)) {
            GetComponent<TriggerTransition>().isTriggerSet = true;
        } else {
            StartCoroutine(AttackCoroutine(stateController));
        }
    }


    IEnumerator AttackCoroutine(StateController stateController) {
        HoveringEnemy controller = stateController as HoveringEnemy;

        controller.m_AnimationController.StartBristleAnimation(bristleAnimationDuration);
        droneAudio.Stop();
        bristleAudio.Play();

        yield return new WaitForSeconds(bristleDuration);

        controller.m_AnimationController.StartAttackAnimation(attackAnimationDuration);
        bristleAudio.Stop();
        attackAudio.Play();

        yield return new WaitForSeconds(attackAnimationDuration * 0.9f);

        // Keep testing attack area until during attack animation.
        float timer = 0f;
        bool playerDamaged = false;
        yield return new WaitUntil(() => {
            if (timer >= attackDuration) {
                return true;
            }

            else {
                timer += Time.deltaTime;
                if (!playerDamaged && PlayerIsInHitBox(stateController)) {
                    Services.gameManager.PlayerWasHurt();
                    playerDamaged = true;
                }

                return false;
            }
        });

        controller.m_AnimationController.StartWithdrawAnimation(attackWithdrawAnimationDuration);

        yield return new WaitForSeconds(postAttackPauseDuration);

        droneAudio.Play();
        GetComponent<TriggerTransition>().isTriggerSet = true;

        yield return null;
    }


    bool PlayerIsInHitBox(StateController stateController) {
        Vector3 hitBoxPosition = hitBox.transform.position;
        hitBoxPosition += Vector3.Scale(hitBox.transform.forward, hitBox.center);
        hitBoxPosition += Vector3.Scale(hitBox.transform.up, hitBox.center);
        hitBoxPosition += Vector3.Scale(hitBox.transform.right, hitBox.center);

        Collider[] collidersInHitBox = Physics.OverlapBox(
            hitBoxPosition, 
            hitBox.bounds.extents * 0.5f, 
            stateController.transform.rotation
            );

        foreach (Collider collider in collidersInHitBox) {
            if (collider.gameObject == Services.playerGameObject) { return true; }
        }

        return false;
    }


    public override void End(StateController stateController) {
    }
}
