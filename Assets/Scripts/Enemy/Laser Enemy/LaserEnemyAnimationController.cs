using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LaserEnemyAnimationController : MonoBehaviour {

    [SerializeField] int dashWindupIndex = 0;
    [SerializeField] int dashReleaseIndex = 0;
    [SerializeField] int shootChargeUpIndex = 0;
    [SerializeField] int shootReleaseIndex = 0;
    [SerializeField] int idleMorph1Index = 0;
    [SerializeField] int idleMorph2Index = 0;
    [SerializeField] int idleMorph3Index = 0;

    [SerializeField] float idleMorphSpeed;
    [SerializeField] FloatRange idleMorphNoiseRange = new FloatRange(1f, 100f);

    SkinnedMeshRenderer[] blendRenderers;

    float currentDashWindupValue = 0f;
    float currentDashReleaseValue = 0f;
    float currentShootChargeUpValue = 0f;
    float currentShootReleaseValue = 0f;
    float currentIdleMorph1Value = 0f;
    float currentIdleMorph2Value = 0f;
    float currentIdleMorph3Value = 0f;

    PerlinNoise idleMorph1Noise;
    PerlinNoise idleMorph2Noise;
    PerlinNoise idleMorph3Noise;


    private void Start() {
        blendRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        idleMorph1Noise = new PerlinNoise(idleMorphSpeed);
        idleMorph2Noise = new PerlinNoise(idleMorphSpeed);
        idleMorph3Noise = new PerlinNoise(idleMorphSpeed);
    }

    private void Update() {
        IdleMorph();

        foreach (SkinnedMeshRenderer renderer in blendRenderers) {
            if (renderer.GetBlendShapeWeight(dashReleaseIndex) != currentDashReleaseValue) { renderer.SetBlendShapeWeight(dashReleaseIndex, currentDashReleaseValue); }
            if (renderer.GetBlendShapeWeight(dashWindupIndex) != currentDashWindupValue) { renderer.SetBlendShapeWeight(dashWindupIndex, currentDashWindupValue); }
            if (renderer.GetBlendShapeWeight(shootChargeUpIndex) != currentShootChargeUpValue) { renderer.SetBlendShapeWeight(shootChargeUpIndex, currentShootChargeUpValue); }
            if (renderer.GetBlendShapeWeight(shootReleaseIndex) != currentShootReleaseValue) { renderer.SetBlendShapeWeight(shootReleaseIndex, currentShootReleaseValue); }
            if (renderer.GetBlendShapeWeight(idleMorph1Index) != currentIdleMorph1Value) { renderer.SetBlendShapeWeight(idleMorph1Index, currentIdleMorph1Value); }
            if (renderer.GetBlendShapeWeight(idleMorph2Index) != currentIdleMorph2Value) { renderer.SetBlendShapeWeight(idleMorph2Index, currentIdleMorph2Value); }
            if (renderer.GetBlendShapeWeight(idleMorph3Index) != currentIdleMorph3Value) { renderer.SetBlendShapeWeight(idleMorph3Index, currentIdleMorph3Value); }
        }
    }

    void IdleMorph() {
        idleMorph1Noise.Iterate();
        idleMorph2Noise.Iterate();
        idleMorph3Noise.Iterate();

        currentIdleMorph1Value = idleMorph1Noise.MapValue(idleMorphNoiseRange.min, idleMorphNoiseRange.max).x;
        currentIdleMorph2Value = idleMorph2Noise.MapValue(idleMorphNoiseRange.min, idleMorphNoiseRange.max).x;
        currentIdleMorph3Value = idleMorph3Noise.MapValue(idleMorphNoiseRange.min, idleMorphNoiseRange.max).x;
    }

    public void StartDashWindupAnimation(float duration) {
        DOTween.To(() => currentDashWindupValue, x => currentDashWindupValue = x, 90, duration).SetEase(Ease.InExpo);
    }

    public void StartDashReleaseAnimation(float duration) {
        DOTween.To(() => currentDashWindupValue, x => currentDashWindupValue = x, 1, duration).SetEase(Ease.InExpo);
        DOTween.To(() => currentDashReleaseValue, x => currentDashReleaseValue = x, 100, duration).SetEase(Ease.InExpo);
    }

    public void EndDashReleaseAnimation(float duration) {
        DOTween.To(() => currentDashReleaseValue, x => currentDashReleaseValue = x, 1, duration).SetEase(Ease.OutExpo);
    }

    public void StartShootAnimation(float duration) {
        DOTween.To(() => currentShootChargeUpValue, x => currentShootChargeUpValue = x, 90, duration).SetEase(Ease.OutExpo);
    }

    public void StartShootReleaseAnimation(float duration) {
        DOTween.To(() => currentShootChargeUpValue, x => currentShootChargeUpValue = x, 1, duration).SetEase(Ease.InExpo);
        DOTween.To(() => currentShootReleaseValue, x => currentShootReleaseValue = x, 90, duration).SetEase(Ease.InExpo);
    }

    public void EndShootAnimation(float duration) {
        DOTween.To(() => currentShootReleaseValue, x => currentShootReleaseValue = x, 1, duration).SetEase(Ease.InExpo);
    }
}