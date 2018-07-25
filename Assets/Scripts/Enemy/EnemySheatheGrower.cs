using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySheatheGrower : MonoBehaviour {

    float maxScaleMultiplier = 10f;
    float playerSpawnHeight = 2000f;
    Vector3 originalScale;


    private void Awake() {
        originalScale = transform.localScale;
    }


    private void Update() {
        if (Services.fallingSequenceManager.isPlayerFalling) {
            transform.localScale = originalScale * MyMath.Map(Services.playerTransform.position.y, 0f, playerSpawnHeight, 1f, 10f);
        } else {
            transform.localScale = originalScale;
        }
    }
}
