using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepBehind : MonoBehaviour {

    [SerializeField] float distance = 3f;

    private void Update()
    {
        Vector3 directionFromPlayer = Vector3.Normalize(transform.parent.position - GameManager.player.transform.position);
        Vector3 newPosition = transform.parent.position + (directionFromPlayer * distance);
        newPosition.y = transform.parent.position.y;
        transform.position = newPosition;

        // Rotate
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, Random.Range(-180f, 180f));
    }
}
