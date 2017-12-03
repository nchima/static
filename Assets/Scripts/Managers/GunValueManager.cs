using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunValueManager : MonoBehaviour {

    [HideInInspector] public static float currentValue = 0f;
    float previousGunValue = 0f;

    [SerializeField] GameObject wiper;

    void Update() {

        // Change current gun value based on mouse Y movement.
        currentValue += Input.GetAxis("Mouse Y") * 0.1f;
        currentValue = Mathf.Clamp(currentValue, -1f, 1f);

        // Handle wiper
        if (Mathf.Abs(previousGunValue - currentValue) >= 0.01f) {
            wiper.transform.localScale = new Vector3(
                MyMath.Map(Mathf.Abs(previousGunValue - currentValue), 0f, 0.5f, 0f, 10f),
                wiper.transform.localScale.y,
                wiper.transform.localScale.z
            );
        } else {
            wiper.transform.localScale = new Vector3(
                    0f,
                    wiper.transform.localScale.y,
                    wiper.transform.localScale.z
                );
        }

        wiper.transform.localPosition = new Vector3(
            wiper.transform.localPosition.x,
            MyMath.Map(currentValue, -1f, 1f, -25f, 11f),
            wiper.transform.localPosition.z
            );

        previousGunValue = currentValue;

        // Set music based on current sine value.
        GameManager.musicManager.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.SetFloat("FilterCutoff", (currentValue + 1f) * 11000f + 200f);
        GameManager.musicManager.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer
            .SetFloat("FilterCutoff", MyMath.Map(currentValue, -1f, 1f, 10000f, 20000f));
    }
}
