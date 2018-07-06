using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleEnemy : Enemy {

    [HideInInspector] public NavMeshAgent navMeshAgent;
    public GameObject shotPrefab;
    public GameObject chargeParticles;
    public SimpleEnemyAnimationController animationController { get { return myGeometry.GetComponent<SimpleEnemyAnimationController>(); } }


    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
}