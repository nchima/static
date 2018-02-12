using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class TankEnemyState_Wandering : State {

    [SerializeField] FloatRange wanderDistanceRange = new FloatRange(5f, 30f);

    Vector3 centerPoint;


    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);
        TankEnemy controller = stateController as TankEnemy;

        centerPoint = transform.position;
        controller.m_NavMeshAgent.SetDestination(GetWanderPoint(stateController));
    }


    public override void Run(StateController stateController) {
        base.Run(stateController);
        TankEnemy controller = stateController as TankEnemy;

        // If we've gotten to our previously chosen wander point, choose a new one.
        if (Vector3.Distance(controller.transform.position, controller.m_NavMeshAgent.destination) <= 5f) {
            controller.m_NavMeshAgent.SetDestination(GetWanderPoint(stateController));
        }
    }


    public override void End(StateController stateController) {
    }


    Vector3 GetWanderPoint(StateController stateController) {
        TankEnemy controller = stateController as TankEnemy;
        Vector3 returnPoint = centerPoint;

        // Check to see if the new position is on the navmesh.
        for (int i = 0; i < 100; i++) {
            returnPoint = centerPoint;
            Vector3 modifyDirection = Quaternion.Euler(0f, Random.Range(-180f, 180f), 0f) * Vector3.forward;
            returnPoint = returnPoint + modifyDirection * wanderDistanceRange.Random;

            if (!controller.IsPointOnNavMesh(returnPoint)) {
                continue;
            }
        }

        return returnPoint;
    }
}
