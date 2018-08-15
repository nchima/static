using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SimpleEnemyAnimationController : EnemyAnimationController {

    enum AnimationState { Pause, TweenPrep, WaitForCoroutine }
    AnimationState animationState = AnimationState.Pause;

    float baseBlendWeight0;
    float baseBlendWeight1;
    float baseBlendWeight2;
    float currentBlendWeight0;
    float currentBlendWeight1;
    float currentBlendWeight2;
    float timeInNextPauseState;
    float pauseTimer = 0f;
    FloatRange vibrationTimeRange = new FloatRange(0.2f, 0.4f);
    float vibrationIntensityRange = 20f;


    protected override void Start() {
        base.Start();

        currentBlendWeight0 = Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange);
        currentBlendWeight1 = Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange);
        currentBlendWeight2 = Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange);
    }


    void Update() {
        switch(animationState) {
            case AnimationState.Pause:
                RunPauseState();
                break;
            case AnimationState.TweenPrep:
                RunTweenPrepState();
                break;
            case AnimationState.WaitForCoroutine:
                // Dont do anything
                break;
        }

        // Update blend values & apply vibration
        foreach (SkinnedMeshRenderer renderer in blendRenderers) {
            renderer.SetBlendShapeWeight(0, currentBlendWeight0 + Random.Range(-vibrationIntensityRange, vibrationIntensityRange));
            renderer.SetBlendShapeWeight(1, currentBlendWeight1 + Random.Range(-vibrationIntensityRange, vibrationIntensityRange));
            renderer.SetBlendShapeWeight(2, currentBlendWeight2 + Random.Range(-vibrationIntensityRange, vibrationIntensityRange));
        }
    }


    void RunPauseState() {
        pauseTimer += Time.deltaTime;
        if (pauseTimer >= timeInNextPauseState) {
            animationState = AnimationState.TweenPrep;
            return;
        }
    }


    void RunTweenPrepState() {
        timeInNextPauseState = vibrationTimeRange.Random;
        pauseTimer = 0f;
        if (tweenBlendWeightsCoroutine != null) { StopCoroutine(tweenBlendWeightsCoroutine); }
        tweenBlendWeightsCoroutine = StartCoroutine(TweenBlendWeightsCoroutine());
        animationState = AnimationState.WaitForCoroutine;
    }


    Coroutine tweenBlendWeightsCoroutine;
    IEnumerator TweenBlendWeightsCoroutine() {
        float duration = Random.Range(0.8f, 1f);
        Vector3 newRotation = Random.rotation.eulerAngles;
        transform.DOLocalRotate(newRotation, duration).SetEase(Ease.Linear);
        activeTweens.Add(DOTween.To(() => currentBlendWeight0, x => currentBlendWeight0 = x, Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange), duration).SetEase(Ease.Linear));
        activeTweens.Add(DOTween.To(() => currentBlendWeight1, x => currentBlendWeight1 = x, Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange), duration).SetEase(Ease.Linear));
        activeTweens.Add(DOTween.To(() => currentBlendWeight2, x => currentBlendWeight2 = x, Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange), duration).SetEase(Ease.Linear));
        yield return new WaitForSeconds(duration);
        baseBlendWeight0 = currentBlendWeight0;
        baseBlendWeight1 = currentBlendWeight1;
        baseBlendWeight2 = currentBlendWeight2;
        animationState = AnimationState.Pause;
        yield return null;
    }


    public void StartAttackAnimation(float preShotDelay, float postShotDelay) {
        animationState = AnimationState.WaitForCoroutine;
        if (tweenBlendWeightsCoroutine != null) { StopCoroutine(tweenBlendWeightsCoroutine); }
        activeCoroutines.Add(StartCoroutine(AttackCoroutine(preShotDelay, postShotDelay)));
    }


    IEnumerator AttackCoroutine(float preShotDelay, float postShotDelay) {
        // Tween blend values to zero
        activeTweens.Add(DOTween.To(() => currentBlendWeight0, x => currentBlendWeight0 = x, 0f, preShotDelay * 0.8f));
        activeTweens.Add(DOTween.To(() => currentBlendWeight1, x => currentBlendWeight1 = x, 0f, preShotDelay * 0.8f));
        activeTweens.Add(DOTween.To(() => currentBlendWeight2, x => currentBlendWeight2 = x, 0f, preShotDelay * 0.8f));

        // Expand
        transform.DOScale(1.3f, preShotDelay);

        // Rotate faster and faster until timer expires.
        float rotationSpeed = 0.01f;
        float timer = 0f;
        yield return new WaitUntil(() => {
            timer += Time.deltaTime;
            if (timer >= preShotDelay) {
                return true;
            }

            else {
                transform.Rotate(Random.Range(-0.1f, 0.1f), rotationSpeed, Random.Range(-0.1f, 0.1f));
                if (Time.deltaTime != 0) rotationSpeed *= 1.3f;
                return false;
            }
        });

        // [Shot is fired (See SimpleEnemyState_Attacking script or whatever it's called)]

        // Shrink
        transform.DOScale(1f, postShotDelay);

        activeTweens.Add(DOTween.To(() => currentBlendWeight0, x => currentBlendWeight0 = x, Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange), postShotDelay).SetEase(Ease.Linear));
        activeTweens.Add(DOTween.To(() => currentBlendWeight1, x => currentBlendWeight1 = x, Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange), postShotDelay).SetEase(Ease.Linear));
        activeTweens.Add(DOTween.To(() => currentBlendWeight2, x => currentBlendWeight2 = x, Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange), postShotDelay).SetEase(Ease.Linear));

        // Rotate slower and slower until timer expires.
        timer = 0f;
        yield return new WaitUntil(() => {
            timer += Time.deltaTime;
            if (timer >= postShotDelay) {
                return true;
            } else {
                transform.Rotate(Random.Range(-0.1f, 0.1f), rotationSpeed, Random.Range(-0.1f, 0.1f));
                rotationSpeed *= 0.99f;
                return false;
            }
        });

        animationState = AnimationState.Pause;

        yield return null;
    }
}
