using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallCatcher : MonoBehaviour {


    private void OnTriggerEnter(Collider other) {
        if (other.gameObject == GameManager.player) {
            if (GameManager.healthManager.playerHealth <= 0) { return; }
            if (GameManager.fallingSequenceManager.isPlayerFalling) { GameManager.fallingSequenceManager.BeginFallingInstant(); } else { GameManager.fallingSequenceManager.BeginFalling(); }
        }
    }
}
