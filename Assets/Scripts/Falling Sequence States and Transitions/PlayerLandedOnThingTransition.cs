using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandedOnThingTransition : Transition {
    public override bool IsConditionTrue(StateController stateController) {
        // See if player is above a wall, railing or obstacle
        RaycastHit hit;
        if (Physics.SphereCast(Services.playerTransform.position, Services.playerTransform.GetComponent<CapsuleCollider>().radius, Vector3.down, out hit, 10f)) {

            // If the player landed on a bad thing, move them into the level.
            if (hit.collider.tag == "Wall" || hit.collider.tag == "Railing" || hit.collider.tag == "Obstacle") {
                Vector3 moveDirection = -hit.collider.transform.forward;
                Services.playerTransform.position += moveDirection * Services.playerTransform.GetComponent<CapsuleCollider>().radius * 2f;
                return true;
            }
        }

        return false;
    }
}
