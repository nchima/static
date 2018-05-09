using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPickup : Pickup {

    [SerializeField] int pointValue;
    [SerializeField] LineRenderer circleRenderer;

    private void Update() {
        float radius = pickupTrigger.GetComponent<SphereCollider>().radius;
        CircleDrawer.Draw(circleRenderer, radius, radius, 20, 0.2f);
    }

    protected override void GetAbsorbed() {
        base.GetAbsorbed();
        Services.scoreManager.score += pointValue;
    }
}
