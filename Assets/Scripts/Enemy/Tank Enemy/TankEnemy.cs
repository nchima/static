using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TankEnemy : Enemy {

    public float runAwayDistance = 30f;
    [SerializeField] FloatRange shotTimeRange = new FloatRange(3f, 6f);
    public TankEnemyAnimationController animationController;

    float nextShotTime;
    float shotTimer;
    [HideInInspector] public bool shotTrigger;


    protected override void Start() {
        base.Start();
        nextShotTime = shotTimeRange.min;
    }

    protected override void Update() {
        base.Update();
        shotTimer += Time.deltaTime;
        if (shotTimer >= nextShotTime) {
            shotTrigger = true;
        }
    }

    public void ResetShotTimer() {
        nextShotTime = shotTimeRange.Random;
        shotTimer = 0f;
        shotTrigger = false;
    }
}
    