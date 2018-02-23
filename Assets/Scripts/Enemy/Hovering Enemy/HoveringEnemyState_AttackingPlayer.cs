using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoveringEnemyState_AttackingPlayer : State {

    [SerializeField] float bristleAnimationDuration = 0.1f;
    [SerializeField] float bristleDuration = 1f;
    [SerializeField] float attackAnimationDuration = 0.34f;
    [SerializeField] float attackLength = 1f;
    [SerializeField] float attackWithdrawAnimationDuration = 0.7f;
    [SerializeField] float postAttackPauseDuration = 1f;
    [SerializeField] BoxCollider hitBox;


    public override void Initialize(StateController stateController) {
        // Turn towards player.
        Vector3 lookTarget = GameManager.player.transform.position;
        lookTarget.y = stateController.transform.position.y;
        stateController.transform.LookAt(lookTarget);

        StartCoroutine(AttackCoroutine(stateController));
    }


    IEnumerator AttackCoroutine(StateController stateController) {
        HoveringEnemy controller = stateController as HoveringEnemy;

        controller.m_AnimationController.StartBristleAnimation(bristleAnimationDuration);

        yield return new WaitForSeconds(bristleDuration);

        controller.m_AnimationController.StartAttackAnimation(attackAnimationDuration);

        yield return new WaitForSeconds(attackAnimationDuration * 0.9f);

        // Keep testing attack area until during attack animation.
        float timer = 0f;
        yield return new WaitUntil(() => {
            if (timer >= attackLength) {
                return true;
            }

            else {
                timer += Time.deltaTime;
                if (PlayerIsInHitBox(stateController)) { GameManager.instance.PlayerWasHurt(); }
                return false;
            }
        });

        controller.m_AnimationController.StartWithdrawAnimation(attackWithdrawAnimationDuration);

        yield return new WaitForSeconds(postAttackPauseDuration);

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
        DebugUtil.DrawBox(hitBoxPosition, hitBox.bounds.extents * 0.5f, stateController.transform.rotation, Color.red);
        foreach (Collider collider in collidersInHitBox) {
            if (collider.gameObject == GameManager.player) { return true; }
        }

        return false;
    }


    public override void End(StateController stateController) {
    }
}
