using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlacer : MonoBehaviour {

    [SerializeField] GameObject enemyPrefab;


    public void PlaceEnemies() {
        Bounds floorBounds = GetLevelFloorBounds();

        for (int i = 0; i < 10; i++) {
            PlaceEnemy(enemyPrefab, floorBounds);
        }
    }


    Bounds GetLevelFloorBounds() {
        // Combine bounds of all floor planes
        Transform floorPlaneHolder = GameObject.Find("Floor Planes").transform;
        Bounds floorBounds = new Bounds();
        for (int i = 0; i < floorPlaneHolder.childCount; i++) {
            floorBounds.Encapsulate(floorPlaneHolder.GetChild(i).GetComponent<Renderer>().bounds);
        }
        return floorBounds;
    }


    void PlaceEnemy(GameObject enemyPrefab, Bounds floorBounds) {
        Vector3 testPosition = floorBounds.center;
        testPosition.x += Random.Range(-floorBounds.extents.x, floorBounds.extents.x);
        testPosition.y = 10f;
        testPosition.z += Random.Range(-floorBounds.extents.z, floorBounds.extents.z);

        Instantiate(enemyPrefab, testPosition, Quaternion.identity);
    }
}
