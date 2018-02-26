﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HoveringEnemyAnimationController : MonoBehaviour {

    [SerializeField] int bristleIndex = 0;
    [SerializeField] int attackIndex = 0;
    [SerializeField] int idleMorph1Index = 0;
    [SerializeField] int idleMorph2Index = 0;

    [SerializeField] float idleMorphSpeed;
    [SerializeField] FloatRange idleMorph1NoiseRange = new FloatRange(1f, 100f);
    [SerializeField] FloatRange idleMorph2NoiseRange = new FloatRange(1f, 100f);

    SkinnedMeshRenderer[] blendRenderers;

    float currentBristleValue = 0f;
    float currentAttackValue = 0f;

    float currentIdleMorph1Value = 0f;
    float currentIdleMorph2Value = 0f;

    PerlinNoise idleMorph1Noise;
    PerlinNoise idleMorph2Noise;


    private void Start() {
        blendRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        idleMorph1Noise = new PerlinNoise(idleMorphSpeed);
        idleMorph2Noise = new PerlinNoise(idleMorphSpeed);
    }


    private void Update() {
        IdleMorph();

        foreach (SkinnedMeshRenderer renderer in blendRenderers) {
            if (renderer.GetBlendShapeWeight(bristleIndex) != currentBristleValue) { renderer.SetBlendShapeWeight(bristleIndex, currentBristleValue); }
            if (renderer.GetBlendShapeWeight(attackIndex) != currentAttackValue) { renderer.SetBlendShapeWeight(attackIndex, currentAttackValue); }
            if (renderer.GetBlendShapeWeight(idleMorph1Index) != currentIdleMorph1Value) { renderer.SetBlendShapeWeight(idleMorph1Index, currentIdleMorph1Value); }
            if (renderer.GetBlendShapeWeight(idleMorph2Index) != currentIdleMorph2Value) { renderer.SetBlendShapeWeight(idleMorph2Index, currentIdleMorph2Value); }
        }
    }


    void IdleMorph() {
        idleMorph1Noise.Iterate();
        idleMorph2Noise.Iterate();

        currentIdleMorph1Value = idleMorph1Noise.MapValue(idleMorph1NoiseRange.min, idleMorph1NoiseRange.max).x;
        currentIdleMorph2Value = idleMorph2Noise.MapValue(idleMorph2NoiseRange.min, idleMorph2NoiseRange.max).x;
    }


    public void StartBristleAnimation(float duration) {
        DOTween.To(() => currentBristleValue, x => currentBristleValue = x, 90, duration).SetEase(Ease.InExpo);
    }


    public void StartAttackAnimation(float duration) {
        DOTween.To(() => currentAttackValue, x => currentAttackValue = x, 100f, duration).SetEase(Ease.InExpo);
        DOTween.To(() => currentBristleValue, x => currentBristleValue = x, 0f, duration).SetEase(Ease.InExpo);
    }


    public void StartWithdrawAnimation(float duration) {
        DOTween.To(() => currentAttackValue, x => currentAttackValue = x, 0f, duration).SetEase(Ease.InExpo);
    }
}
