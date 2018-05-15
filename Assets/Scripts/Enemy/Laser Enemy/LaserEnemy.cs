using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LaserEnemy : Enemy {

    [SerializeField] int maxTimesToDash = 3;
    public GameObject laserPrefab;
    [SerializeField] EnemyWeakPointGrower weakpoint;

    [HideInInspector] public int timesDashed = 0;
    [HideInInspector] public int timesToDash;
    [HideInInspector] public LaserEnemyAnimationController animationController;


    void Awake () {
        weakpoint.myDad = this;
        animationController = GetComponentInChildren<LaserEnemyAnimationController>();
	}

    public void DetermineTimesToDash() {
        timesToDash = Random.Range(0, maxTimesToDash);
    }
}