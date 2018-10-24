using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallCatcher : MonoBehaviour {

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject == Services.playerGameObject) {
            if (Services.healthManager.CurrentHealth <= 0) { return; }
            Services.fallingSequenceManager.BeginFalling();
        }
    }
}
