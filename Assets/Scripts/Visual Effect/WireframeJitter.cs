using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireframeJitter : MonoBehaviour {

    private void Update() {
        float newThickness = Random.Range(0.2f, 0.6f);
        GetComponent<Renderer>().material.SetFloat("_Thickness", newThickness);
    }
}
