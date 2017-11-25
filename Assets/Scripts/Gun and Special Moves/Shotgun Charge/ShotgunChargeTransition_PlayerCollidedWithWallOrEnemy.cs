using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunChargeTransition_PlayerCollidedWithWallOrEnemy : Transition_Collision {

    public override bool IsConditionTrue(StateController stateController) {
        return base.IsConditionTrue(stateController);
    }

    protected override void ReportedCollision(Collision collision) {
        if (collision.collider.GetComponent<Enemy>() || collision.collider.name.ToLower().Contains("wall") || collision.collider.name.ToLower().Contains("obstacle")) {
            Debug.Log("Collision was reported with " + collision.collider.name);
            collided = true;
        }
    }
}
