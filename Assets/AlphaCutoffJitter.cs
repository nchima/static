using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlphaCutoffJitter : MonoBehaviour {

    Material material;
    FloatRange minMax = new FloatRange(0.1f, 1f);

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }


    private void Update()
    {
        material.SetFloat("_AlphaCutOff", minMax.Random);
    }
}
