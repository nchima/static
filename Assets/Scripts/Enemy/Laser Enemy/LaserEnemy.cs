using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LaserEnemy : Enemy {

    [SerializeField] int maxTimesToDash = 3;
    public GameObject laserPrefab;

    [HideInInspector] public int timesDashed = 0;
    [HideInInspector] public int timesToDash;
    [HideInInspector] public NavMeshAgent m_NavMeshAgent;

    [HideInInspector] public LaserEnemyAnimationController animationController;


    void Awake () {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        animationController = GetComponentInChildren<LaserEnemyAnimationController>();
	}

    public void DetermineTimesToDash() {
        timesToDash = Random.Range(0, maxTimesToDash);
    }
}