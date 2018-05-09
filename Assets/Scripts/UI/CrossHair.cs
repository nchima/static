﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CrossHair : MonoBehaviour {

    public int circleSegments;
    [SerializeField] float maxRadius = 1.25f;
    [SerializeField] float minRadius = 0.34f;
    float xradius;
    float yradius;
    LineRenderer line;

    float shakeValueResting = 0.025f;
    public FloatRange shakeRange = new FloatRange(0.1f, 0.4f);
    float shakeValue;


    private void Awake() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerFiredGun>(PlayerFiredGunHandler);
    }


    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();

        line.positionCount = circleSegments + 1;
        line.useWorldSpace = false;
        CircleDrawer.Draw(line, xradius, yradius, circleSegments, shakeValue);

        shakeValue = shakeRange.min;
    }


    private void Update()
    {
        xradius = MyMath.Map(GunValueManager.currentValue, -1f, 1f, maxRadius, minRadius);
        yradius = MyMath.Map(GunValueManager.currentValue, -1f, 1f, maxRadius, minRadius);
        transform.Rotate(new Vector3(0f, Random.Range(-180f, 180f), 0f));
        CircleDrawer.Draw(line, xradius, yradius, circleSegments, shakeValue);

        // Lerp shake value back towards resting value
        shakeValue = Mathf.Lerp(shakeValue, shakeValueResting, 0.4f);
        shakeValue = Mathf.Clamp(shakeValue, shakeValueResting, shakeRange.max);
    }


    void AdjustShakeValueForShotFired() {
        shakeValue = MyMath.Map(GunValueManager.currentValue, 1f, -1f, shakeRange.min * 1.1f, shakeRange.max);
    }


    public void PlayerFiredGunHandler(GameEvent gameEvent) {
        AdjustShakeValueForShotFired();
    }
}
