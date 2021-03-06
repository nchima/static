using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemyStateShooting : State {

    //[SerializeField] float inaccuracy;
    [SerializeField] float preShotDelay;  // How long I take to 'charge up' my shot.
    [SerializeField] float postShotDelay; // How long I pause after firing before I take a new action.
    [SerializeField] GameObject shotPrefab;
    [SerializeField] Transform bulletOrigin;


    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);

        // Not sure why I'm doing this - look into it later. (Does it have to do with the enemies firing shots at the wrong point during their animation?)
        if (stateController.GetComponent<Animator>() != null) stateController.GetComponent<Animator>().speed = 1.0f / preShotDelay;

        StartCoroutine(FiringSequence(stateController));
    }


    public override void Run(StateController stateController) {
        base.Run(stateController);
    }


    IEnumerator FiringSequence(StateController stateController) {
        SimpleEnemy controller = stateController as SimpleEnemy;

        if (controller.navMeshAgent != null) controller.navMeshAgent.enabled = false;

        // Begin the charging up animation.
        controller.animationController.StartAttackAnimation(preShotDelay, postShotDelay);

        foreach (ParticleSystem particleSystem in controller.chargeParticles.GetComponentsInChildren<ParticleSystem>()) {
            particleSystem.Stop();
            particleSystem.Play();
        }

        yield return new WaitForSeconds(preShotDelay);

        // Fire a shot.
        FireShot(controller);

        // Set the shot timer for the post shot delay.
        yield return new WaitForSeconds(postShotDelay);

        GetComponent<TriggerTransition>().isTriggerSet = true;

        yield return null;
    }


    void FireShot(SimpleEnemy controller) {
        GameObject newShot = Instantiate(shotPrefab, bulletOrigin.position, Quaternion.identity);
        newShot.GetComponent<EnemyShot>().firedEnemy = gameObject;

        // Get the direction towards the player.
        Vector3 targetPosition = Services.playerTransform.position;
        targetPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        Vector3 direction = targetPosition - transform.position;

        /* I'll keep this inaccuracy stuff commented. For now, let's see how it works if this enemy just fire's it's bullet directly towards the player's position. */
        //float scaledInaccuracy = MyMath.Map(GameManager.player.GetComponent<Rigidbody>().velocity, maxGroundSpeed);   // This line's incomplete btw.
        //direction = Quaternion.Euler(0, Random.Range(-inaccuracy, inaccuracy), 0) * direction;

        direction.Normalize();

        newShot.GetComponent<NormalShot>().Direction = direction;
    }
}
