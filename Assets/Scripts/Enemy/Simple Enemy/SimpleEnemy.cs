using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleEnemy : Enemy {

    public FloatRange timeBetweenShotsRange;    // How often I fire a shot.
    [HideInInspector] public NavMeshAgent navMeshAgent;
    public GameObject shotPrefab;


    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
}
