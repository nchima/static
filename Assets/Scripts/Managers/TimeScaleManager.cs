using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TimeScaleManager : MonoBehaviour {

    [SerializeField] float normalTimeScale = 1f;

    Tween timeScaleTween;
    float memorizedTimeScale;
    public bool IsAtFullSpeed { get { return Time.timeScale == normalTimeScale; } }
    bool keepAtZero;

    private void Update() {
        if (keepAtZero) { Time.timeScale = 0; }
    }

    public void Pause(bool value) {
        if (timeScaleTween != null) { timeScaleTween.TogglePause(); }
        keepAtZero = value;

        if (value == true) {
            memorizedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        } else {
            Time.timeScale = memorizedTimeScale;
        }
    }

    private void ResetGunFiring() {
        Services.gun.fireOnEveryMouseDown = false;
    }
    
    public void ReturnToFullSpeed(float duration) {
        Invoke("ResetGunFiring", duration);
        TweenTimeScale(normalTimeScale, duration);
        DOTween.To(() => Services.gun.burstsPerSecondSloMoModifierCurrent, x => Services.gun.burstsPerSecondSloMoModifierCurrent = x, 1f, duration).SetEase(Ease.InQuad).SetUpdate(true);
        Services.musicManager.ReturnMusicPitchToFullSpeed(duration);
    }

    public void TweenTimeScale(float targetValue, float duration) {
        if (timeScaleTween != null) { timeScaleTween.Kill(); }
        // Debug.Log(duration);
        timeScaleTween = DOTween.To(() => Time.timeScale, x => Time.timeScale = x, targetValue, duration).SetEase(Ease.InQuad).SetUpdate(true);
    }
}
