using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialMoveRefillZone : MonoBehaviour {

    [SerializeField] LineRenderer circleRenderer;

    const float REFILL_RATE = 0.5f;
    bool isRefilling;

    private void Update() {
        float radius = GetComponentInChildren<SphereCollider>().radius;
        CircleDrawer.Draw(circleRenderer, radius, radius, 20, 0.2f);

        if (isRefilling) {
            Services.specialBarManager.AddValue(REFILL_RATE * Time.deltaTime);
        }
    }

    private void OnTriggerEnterChild(Collider other) {
        if (other.gameObject == Services.playerGameObject) {
            isRefilling = true;
        }
    }

    private void OnTriggerExitChild(Collider other) {
        if (other.gameObject == Services.playerGameObject) {
            isRefilling = false;
        }
    }
}
