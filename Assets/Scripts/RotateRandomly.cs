using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRandomly : MonoBehaviour {

    private void Update()
    {
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + Random.insideUnitSphere);
    }
}
