using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FieldOfViewController : MonoBehaviour {

    [SerializeField] Transform pullbackPoint;

    FloatRange fieldOfViewRange = new FloatRange(58f, 85f);
    FloatRange orthographicSizeRange = new FloatRange(15f, 32f);

    List<Camera> perspectiveCams;
    List<Camera> orthographicCams;

    FloatRange shotgunChargeFOVRange = new FloatRange(85f, 100f);
    FloatRange shotgunChargeOrthoSizeRange = new FloatRange(32f, 50f);
    float tweenDuration = 0.6f;

    Tween fovTween;
    Tween orthoSizeTween;

    bool freezeUpdate = false;

    float _currentFOV;
    float currentFOV {
        get {
            return _currentFOV;
        }

        set {
            for (int i = 0; i < perspectiveCams.Count; i++) { perspectiveCams[i].fieldOfView = value; }
            _currentFOV = value;
        }
    }

    float _currentOrthoSize;
    float currentOrthoSize {
        get {
            return _currentOrthoSize;
        }

        set {
            for (int i = 0; i < orthographicCams.Count; i++) { orthographicCams[i].orthographicSize = value; }
            _currentOrthoSize = value;
        }
    }


    private void Awake() {
        // Go through all cameras in my children and add them to the right array.
        perspectiveCams = new List<Camera>();
        orthographicCams = new List<Camera>();
        Camera[] camerasInChildren = GetComponentsInChildren<Camera>();
        for (int i = 0; i < camerasInChildren.Length; i++) {
            if (camerasInChildren[i].orthographic) { orthographicCams.Add(camerasInChildren[i]); }
            else { perspectiveCams.Add(camerasInChildren[i]); }
        }
    }

    private void Update() {
        SetFieldOfView(GunValueManager.currentValue);
    }

    void SetFieldOfView(float sineValue) {
        if (freezeUpdate) { return; }
        currentFOV = MyMath.Map(sineValue, 1f, -1f, fieldOfViewRange.min, fieldOfViewRange.max);
        currentOrthoSize = MyMath.Map(sineValue, 1f, -1f, orthographicSizeRange.min, orthographicSizeRange.max);
    }

    Tween moveCameraTween;
    Tween rotateCameraTween;
    Vector3 originalLocalPosition = new Vector3(0f, 1.1f, 0f);
    public void TweenToShotgunChargeFOV() {
        TweenFieldOfView(shotgunChargeFOVRange.max, shotgunChargeOrthoSizeRange.max, 0.4f);

        if (moveCameraTween != null && moveCameraTween.IsPlaying()) { moveCameraTween.Kill(); }
        if (rotateCameraTween != null && rotateCameraTween.IsPlaying()) { rotateCameraTween.Kill(); }
        moveCameraTween = transform.DOLocalMove(pullbackPoint.transform.localPosition, 0.4f).SetEase(Ease.OutQuad);
        rotateCameraTween = transform.DOLocalRotate(pullbackPoint.transform.localRotation.eulerAngles, 0.4f).SetEase(Ease.OutElastic);

        freezeUpdate = true;
    }

    public void TweenToNormalFOV() {
        TweenFieldOfView(shotgunChargeFOVRange.min, shotgunChargeOrthoSizeRange.min, 0.14f);

        if (moveCameraTween != null && moveCameraTween.IsPlaying()) { moveCameraTween.Kill(); }
        if (rotateCameraTween != null && rotateCameraTween.IsPlaying()) { rotateCameraTween.Kill(); }
        moveCameraTween = transform.DOLocalMove(originalLocalPosition, 0.14f);
        rotateCameraTween = transform.DOLocalRotate(Vector3.zero, 0.14f);

        freezeUpdate = false;
    }

    void TweenFieldOfView(float targetFOV, float targetOrthoSize, float duration) {
        DOTween.To(() => currentFOV, x => currentFOV = x, targetFOV, duration);
        DOTween.To(() => currentOrthoSize, x => currentOrthoSize = x, targetOrthoSize, duration);
    }
}
