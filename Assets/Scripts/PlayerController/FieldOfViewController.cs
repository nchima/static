using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FieldOfViewController : MonoBehaviour {

    [SerializeField] Transform pullbackPoint;
    [SerializeField] GameObject backgroundClearVeil1;
    [SerializeField] GameObject backgroundClearVeil2;
    [SerializeField] GameObject foregroundClearVeil1;
    [SerializeField] GameObject foregroundClearVeil2;

    FloatRange fieldOfViewRange = new FloatRange(58f, 85f);
    FloatRange orthographicSizeRange = new FloatRange(15f, 32f);

    [HideInInspector] public List<Camera> perspectiveCams;
    [HideInInspector] public List<Camera> orthographicCams;

    FloatRange shotgunChargeFOVRange = new FloatRange(85f, 100f);
    FloatRange shotgunChargeOrthoSizeRange = new FloatRange(32f, 50f);
    float tweenDuration = 0.6f;

    Tween fovTween;
    Tween orthoSizeTween;

    bool freezeUpdate = false;

    float _currentFOV;
    public float CurrentFOV {
        get {
            return _currentFOV;
        }

        set {
            foreach (Camera camera in perspectiveCams) { camera.fieldOfView = value; }
            _currentFOV = value;
        }
    }

    float _currentOrthoSize;
    public float CurrentOrthoSize {
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
        orthographicCams = new List<Camera>();
        Camera[] camerasInChildren = GetComponentsInChildren<Camera>();
        for (int i = 0; i < camerasInChildren.Length; i++) {
            if (camerasInChildren[i].orthographic) { camerasInChildren[i].enabled = false;  orthographicCams.Add(camerasInChildren[i]); }
            else { perspectiveCams.Add(camerasInChildren[i]); }
        }
    }

    private void Update() {
        SetFieldOfView(GunValueManager.currentValue);
    }

    void SetFieldOfView(float sineValue) {
        if (freezeUpdate) { return; }
        CurrentFOV = MyMath.Map(sineValue, 1f, -1f, fieldOfViewRange.min, fieldOfViewRange.max);
        CurrentOrthoSize = MyMath.Map(sineValue, 1f, -1f, orthographicSizeRange.min, orthographicSizeRange.max);
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

    public void SetClearVeilActive(bool value) {
        backgroundClearVeil1.SetActive(value);
        foregroundClearVeil1.SetActive(!value);
        foregroundClearVeil2.SetActive(value);
    }

    public void TweenFieldOfView(float targetFOV, float targetOrthoSize, float duration) {
        DOTween.To(() => CurrentFOV, x => CurrentFOV = x, targetFOV, duration);
        DOTween.To(() => CurrentOrthoSize, x => CurrentOrthoSize = x, targetOrthoSize, duration);
    }

    public void SetFieldOfView(float targetFOV, float targetOrthoSize) {
        CurrentFOV = targetFOV;
        CurrentOrthoSize = targetOrthoSize;
    }

    public void ActivateCameraClearing(bool value) {
        CameraClearFlags newClearFlags;
        if (value == true) { newClearFlags = CameraClearFlags.SolidColor; }
        else { newClearFlags = CameraClearFlags.Depth; }

        perspectiveCams[0].clearFlags = newClearFlags;
    }
}
