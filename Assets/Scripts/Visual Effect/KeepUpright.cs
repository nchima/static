using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepUpright : MonoBehaviour {

    float minimumVisibleDistance = 50f;
    float maxDistance = 100f;

    float widthAtMaxDistance = 0.5f;

    private void Awake() {
        Vector3 newScale = transform.localScale;
        newScale.y = 625f;
        transform.localScale = newScale;
    }

    private void Update () {
		if (Services.fallingSequenceManager.isPlayerFalling) {
			transform.localScale = new Vector3 (widthAtMaxDistance * 0.1f, transform.localScale.y, widthAtMaxDistance * 0.1f);
		} else {
			ChangeScaleBasedOnPlayerDistance ();
		}

        // Keep self upright
        //transform.rotation = Quaternion.identity;
        transform.rotation = Services.playerTransform.Find("Cameras").transform.rotation;
    }


	void ChangeScaleBasedOnPlayerDistance() {
		// Get distance from player.
		float distance = Vector3.Distance(transform.position, Services.playerTransform.position);

		// Change width based on distance from player.
		if (GetComponentInParent<EnemyOld>() != null && (distance < minimumVisibleDistance && GetComponentInParent<EnemyOld> ().canSeePlayer)
            || GetComponentInParent<Enemy>() != null && (distance < minimumVisibleDistance && GetComponentInParent<Enemy>().CanSeePlayer)) {
			transform.localScale = new Vector3 (0f, transform.localScale.y, 0f);
		}

		else {
			float newWidth = MyMath.Map(distance, minimumVisibleDistance, maxDistance, 0f, 0.5f);
			transform.localScale = new Vector3(newWidth, transform.localScale.y, newWidth);
		}
	}
}
