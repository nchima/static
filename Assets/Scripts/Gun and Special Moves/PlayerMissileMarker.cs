using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMissileMarker : MonoBehaviour {

	void OnEnable() {
		GameEventManager.instance.Subscribe<GameEvents.FallingSequenceFinished>(FallingSequenceFinishedHandler);
	}

	void OnDisable() {
		GameEventManager.instance.Unsubscribe<GameEvents.FallingSequenceFinished>(FallingSequenceFinishedHandler);
	}

	void Start() {
		transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
	}

	void FallingSequenceFinishedHandler(GameEvent gameEvent) {
		Destroy(gameObject);
	}
}
