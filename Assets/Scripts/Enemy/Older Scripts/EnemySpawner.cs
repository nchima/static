using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : EnemyOld {

	[SerializeField] GameObject enemyToSpawn;
    [SerializeField] float timeBetweenSpawns;
    float spawnTimer;


    protected override void Update()
    {
        base.Update();

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= timeBetweenSpawns)
        {
            SpawnEnemy();
            spawnTimer = 0;
        }
    }


    void SpawnEnemy()
    {
        Vector3 newPosition = transform.position + new Vector3(Random.insideUnitCircle.x * 20f, 20f, Random.insideUnitCircle.y * 20f);

        GameObject newEnemy = Instantiate(enemyToSpawn);
        newEnemy.transform.position = newPosition;

        gameManager.currentEnemyAmt++;
    }
}
