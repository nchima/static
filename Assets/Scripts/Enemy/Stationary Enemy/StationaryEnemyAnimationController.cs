using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StationaryEnemyAnimationController : EnemyAnimationController {

    [SerializeField] private float orbScaleMultiplier = 1.7f;
    [SerializeField] private float orbRiseHeight = 15f;
    [SerializeField] private GameObject baseParent;
    [SerializeField] private GameObject orbParent;

    private float openTopValue = 0f;
    private float orbBloomValue = 0f;
    private float orbAngerValue = 0f;
    private float angrinessFactor = 0f;

    private SkinnedMeshRenderer[] baseMeshRenderers;
    private SkinnedMeshRenderer[] orbMeshRenderers;

    private Vector3 originalOrbPosition;
    private Vector3 originalOrbScale;

    private void Awake() {
        baseMeshRenderers = baseParent.GetComponentsInChildren<SkinnedMeshRenderer>();
        orbMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        originalOrbPosition = orbParent.transform.localPosition;
        originalOrbScale = orbParent.transform.localScale;
    }

    private void Update() {
        float trueBloomValue = orbBloomValue;
        if (angrinessFactor > 0) {
            float randomness = Random.Range(0, MyMath.Map(angrinessFactor, 0f, 100f, 0f, 75f));
            orbAngerValue = Mathf.Clamp(angrinessFactor + Random.Range(-randomness, randomness), 0, 100f);
            trueBloomValue = Mathf.Clamp(trueBloomValue + Random.Range(-randomness, randomness), 0f, orbAngerValue);
        }

        foreach(SkinnedMeshRenderer renderer in baseMeshRenderers) { renderer.SetBlendShapeWeight(0, openTopValue); }
        foreach(SkinnedMeshRenderer renderer in orbMeshRenderers) {
            renderer.SetBlendShapeWeight(0, orbAngerValue);
            //renderer.SetBlendShapeWeight(1, trueBloomValue);
        }

        orbParent.transform.LookAt(Services.playerTransform);
    }

    Tween openTopTween;
    public void OpenTop(float finalValue, float duration) {
        if (openTopTween != null) {
            openTopTween.Complete();
            openTopValue = 0f;
        }
        openTopTween = DOTween.To(() => openTopValue, x => openTopValue = x, finalValue, duration);
    }

    Tween orbBloomTween;
    public void BloomOrb(float finalValue, float duration) {
        if (orbBloomTween != null) {
            orbBloomTween.Kill();
        }
        orbBloomTween = DOTween.To(() => orbBloomValue, x => orbBloomValue = x, finalValue, duration);
    }

    Tween angrinessTween;
    public void MakeOrbAngry(float finalValue, float duration) {
        if (angrinessTween != null) {
            angrinessTween.Kill();
        }
        angrinessTween = DOTween.To(() => angrinessFactor, x => angrinessFactor = x, finalValue, duration);
    }

    Tween orbRiseTween;
    Tween orbGrowTween;
    public void OrbRise(float duration) {
        if (orbRiseTween != null) { orbRiseTween.Kill(); }
        if (orbGrowTween != null) { orbGrowTween.Kill(); }
        Vector3 risePosition = originalOrbPosition + Vector3.up * orbRiseHeight;
        Vector3 riseScale = originalOrbScale * orbScaleMultiplier;
        orbParent.transform.localPosition = originalOrbPosition;
        orbParent.transform.localScale = originalOrbScale;
        orbRiseTween = orbParent.transform.DOLocalMove(risePosition, duration);
        orbGrowTween = orbParent.transform.DOScale(riseScale, duration);
    }


    public void OrbDescend(float duration) {
        if (orbRiseTween != null) { orbRiseTween.Kill(); }
        if (orbGrowTween != null) { orbGrowTween.Kill(); }
        if (orbBloomTween != null) { orbBloomTween.Kill(); }
        if (angrinessTween != null) { angrinessTween.Kill(); }
        orbParent.transform.localPosition = originalOrbPosition + Vector3.up * orbRiseHeight;
        orbParent.transform.localScale = originalOrbScale * orbScaleMultiplier;
        orbRiseTween = orbParent.transform.DOLocalMove(originalOrbPosition, duration);
        orbGrowTween = orbParent.transform.DOScale(originalOrbScale, duration);
        orbBloomTween = DOTween.To(() => orbBloomValue, x => orbBloomValue = x, 0, duration);
        angrinessTween = DOTween.To(() => angrinessFactor, x => angrinessFactor = x, 0, duration);
    }
}
