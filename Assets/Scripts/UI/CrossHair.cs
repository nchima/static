using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CrossHair : MonoBehaviour {

    [SerializeField] FloatRange circleSegmentsRange = new FloatRange(8f, 12f);
    int positionCount { get { return Mathf.FloorToInt(GunValueManager.MapToFloatRangeInverted(circleSegmentsRange)); } }
    LineRenderer m_LineRenderer;

    float shakeValueResting = 0.05f;
    public FloatRange shakeRange = new FloatRange(0.1f, 0.4f);
    float shakeValue;

    [HideInInspector] public Vector3[] upperWeaponPositions;
    [HideInInspector] public Vector3[] lowerWeaponPositions;

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerFiredGun>(PlayerFiredGunHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerFiredGun>(PlayerFiredGunHandler);
    }

    void Start() {
        m_LineRenderer = gameObject.GetComponent<LineRenderer>();

        m_LineRenderer.positionCount = positionCount + 1;
        m_LineRenderer.useWorldSpace = false;
        //CircleDrawer.Draw(m_LineRenderer, xRadius, yRadius, circleSegments, shakeValue);

        upperWeaponPositions = Services.gun.upperWeapon.CrosshairVectors;
        lowerWeaponPositions = Services.gun.lowerWeapon.CrosshairVectors;

        shakeValue = shakeRange.min;
    }

    int lastPositionCount = 0;
    private void Update() {
        // Draw the crosshair:
        int positionCount = Mathf.FloorToInt(GunValueManager.MapTo(lowerWeaponPositions.Length, upperWeaponPositions.Length));
        Vector3[] positions = new Vector3[positionCount];
        m_LineRenderer.positionCount = positionCount;

        for (int i = 0; i < positions.Length; i++) {
            Vector3 newPosition = Vector3.zero;

            int upperVectorIndex = Mathf.FloorToInt(MyMath.Map(i, 0, positions.Length, 0, upperWeaponPositions.Length));
            int lowerVectorIndex = Mathf.FloorToInt(MyMath.Map(i, 0, positions.Length, 0, lowerWeaponPositions.Length));
            newPosition = Vector3.Lerp(lowerWeaponPositions[lowerVectorIndex], upperWeaponPositions[upperVectorIndex], GunValueManager.MapTo(0f, 1f));
            newPosition.x += Random.Range(-shakeValue, shakeValue);
            newPosition.z += Random.Range(-shakeValue, shakeValue);

            positions[i] = newPosition;
        }

        m_LineRenderer.SetPositions(positions);

        //float radius = MyMath.Map(GunValueManager.currentValue, -1f, 1f, maxRadius, minRadius);
        //yRadius = MyMath.Map(GunValueManager.currentValue, -1f, 1f, maxRadius, minRadius);

        //if (positionCount != lastPositionCount && positionCount != circleSegmentsRange.min) {
        //    Vector3 newRotation = transform.localRotation.eulerAngles;
        //    newRotation.x = Random.Range(-180f, 180f);
        //    transform.localRotation = Quaternion.Euler(newRotation);
        //}
        //else if (positionCount == circleSegmentsRange.min) {
        //    Vector3 newRotation = transform.localRotation.eulerAngles;
        //    newRotation.x = 20f;
        //    transform.localRotation = Quaternion.Euler(newRotation);
        //}

        lastPositionCount = positionCount;

        // Rotate crosshair a little just for fun
        //Vector3 newNewRotation = transform.localRotation.eulerAngles;
        //newNewRotation.y = Random.Range(66.5f, 106.5f);
        //newNewRotation.z = Random.Range(66.5f, 106.5f);
        //transform.localRotation = Quaternion.Euler(newNewRotation);

        //CircleDrawer.DrawCrosshair(m_LineRenderer, xRadius, yRadius, positionCount, shakeValue);

        // Lerp shake value back towards resting value
        shakeValue = Mathf.Lerp(shakeValue, shakeValueResting, 0.4f);
        shakeValue = Mathf.Clamp(shakeValue, shakeValueResting, shakeRange.max);
    }

    void AdjustShakeValueForShotFired() {
        shakeValue = MyMath.Map(Services.gun.BulletsPerBurst, Services.gun.bulletsPerBurstRange.min, Services.gun.bulletsPerBurstRange.max, shakeRange.min, shakeRange.max * 1.1f);
    }

    public void PlayerFiredGunHandler(GameEvent gameEvent) {
        AdjustShakeValueForShotFired();
    }
}
