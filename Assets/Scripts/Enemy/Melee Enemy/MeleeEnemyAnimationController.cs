using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MeleeEnemyAnimationController : EnemyAnimationController {

    [SerializeField] int chargeWindupIndex = 0;
    [SerializeField] int chargeReleaseIndex = 0;
    [SerializeField] int idleMorph1Index = 0;
    [SerializeField] int idleMorph2Index = 0;

    float currentChargeWindupValue = 0f;
    float currentChargeReleaseValue = 0f;
    float currentIdleMorph1Value = 0f;
    float currentIdleMorph2Value = 0f;

    PerlinNoise idleMorph1Noise;
    PerlinNoise idleMorph2Noise;


    protected override void Start() {
        base.Start();
        blendRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        idleMorph1Noise = new PerlinNoise(idleMorphSpeed);
        idleMorph2Noise = new PerlinNoise(idleMorphSpeed);
    }

    private void Update() {
        IdleMorph();

        foreach (SkinnedMeshRenderer renderer in blendRenderers) {
            if (renderer.GetBlendShapeWeight(chargeWindupIndex) != currentChargeWindupValue) { renderer.SetBlendShapeWeight(chargeWindupIndex, currentChargeReleaseValue); }
            if (renderer.GetBlendShapeWeight(chargeReleaseIndex) != currentChargeReleaseValue) { renderer.SetBlendShapeWeight(chargeReleaseIndex, currentChargeReleaseValue); }
            if (renderer.GetBlendShapeWeight(idleMorph1Index) != currentIdleMorph1Value) { renderer.SetBlendShapeWeight(idleMorph1Index, currentIdleMorph1Value); }
            if (renderer.GetBlendShapeWeight(idleMorph2Index) != currentIdleMorph2Value) { renderer.SetBlendShapeWeight(idleMorph2Index, currentIdleMorph2Value); }
        }
    }

    void IdleMorph() {
        idleMorph1Noise.Iterate();
        idleMorph2Noise.Iterate();

        currentIdleMorph1Value = idleMorph1Noise.MapValue(idleMorphNoiseRange.min, idleMorphNoiseRange.max).x;
        currentIdleMorph2Value = idleMorph2Noise.MapValue(idleMorphNoiseRange.min, idleMorphNoiseRange.max).x;
    }


    public void StartChargeWindupAnimation(float duration) {
        DOTween.To(() => currentChargeWindupValue, x => currentChargeWindupValue = x, 90, duration).SetEase(Ease.InExpo);
    }

    public void StartChargeReleaseAnimation(float duration) {
        DOTween.To(() => currentChargeWindupValue, x => currentChargeWindupValue = x, 1, duration).SetEase(Ease.InExpo);
        DOTween.To(() => currentChargeReleaseValue, x => currentChargeReleaseValue = x, 100, duration).SetEase(Ease.InExpo);
    }

    public void EndChargeReleaseAnimation(float duration) {
        DOTween.To(() => currentChargeReleaseValue, x => currentChargeReleaseValue = x, 1, duration).SetEase(Ease.OutExpo);
    }
}
