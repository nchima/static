using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_Collision : Transition {

    [SerializeField] Collider colliderToCheck;
    protected bool collided;
    float triggerResetTime = 0.1f;
    float triggerResetTimer = 0f;

    private void Start() {
        // Attach a script to the watched collider which will report back when a collision happens.
        CollisionReporter collisionReporter = colliderToCheck.gameObject.AddComponent<CollisionReporter>();
        collisionReporter.reportToScript = this;
    }

    private void Update() {
        if (collided) {
            triggerResetTimer += Time.deltaTime;
            if (triggerResetTimer >= triggerResetTime) {
                triggerResetTimer = 0f;
                collided = false;
            }
        }
    }

    public override bool IsConditionTrue(StateController stateController) {
        if (collided) {
            collided = false;
            return true;
        }
        else {
            return false;
        }
    }

    // This script will be called by the CollisionReporter attached in Start()
    protected virtual void ReportedCollision(Collision collision) {
        collided = true;
    }
}
