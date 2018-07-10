using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CrossHair : MonoBehaviour {

    [SerializeField] FloatRange circleSegmentsRange = new FloatRange(8f, 12f);
    int circleSegments { get { return Mathf.FloorToInt(GunValueManager.MapToFloatRangeInverted(circleSegmentsRange)); } }
    [SerializeField] float maxRadius = 1.25f;
    [SerializeField] float minRadius = 0.34f;
    float xRadius;
    float yRadius;
    LineRenderer line;

    float shakeValueResting = 0.05f;
    public FloatRange shakeRange = new FloatRange(0.1f, 0.4f);
    float shakeValue;


    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerFiredGun>(PlayerFiredGunHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerFiredGun>(PlayerFiredGunHandler);
    }


    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();

        line.positionCount = circleSegments + 1;
        line.useWorldSpace = false;
        CircleDrawer.Draw(line, xRadius, yRadius, circleSegments, shakeValue);

        shakeValue = shakeRange.min;
    }


    int lastCircleSegments = 0;
    private void Update() {
        xRadius = MyMath.Map(GunValueManager.currentValue, -1f, 1f, maxRadius, minRadius);
        yRadius = MyMath.Map(GunValueManager.currentValue, -1f, 1f, maxRadius, minRadius);

        if (circleSegments != lastCircleSegments && circleSegments != circleSegmentsRange.min) {
            Vector3 newRotation = transform.localRotation.eulerAngles;
            newRotation.x = Random.Range(-180f, 180f);
            transform.localRotation = Quaternion.Euler(newRotation);
        } else if (circleSegments == circleSegmentsRange.min) {
            Debug.Log("stabilizing crosshair.");
            Vector3 newRotation = transform.localRotation.eulerAngles;
            newRotation.x = 20f;
            transform.localRotation = Quaternion.Euler(newRotation);
        }

        lastCircleSegments = circleSegments;

        // Rotate crosshair a little just for fun
        Vector3 newNewRotation = transform.localRotation.eulerAngles;
        newNewRotation.y = Random.Range(66.5f, 106.5f);
        newNewRotation.z = Random.Range(66.5f, 106.5f);
        transform.localRotation = Quaternion.Euler(newNewRotation);


        CircleDrawer.DrawCrosshair(line, xRadius, yRadius, circleSegments, shakeValue);

        // Lerp shake value back towards resting value
        shakeValue = Mathf.Lerp(shakeValue, shakeValueResting, 0.4f);
        shakeValue = Mathf.Clamp(shakeValue, shakeValueResting, shakeRange.max);
    }


    void AdjustShakeValueForShotFired() {
        shakeValue = MyMath.Map(GunValueManager.currentValue, 1f, -1f, shakeRange.max * 1.1f, shakeRange.min);
    }


    public void PlayerFiredGunHandler(GameEvent gameEvent) {
        AdjustShakeValueForShotFired();
    }
}
