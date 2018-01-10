using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CrossHair : MonoBehaviour {

    public int segments;
    [SerializeField] float maxRadius = 1.25f;
    [SerializeField] float minRadius = 0.34f;
    float xradius;
    float yradius;
    LineRenderer line;

    public LineRenderer tuningTarget;
    float ttXRadius = 0.5f;
    float ttYRadius = 0.5f;

    float shakeValueResting = 0.025f;
    public FloatRange shakeRange = new FloatRange(0.1f, 0.4f);
    float shakeValue;


    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();

        line.positionCount = segments + 1;
        line.useWorldSpace = false;
        CreatePoints(line, xradius, yradius);

        shakeValue = shakeRange.min;
    }


    private void Update()
    {
        xradius = MyMath.Map(GunValueManager.currentValue, -1f, 1f, maxRadius, minRadius);
        yradius = MyMath.Map(GunValueManager.currentValue, -1f, 1f, maxRadius, minRadius);
        transform.Rotate(new Vector3(0f, Random.Range(-180f, 180f), 0f));
        CreatePoints(line, xradius, yradius);

        shakeValue = Mathf.Lerp(shakeValue, shakeValueResting, 0.4f);
        shakeValue = Mathf.Clamp(shakeValue, shakeValueResting, shakeRange.max);

        //if (tuningTarget.gameObject.activeSelf)
        //{
        //    CreatePoints(tuningTarget, ttXRadius, ttYRadius);
        //}
    }


    void CreatePoints(LineRenderer target, float xRadius, float yRadius)
    {
        float x;
        float y;
        float z = 0f;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++) {
            x = (Mathf.Sin(Mathf.Deg2Rad * angle) * xRadius) + Random.Range(-shakeValue, shakeValue);
            y = 0f;
            z = (Mathf.Cos(Mathf.Deg2Rad * angle) * yRadius) + Random.Range(-shakeValue, shakeValue);

            target.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / segments);
        }
    }


    public void AdjustShakeValueForShotFired() {
        shakeValue = MyMath.Map(GunValueManager.currentValue, 1f, -1f, shakeRange.min * 1.1f, shakeRange.max);
    }

    
    public void UpdateTuningTarget(float value)
    {
        //tuningTarget.gameObject.SetActive(true);

        float newRad = MyMath.Map(value, -1f, 1f, maxRadius, minRadius);

        DOTween.To(() => ttXRadius, x => ttXRadius = x, newRad, 0.2f).SetEase(Ease.InQuad).SetUpdate(true);
        DOTween.To(() => ttYRadius, x => ttYRadius = x, newRad, 0.2f).SetEase(Ease.InQuad).SetUpdate(true);
    }
}
