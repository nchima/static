using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepUpright : MonoBehaviour {

    float minimumVisibleDistance = 50f;
    float maxDistance = 100f;

    float widthAtMaxDistance = 0.5f;

	private void Update () {
		if (GameManager.fallingSequenceManager.isPlayerFalling) {
			transform.localScale = new Vector3 (widthAtMaxDistance * 0.1f, transform.localScale.y, widthAtMaxDistance * 0.1f);
		} else {
			ChangeScaleBasedOnPlayerDistance ();
		}

        // Keep self upright
        //transform.rotation = Quaternion.identity;
        transform.rotation = GameManager.player.transform.Find("Cameras").transform.rotation;
    }


	void ChangeScaleBasedOnPlayerDistance() {
		// Get distance from player.
		float distance = Vector3.Distance(transform.position, GameManager.player.transform.position);

		// Change width based on distance from player.
		if (GetComponentInParent<EnemyOld>() != null && (distance < minimumVisibleDistance && GetComponentInParent<EnemyOld> ().canSeePlayer)
            || GetComponentInParent<Enemy>() != null && (distance < minimumVisibleDistance && GetComponentInParent<Enemy>().canSeePlayer)) {
			transform.localScale = new Vector3 (0f, transform.localScale.y, 0f);
		}

		else {
			float newWidth = MyMath.Map(distance, minimumVisibleDistance, maxDistance, 0f, 0.5f);
			transform.localScale = new Vector3(newWidth, transform.localScale.y, newWidth);
		}
	}
}
