using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScaler : MonoBehaviour {


    public void ScaleLevel(float newArea) {

        Bounds currentBounds = GetComponent<EnemyPlacer>().GetLevelFloorBounds();
        float currentArea = currentBounds.size.x * currentBounds.size.z;
        Debug.Log("Currenta area: " + currentArea);
        float scaleMultiplier = newArea*1000 / currentArea;
        Debug.Log("Scale multiplier: " + scaleMultiplier);

        // Do floor
        Transform floorHolder = GameObject.Find("Floor Planes").transform;
        for (int i = 0; i < floorHolder.childCount; i++) {
            MoveObject(floorHolder.GetChild(i), scaleMultiplier);
            floorHolder.GetChild(i).localScale *= scaleMultiplier;
        }

        // Do walls
        Transform wallHolder = GameObject.Find("Walls").transform;
        for (int i = 0; i < wallHolder.childCount; i++) {
            MoveObject(wallHolder.GetChild(i), scaleMultiplier);
            ScaleObjectX(wallHolder.GetChild(i), scaleMultiplier);
        }

        // Do railings
        if (GameObject.Find("Railings") != null) {
            Transform railingHolder = GameObject.Find("Railings").transform;
            for (int i = 0; i < railingHolder.childCount; i++) {
                MoveObject(railingHolder.GetChild(i), scaleMultiplier);
                ScaleObjectX(railingHolder.GetChild(i), scaleMultiplier);
            }
        }

        // Do obstacles
        if (GameObject.Find("Obstacle") != null) {
            Transform obstacleHolder = GameObject.Find("Obstacle").transform;
            for (int i = 0; i < obstacleHolder.childCount; i++) {
                MoveObject(obstacleHolder.GetChild(i), scaleMultiplier);
                ScaleObjectXZ(obstacleHolder.GetChild(i), scaleMultiplier);
            }
        }
    }


    private void ScaleObjectX(Transform t, float scaleMultiplier) {
        Vector3 newLocalScale = t.localScale;
        newLocalScale.x *= scaleMultiplier;
        t.localScale = newLocalScale;
    }


    private void ScaleObjectXZ(Transform t, float scaleMultiplier) {
        Vector3 newLocalScale = t.localScale;
        newLocalScale.x *= scaleMultiplier;
        newLocalScale.z *= scaleMultiplier;
        t.localScale = newLocalScale;
    }


    private void MoveObject(Transform t, float scaleMultiplier) {
        Vector3 origin = t.parent.position;
        origin.y = t.localPosition.y;
        Vector3 moveDirection = Vector3.Normalize(t.position - origin);
        float distanceFromOrigin = Vector3.Distance(t.position, origin);
        t.position = origin + distanceFromOrigin * moveDirection * scaleMultiplier;
    }
}
