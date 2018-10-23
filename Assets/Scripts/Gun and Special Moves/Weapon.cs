using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Weapon : MonoBehaviour {

    public string weaponName;

    [Serializable] public class WeaponValue {
        [SerializeField] private float maxValue;
        [SerializeField] private AnimationCurve curve;

        public float CurrentValue { get { return MyMath.Map(curve.Evaluate(MyMath.Map01(GunValueManager.currentValue, -1f, 1f)), 0f, 1f, 0f, maxValue); } }
        public float CurrentValueInverse { get { return MyMath.Map(curve.Evaluate(MyMath.Map01(GunValueManager.currentValue * -1, -1f, 1f)), 0f, 1f, 0f, maxValue); } }
    }

    public WeaponValue bulletsPerBurst;
    public WeaponValue burstsPerSecond;
    public WeaponValue bulletSpread;
    public WeaponValue bulletThickness;

    private enum CrosshairType { LineRendererReference, Circle }
    [SerializeField] CrosshairType crosshairType = CrosshairType.Circle;
    [Serializable] private class CrosshairCircleInfo {
        public int segments = 12;
        public float radius = 1.7f;
        public float pinch = 0f;
        public bool pinchOddVectors = false;
    }
    [SerializeField] CrosshairCircleInfo crosshairCircleInfo;
    public Vector3[] CrosshairVectors {
        get {
            // Use an attached line renderer component for the crosshair reference.
            if (crosshairType == CrosshairType.LineRendererReference) {
                if (GetComponent<LineRenderer>() == null) {
                    Debug.LogError("Crosshair type for " + weaponName + " weapon was set to use a line renderer as reference, but a line renderer was not attached to its game object.");
                    return null;
                }

                else {
                    Vector3[] positions = new Vector3[GetComponent<LineRenderer>().positionCount];
                    GetComponent<LineRenderer>().GetPositions(positions);
                    return positions;
                }
            }

            // Draw a circle via code
            else {
                return DenGeometry.NerveDamageCrosshair(crosshairCircleInfo.radius, crosshairCircleInfo.segments, crosshairCircleInfo.pinch, crosshairCircleInfo.pinchOddVectors);
            }
        }
    }

    public AudioSource m_AudioSource { get { return GetComponent<AudioSource>(); } }
}
