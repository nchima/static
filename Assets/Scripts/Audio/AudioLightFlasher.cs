using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLightFlasher : MonoBehaviour {

    [SerializeField] FlashyLight[] flashyLights;
    [SerializeField] FloatRange decibelRange = new FloatRange(0f, 1f);

    float maxDb = 0f;
    float minDb = 1f;


    void Update() {

        if (Input.GetKeyDown(KeyCode.P)) { PrintSpectrumDataToConsole(); }

        Graph();

        // Get average decibels of audio.
        float[] spectrum = new float[256];
        float sum = 0;
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
        for (int i = 0; i < spectrum.Length; i++) {
            sum += spectrum[i] * spectrum[i];
        }

        float rms = Mathf.Sqrt(sum / spectrum.Length);
        float db = 20 * Mathf.Log10(rms / 0.1f);
        if (db < -160) db = -160;

        if (db > maxDb) { maxDb = db; }
        if (db < minDb) { minDb = db; }

        //Debug.Log("Average decibels: " + db);
        //Debug.Log("Min dB: " + minDb);
        //Debug.Log("Max dB: " + maxDb);

        // Change the intensity of each light.
        for (int i = 0; i < flashyLights.Length; i++) {
            flashyLights[i].light.intensity = MyMath.Map(db, decibelRange.max, decibelRange.min, flashyLights[i].intensityRange.min, flashyLights[i].intensityRange.max);
            flashyLights[i].light.intensity = Mathf.Clamp(flashyLights[i].light.intensity, flashyLights[i].intensityRange.min, flashyLights[i].intensityRange.max);
        }
    }

    void PrintSpectrumDataToConsole() {
        float[] spectrum = new float[256];
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
        for (int i = 0; i < spectrum.Length; i++) { Debug.Log(i + ": " + spectrum[i]); }
    }

    void Graph() {
        float[] spectrum = new float[256];

        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

        for (int i = 1; i < spectrum.Length - 1; i++) {
            Debug.DrawLine(new Vector3(i - 1, spectrum[i] + 10, 0), new Vector3(i, spectrum[i + 1] + 10, 0), Color.red);
            Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]) + 10, 2), new Vector3(i, Mathf.Log(spectrum[i]) + 10, 2), Color.cyan);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), spectrum[i - 1] - 10, 1), new Vector3(Mathf.Log(i), spectrum[i] - 10, 1), Color.green);
            Debug.DrawLine(new Vector3(Mathf.Log(i - 1), Mathf.Log(spectrum[i - 1]), 3), new Vector3(Mathf.Log(i), Mathf.Log(spectrum[i]), 3), Color.blue);
        }
    }

    [Serializable]
    class FlashyLight {
        public Light light;
        public FloatRange intensityRange = new FloatRange(0f, 1f);
    }
}
