using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunValueManager : MonoBehaviour {
    private static float internalValue = 0f;
    [HideInInspector] public static float currentValue = 0f;
    float previousGunValue = 0f;
    [SerializeField] GameObject wiper;
    public AnimationCurve tuningCurve;

    bool inNotch;
    public float unNotchThresh;
    public float notchInThresh;
    float notchedValue;
    public float notchDistThresh;
    public float notchInSpeed;


    void Update() {

        // Change current gun value based on mouse Y movement.
        float inputMod = 10f;
        if (InputManager.inputMode == InputManager.InputMode.Controller) { inputMod = -2.9f; }
        if (Time.timeScale == 0) { inputMod = 0; }
        else { inputMod *= (1 / Time.timeScale); }

        //if (inNotch && Mathf.Abs(InputManager.gunTuningValue) > unNotchThresh) {
        //    inNotch = false;
        //} else if (!inNotch && Mathf.Abs(InputManager.gunTuningValue) < notchInThresh) {
        //    CheckNotch(-1f);
        //    CheckNotch(0f);
        //    CheckNotch(1f);
        //}
        //if (!inNotch)
            internalValue += InputManager.gunTuningValue * inputMod * Time.deltaTime;
        //else
        //    internalValue += Mathf.Clamp((notchedValue - internalValue), -notchInSpeed * Time.deltaTime, notchInSpeed * Time.deltaTime);

       // internalVlaue += InputManager.gunTuningValue * inputMod * Time.deltaTime;
        internalValue = Mathf.Clamp(internalValue, -1f, 1f);

        currentValue = internalValue;//tuningCurve.Evaluate(internalVlaue);






        // Handle wiper
        if (Mathf.Abs(previousGunValue - internalValue) >= 0.01f) {
            wiper.transform.localScale = new Vector3(
                MyMath.Map(Mathf.Abs(previousGunValue - internalValue), 0f, 0.5f, 0f, 10f),
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
            MyMath.Map(internalValue, -1f, 1f, -25f, 11f),
            wiper.transform.localPosition.z
            );

        previousGunValue = internalValue;
    }
    void CheckNotch(float notch) {
        if (Mathf.Abs(notch - internalValue) < notchDistThresh) {
            inNotch = true;
            notchedValue = notch;
        }
    }
    
    public static float MapToFloatRange(FloatRange floatRange) {
        return MyMath.Map(internalValue, -1f, 1f, floatRange.min, floatRange.max);
    }

    public static float MapToFloatRangeInverted(FloatRange floatRange) {
        return MyMath.Map(internalValue, -1f, 1f, floatRange.max, floatRange.min);
    }

    public static float MapTo(float min, float max) {
        return MyMath.Map(internalValue, -1f, 1f, min, max);
    }
}
