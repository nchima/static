using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankEnemyState_RunFromPlayer : State {

    [SerializeField] float runAwayDistanceMax = 50f;
    [SerializeField] float runAwaySpeed = 30f;
    float originalSpeed = 0;


    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);
        TankEnemy controller = stateController as TankEnemy;
        if (originalSpeed == 0) { originalSpeed = controller.m_NavMeshAgent.speed; }
        StartCoroutine(BeginRunningAway(controller));
    }

    public override void End(StateController stateController) {
        TankEnemy controller = stateController as TankEnemy;
        controller.m_NavMeshAgent.speed = originalSpeed;
    }

    IEnumerator BeginRunningAway(TankEnemy controller) {
        controller.animationController.SetSeigeMode(false);
        controller.m_NavMeshAgent.isStopped = true;

        yield return new WaitForSeconds(0.5f);

        controller.animationController.EnterRunMode();

        controller.m_NavMeshAgent.speed = runAwaySpeed;
        controller.m_NavMeshAgent.SetDestination(ChooseRunAwayDestination(controller));
        controller.m_NavMeshAgent.isStopped = false;

        yield return null;
    }

    public Vector3 ChooseRunAwayDestination(StateController stateController) {
        TankEnemy controller = stateController as TankEnemy;

        Vector3 destination = controller.transform.position;
        for (float distance = runAwayDistanceMax; distance > 0; distance -= 1f) {
            destination = controller.transform.position;
            Vector3 modifyDirection = Vector3.Normalize(Quaternion.Euler(0f, Random.Range(-180f, 180f), 0f) * Vector3.forward);
            destination += modifyDirection * distance;
            if (controller.IsPointOnNavMesh(destination)) {
                return destination;
            }
        }

        return destination;
    }
}
