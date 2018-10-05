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
    float vibrationIntensityRange = 1f;
    float idleMorphTime = 0f;


    protected override void Start() {
        base.Start();

        currentBlendWeight0 = Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange);
        currentBlendWeight1 = Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange);
        currentBlendWeight2 = Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange);

        idleMorphTime = Random.Range(-1000f, 1000f);
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
            //renderer.SetBlendShapeWeight(2, currentBlendWeight2 + Random.Range(-vibrationIntensityRange, vibrationIntensityRange));
        }

        //currentBlendWeight0 = MyMath.Map(Mathf.PerlinNoise(idleMorphTime, 0f), -1f, 1f, 0f, 100);
        //idleMorphTime += idleMorphSpeed;
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
        float duration = Random.Range(0.4f, 0.7f);
        Vector3 newRotation = Random.rotation.eulerAngles;
        newRotation.x = 0f;
        newRotation.z = 0f;
        transform.DOLocalRotate(newRotation, duration).SetEase(Ease.Linear);
        idleMorphSpeed = 0f;
        float newBlendWeight = MyMath.Wrap(currentBlendWeight0 + Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange), 0f, 100f);
        DOTween.To(() => currentBlendWeight0, x => currentBlendWeight0 = x, newBlendWeight, duration).SetEase(Ease.Linear);
        //DOTween.To(() => currentBlendWeight1, x => currentBlendWeight1 = x, Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange), duration).SetEase(Ease.Linear);
        //DOTween.To(() => currentBlendWeight2, x => currentBlendWeight2 = x, Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange), duration).SetEase(Ease.Linear);
        duration = 0.4f;
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
        StartCoroutine(AttackCoroutine(preShotDelay, postShotDelay));
    }


    IEnumerator AttackCoroutine(float preShotDelay, float postShotDelay) {

        float duration = preShotDelay * 0.3f;

        // Tween blend values to zero
        DOTween.To(() => currentBlendWeight0, x => currentBlendWeight0 = x, 0f, duration * 0.8f);
        DOTween.To(() => currentBlendWeight1, x => currentBlendWeight1 = x, 100f, duration * 0.8f);

        // Expand
        transform.DOScale(1.3f, duration);

        // Rotate sideways
        foreach (SkinnedMeshRenderer renderer in blendRenderers) {
            renderer.transform.DOBlendableLocalRotateBy(new Vector3(0f, 0f, 90f), duration * 0.8f);
        }

        

        yield return new WaitForSeconds(duration);

        duration = preShotDelay * 0.7f;

        // Rotate faster and faster until timer expires.
        float timer = 0f;
        yield return new WaitUntil(() => {
            timer += Time.deltaTime;
            if (timer >= duration) {
                return true;
            }

            else {
                transform.LookAt(Services.playerTransform.position);
                return false;
            }
        });

        // [Shot is fired (See SimpleEnemyState_Attacking script or whatever it's called)]

        // Shrink
        transform.DOScale(1f, postShotDelay);

        transform.DOLocalRotate(new Vector3(0f, 0f, 0f), postShotDelay);

        DOTween.To(() => currentBlendWeight0, x => currentBlendWeight0 = x, Random.Range(vibrationIntensityRange, 100f - vibrationIntensityRange), postShotDelay).SetEase(Ease.Linear);
        DOTween.To(() => currentBlendWeight1, x => currentBlendWeight1 = x, 0f, postShotDelay).SetEase(Ease.Linear);

        // Rotate slower and slower until timer expires.
        timer = 0f;
        yield return new WaitUntil(() => {
            timer += Time.deltaTime;
            if (timer >= postShotDelay) {
                return true;
            } else {
                return false;
            }
        });

        foreach (SkinnedMeshRenderer renderer in blendRenderers) {
            renderer.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }

        animationState = AnimationState.Pause;

        yield return null;
    }
}
