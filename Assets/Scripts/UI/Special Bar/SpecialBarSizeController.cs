using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpecialBarSizeController : MonoBehaviour {

    [SerializeField] Transform barTransform;
    [SerializeField] FloatRange sizeRange = new FloatRange(0.1f, 2f);   // How large and small the bar can be.

    float _percentageFilled;
    [HideInInspector] public float percentageFilled {
        get {
            return _percentageFilled;
        }

        set {
            _percentageFilled = value;

            // Set the bar's scale based on it's percentage.
            float newYScale = MyMath.Map(_percentageFilled, 0f, 1f, sizeRange.min, sizeRange.max);
            barTransform.DOScaleY(newYScale, tweenSpeed);
        }
    }

    [SerializeField] private float originPosition = -2.9f;   // The local position of the special bar's lowest point.
    [SerializeField] private float tweenSpeed = 0.4f;    // How quickly the special bar changes size.


    private void Update() {
        // Set the bar's Y position so that it still starts from it's original bottom.
        float newYPos = originPosition + barTransform.lossyScale.y * 0.5f;
        barTransform.localPosition = new Vector3(
                barTransform.localPosition.x,
                newYPos,
                barTransform.localPosition.z
            );
    }
}
