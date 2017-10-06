using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepUpright : MonoBehaviour {

    float minimumVisibleDistance = 30f;
    float maxDistance = 100f;

    float widthAtMaxDistance = 0.5f;

	private void Update ()
    {
		if (GameManager.instance.playerState == GameManager.PlayerState.FallingIntoLevel) {
			transform.localScale = new Vector3 (widthAtMaxDistance * 0.1f, transform.localScale.y, widthAtMaxDistance * 0.1f);
		} else {
			ChangeScaleBasedOnPlayerDistance ();
		}

        // Keep self upright
        transform.rotation = Quaternion.identity;
	}


	void ChangeScaleBasedOnPlayerDistance()
	{
		// Get distance from player.
		float distance = Vector3.Distance(transform.position, GameManager.instance.player.transform.position);

		// Change width based on distance from player.
		if (GetComponentInParent<Enemy>() != null && (distance < minimumVisibleDistance && GetComponentInParent<Enemy> ().canSeePlayer)) {
			transform.localScale = new Vector3 (0f, transform.localScale.y, 0f);
		}

		else
		{
			float newWidth = MyMath.Map(distance, minimumVisibleDistance, maxDistance, 0f, 0.5f);
			transform.localScale = new Vector3(newWidth, transform.localScale.y, newWidth);
		}
	}
}
