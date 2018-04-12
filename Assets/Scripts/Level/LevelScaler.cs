using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScaler : MonoBehaviour {

    [SerializeField] float scaleMultiplier = 1f;


    private void Start() {
        // Do floor
        Transform floorHolder = GameObject.Find("Floor Planes").transform;
        for (int i = 0; i < floorHolder.childCount; i++) {
            MoveObject(floorHolder.GetChild(i));
            floorHolder.GetChild(i).localScale *= scaleMultiplier;
        }

        // Do walls
        Transform wallHolder = GameObject.Find("Walls").transform;
        for (int i = 0; i < wallHolder.childCount; i++) {
            MoveObject(wallHolder.GetChild(i));
            ScaleObjectX(wallHolder.GetChild(i));
        }

        // Do railings
        if (GameObject.Find("Railings") != null) {
            Transform railingHolder = GameObject.Find("Railings").transform;
            for (int i = 0; i < railingHolder.childCount; i++) {
                MoveObject(railingHolder.GetChild(i));
                ScaleObjectX(railingHolder.GetChild(i));
            }
        }

        // Do obstacles
        if (GameObject.Find("Obstacle") != null) {
            Transform obstacleHolder = GameObject.Find("Obstacle").transform;
            for (int i = 0; i < obstacleHolder.childCount; i++) {
                MoveObject(obstacleHolder.GetChild(i));
                ScaleObjectXZ(obstacleHolder.GetChild(i));
            }
        }
    }


    private void ScaleObjectX(Transform t) {
        Vector3 newLocalScale = t.localScale;
        newLocalScale.x *= scaleMultiplier;
        t.localScale = newLocalScale;
    }


    private void ScaleObjectXZ(Transform t) {
        Vector3 newLocalScale = t.localScale;
        newLocalScale.x *= scaleMultiplier;
        newLocalScale.z *= scaleMultiplier;
        t.localScale = newLocalScale;
    }


    private void MoveObject(Transform t) {
        Vector3 origin = t.parent.position;
        origin.y = t.localPosition.y;
        Vector3 moveDirection = Vector3.Normalize(t.position - origin);
        float distanceFromOrigin = Vector3.Distance(t.position, origin);
        t.position = origin + distanceFromOrigin * moveDirection * scaleMultiplier;
    }
}
