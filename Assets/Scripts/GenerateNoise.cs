using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateNoise : MonoBehaviour {

    public Texture2D noiseTex;
    Color[] pixels;

    private void Start()
    {
        noiseTex = new Texture2D(128, 128);
        noiseTex.filterMode = FilterMode.Point;
    }

    private void FixedUpdate()
    {
        //GenNoise();
    }


    void GenNoise() {
        pixels = noiseTex.GetPixels(0, 0, noiseTex.width, noiseTex.height);
        for (int i = 0; i < pixels.Length; i++) {
            float pixelColor = Random.value;
            pixels[i] = new Color(pixelColor, pixelColor, pixelColor, 1);
            //pixels[i] = Random.ColorHSV();
        }
        noiseTex.SetPixels(pixels);
        noiseTex.Apply();
    }
}
