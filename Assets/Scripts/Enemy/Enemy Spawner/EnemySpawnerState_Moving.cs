using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerState_Moving : State {

	private Vector3 nextSpawnPosition = Vector3.zero;

	public override void Initialize(StateController stateController) {
		base.Initialize(stateController);

		EnemySpawner controller = stateController as EnemySpawner;

		// Choose a position at which to spawn the enemy
		nextSpawnPosition = transform.position;
		float testDistance = 100f;
		for (int i = 0; i < 100; i++) {
			// Get a position to test.
			Vector3 positionToTest = Services.playerTransform.position;
			Vector3 testDirection = Quaternion.Euler(0f, Random.Range(-180f, 180f), 0f) * Vector3.right;
			positionToTest += testDirection * testDistance;

			// Test the position.
			if (IsPositionAboveFloor(positionToTest, 5f)) { 
				nextSpawnPosition = positionToTest;
				nextSpawnPosition.y = controller.hoverHeight;
				break;
			}

			// If the position was not above the floor, decrease the test distance and try again.
			testDistance -= 1;
		}

		if (nextSpawnPosition == Vector3.zero) { Debug.Log("ops, could not find the right spot."); }
	}

	public override void Run(StateController stateController) {
		base.Run(stateController);

		EnemySpawner controller = stateController as EnemySpawner;

		// Move towards the spawn position chosen in Initialize()
		Vector3 moveDirection = Vector3.Normalize(controller.transform.position - nextSpawnPosition);
		controller.m_Rigidbody.MovePosition(controller.transform.position + moveDirection * controller.maxSpeed * Time.deltaTime);
	}

	private bool IsPositionAboveFloor(Vector3 position, float radius) {
		bool returnValue = false;

		// See if the player is over a floor tile.
		RaycastHit hit1;
		RaycastHit hit2;

		float colliderRadius = GetComponent<CapsuleCollider>().radius;

		// If we didn't find anything, return false.
		if (!Physics.Raycast(position + Vector3.forward * radius, Vector3.down, out hit1, 20f, (1 << 20 | 1 << 24))) { return false; }
		if (!Physics.Raycast(position + Vector3.forward * -radius, Vector3.down, out hit2, 20f, (1 << 20 | 1 << 24))) { return false; }
		if (!Physics.Raycast(position + Vector3.right * radius, Vector3.down, out hit1, 20f, (1 << 20 | 1 << 24))) { return false; }
		if (!Physics.Raycast(position + Vector3.right * -radius, Vector3.down, out hit2, 20f, (1 << 20 | 1 << 24))) { return false; }

		// If both things hit something and it was the floor, we're all good baby!
		if (hit1.transform.name.ToLower().Contains("floor") && hit2.transform.name.ToLower().Contains("floor")) {
			returnValue = true;
		}

		// If it wasn't the floor, return false.
		else {
			return false;
		}

		return returnValue;
	}
}
