using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySheatheGrower : MonoBehaviour {

    const float MAX_SCALE_MULTIPLIER = 5f;
    float playerSpawnHeight = 2000f;
    Vector3 originalScale;


    private void Awake() {
        originalScale = transform.localScale;
    }


    private void Update() {
        if (Services.fallingSequenceManager.isPlayerFalling) {
            transform.localScale = originalScale * Mathf.Clamp(MyMath.Map(Services.playerTransform.position.y, 0f, playerSpawnHeight, 1f, MAX_SCALE_MULTIPLIER), 1f, MAX_SCALE_MULTIPLIER);
        } else {
            transform.localScale = originalScale;
        }
    }
}
