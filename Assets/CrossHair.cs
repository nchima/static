using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour {

    private void Update()
    {
        GetComponent<MeshRenderer>().material.mainTexture = GameManager.instance.noiseGenerator.noiseTex;

        float newScale = MyMath.Map(GameManager.instance.currentSine, -1f, 1f, 3f, 0.6696f);
        transform.localScale = new Vector3(newScale, transform.localScale.y, newScale);
    }
}
