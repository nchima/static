using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepBehind : MonoBehaviour {

    [SerializeField] float distance = 3f;

    float maxFallingScale = 2f;
    float originalScale;

    private void Start() {
        originalScale = transform.localScale.x;
    }

    private void Update()
    {
        Vector3 directionFromPlayer = Vector3.Normalize(transform.parent.position - GameManager.player.transform.position);
        Vector3 newPosition = transform.parent.position + (directionFromPlayer * distance);
        newPosition.y = transform.parent.position.y;
        transform.position = newPosition;

        // Rotate
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, Random.Range(-180f, 180f));

        if (GameManager.fallingSequenceManager.isPlayerFalling) {
            float newRadius = MyMath.Map(GameManager.player.transform.position.y, 0f, 1000f, originalScale, maxFallingScale);
            transform.localScale = new Vector3(newRadius, newRadius, 1f);
        }
    }
}
