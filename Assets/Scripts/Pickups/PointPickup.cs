using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPickup : Pickup {

    public int scoreValue;
    [SerializeField] LineRenderer circleRenderer;

    private void Start() {
        transform.position = new Vector3(transform.position.x, 2.9f, transform.position.z);
    }

    private void Update() {
        float radius = pickupTrigger.GetComponent<SphereCollider>().radius;
        CircleDrawer.Draw(circleRenderer, radius, radius, 20, 0.2f);
    }

    public override void BeginMovingTowardsPlayer() {
        Services.scoreManager.IncreaseMultiplier();
        Services.scorePopupManager.CreatePositionalPopup(transform.position, scoreValue);
        Services.scoreManager.Score += scoreValue;
        base.BeginMovingTowardsPlayer();
    }
}
