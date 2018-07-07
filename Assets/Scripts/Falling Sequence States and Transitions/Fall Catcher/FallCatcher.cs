using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallCatcher : MonoBehaviour {

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject == Services.playerGameObject) {
            if (Services.healthManager.currentHealth <= 0) { return; }
            if (!Services.fallingSequenceManager.isPlayerFalling) {
                Services.fallingSequenceManager.BeginFallingInstant();
            } else {
                Services.fallingSequenceManager.BeginFalling();
            }
        }
    }
}
