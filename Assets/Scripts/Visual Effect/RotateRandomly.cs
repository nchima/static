using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRandomly : MonoBehaviour {

    [HideInInspector] public float rotationScale = 0.01f;

    private void Update() {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + Random.insideUnitSphere * rotationScale);
    }
}
