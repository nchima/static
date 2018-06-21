using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunValueManager : MonoBehaviour {

    [HideInInspector] public static float currentValue = 0f;
    float previousGunValue = 0f;
    [SerializeField] GameObject wiper;


    void Update() {

        // Change current gun value based on mouse Y movement.
        float inputMod = 10f;
        if (InputManager.inputMode == InputManager.InputMode.Controller) { inputMod = -2.9f; }
        inputMod *= (1 / Time.timeScale);
        currentValue += InputManager.gunTuningValue * inputMod * (Time.deltaTime);
        currentValue = Mathf.Clamp(currentValue, -1f, 1f);

        // Handle wiper
        if (Mathf.Abs(previousGunValue - currentValue) >= 0.01f) {
            wiper.transform.localScale = new Vector3(
                MyMath.Map(Mathf.Abs(previousGunValue - currentValue), 0f, 0.5f, 0f, 10f),
                wiper.transform.localScale.y,
                wiper.transform.localScale.z
            );
        }
        
        else {
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
    }

    
    public static float MapToFloatRange(FloatRange floatRange) {
        return MyMath.Map(currentValue, -1f, 1f, floatRange.min, floatRange.max);
    }

    public static float MapToFloatRangeInverted(FloatRange floatRange) {
        return MyMath.Map(currentValue, -1f, 1f, floatRange.max, floatRange.min);
    }

    public static float MapTo(float min, float max) {
        return MyMath.Map(currentValue, -1f, 1f, min, max);
    }
}
