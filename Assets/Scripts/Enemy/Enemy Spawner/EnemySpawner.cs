using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : Enemy {

	[SerializeField] public GameObject enemyToSpawn;
	[SerializeField] public int amountToSpawn;
	[SerializeField] public float hoverHeight;
	[SerializeField] public float maxSpeed;
}
