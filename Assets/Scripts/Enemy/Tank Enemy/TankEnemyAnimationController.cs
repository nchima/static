using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TankEnemyAnimationController : MonoBehaviour {

    [SerializeField] GameObject artilleryBase;
    [SerializeField] GameObject artilleryBaseSheathe;
    [SerializeField] GameObject crystal;
    
    float openCrystalValue = 0f;
    
    [SerializeField] float seigeModeIdleNoiseSpeed;
    [SerializeField] float seigeModeIdleNoiseBase = 10f;
    [SerializeField] float seigeModeIdleNoiseSpread = 10f;

    [SerializeField] float runModeIdleNoiseSpeed;
    [SerializeField] float runModeIdleNoiseBase = 90f;
    [SerializeField] float runModeIdleNoiseSpread = 10f;

    float currentIdleNoiseSpeed;
    float currentIdleNoiseBase;
    float currentIdleNoiseSpread;

    SkinnedMeshRenderer[] blendRenderers;

    PerlinNoise leg1IdleMorphNoise;
    PerlinNoise leg2IdleMorphNoise;
    PerlinNoise leg3IdleMorphNoise;
    PerlinNoise leg4IdleMorphNoise;
    PerlinNoise leg5IdleMorphNoise;
    PerlinNoise leg6IdleMorphNoise;


    private void Start() {
        currentIdleNoiseSpeed = seigeModeIdleNoiseSpeed;
        currentIdleNoiseBase = seigeModeIdleNoiseBase;
        currentIdleNoiseSpread = seigeModeIdleNoiseSpread;

        leg1IdleMorphNoise = new PerlinNoise(seigeModeIdleNoiseSpeed);
        leg2IdleMorphNoise = new PerlinNoise(seigeModeIdleNoiseSpeed);
        leg3IdleMorphNoise = new PerlinNoise(seigeModeIdleNoiseSpeed);
        leg4IdleMorphNoise = new PerlinNoise(seigeModeIdleNoiseSpeed);
        leg5IdleMorphNoise = new PerlinNoise(seigeModeIdleNoiseSpeed);
        leg6IdleMorphNoise = new PerlinNoise(seigeModeIdleNoiseSpeed);
    }


    private void Update() {
        IdleMorph();

        foreach (SkinnedMeshRenderer renderer in crystal.GetComponentsInChildren<SkinnedMeshRenderer>()) {
            if (renderer.GetBlendShapeWeight(0) != openCrystalValue) { renderer.SetBlendShapeWeight(0, openCrystalValue); }
        }
    }


    void IdleMorph() {
        //float tempNoiseSpeed = 

        leg1IdleMorphNoise.Iterate();
        leg2IdleMorphNoise.Iterate();
        leg3IdleMorphNoise.Iterate();
        leg4IdleMorphNoise.Iterate();
        leg5IdleMorphNoise.Iterate();
        leg6IdleMorphNoise.Iterate();

        SkinnedMeshRenderer[] legMeshRenderers = artilleryBase.GetComponentsInChildren<SkinnedMeshRenderer>();
        legMeshRenderers[0].SetBlendShapeWeight(0, leg1IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);
        legMeshRenderers[0].SetBlendShapeWeight(0, leg1IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);
        legMeshRenderers[1].SetBlendShapeWeight(0, leg2IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);
        legMeshRenderers[2].SetBlendShapeWeight(0, leg3IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);
        legMeshRenderers[3].SetBlendShapeWeight(0, leg4IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);
        legMeshRenderers[4].SetBlendShapeWeight(0, leg5IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);
        legMeshRenderers[5].SetBlendShapeWeight(0, leg6IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);

        legMeshRenderers = artilleryBaseSheathe.GetComponentsInChildren<SkinnedMeshRenderer>();
        legMeshRenderers[0].SetBlendShapeWeight(0, leg1IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);
        legMeshRenderers[1].SetBlendShapeWeight(0, leg2IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);
        legMeshRenderers[2].SetBlendShapeWeight(0, leg3IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);
        legMeshRenderers[3].SetBlendShapeWeight(0, leg4IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);
        legMeshRenderers[4].SetBlendShapeWeight(0, leg5IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);
        legMeshRenderers[5].SetBlendShapeWeight(0, leg6IdleMorphNoise.MapValue(currentIdleNoiseBase - currentIdleNoiseSpread, currentIdleNoiseBase + currentIdleNoiseSpread).x);
    }


    public void StartOpenCrystalAnimation(float duration) {
        DOTween.To(() => openCrystalValue, x => openCrystalValue = x, 100, duration).SetEase(Ease.InExpo);
    }


    public void StartCloseCrystalAnimation(float duration) {
        DOTween.To(() => openCrystalValue, x => openCrystalValue = x, 0, duration).SetEase(Ease.InExpo);
    }


    public void SetSeigeMode(bool active) {
        if (active) {
            currentIdleNoiseSpeed = seigeModeIdleNoiseSpeed;
            currentIdleNoiseSpread = seigeModeIdleNoiseSpread;
            DOTween.To(() => currentIdleNoiseBase, x => currentIdleNoiseBase = x, seigeModeIdleNoiseBase, 0.4f).SetEase(Ease.OutExpo);
        }

        else {
            currentIdleNoiseSpeed = 0f;
            currentIdleNoiseSpread = 0f;
            DOTween.To(() => currentIdleNoiseBase, x => currentIdleNoiseBase = x, runModeIdleNoiseBase, 0.4f).SetEase(Ease.OutExpo);
        }
    }


    public void EnterRunMode() {
        currentIdleNoiseSpeed = runModeIdleNoiseSpeed;
        currentIdleNoiseSpread = runModeIdleNoiseSpread;
    }
}
