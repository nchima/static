using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShotgunChargeSphere : MonoBehaviour {

    float inactivePosition;
    [SerializeField] float activePosition = 0.25f;

    FloatRange materialTilingRangeX = new FloatRange(11f, 17f);
    FloatRange materialTilingRangeY = new FloatRange(-18.7f, 18.7f);


    private void Start() {
        inactivePosition = transform.position.z;
    }


    private void Update() {
        // Rotate visual sphere
        transform.rotation = Random.rotation;

        // Do texture stuff on the visual sphere.
        foreach (MeshRenderer mr in GetComponentsInChildren<MeshRenderer>()) {
            mr.material.mainTextureOffset = new Vector2(0f, Random.Range(0f, 100f));
            mr.material.mainTextureScale = new Vector2(materialTilingRangeX.Random, materialTilingRangeY.Random);
            Color newColor = mr.material.color;
            newColor.r = Random.Range(0.8f, 1f);
            newColor.g = Random.Range(0.8f, 1f);
            mr.material.color = newColor;
        }
    }


    public void MoveIntoChargePosition() {
        // Begin moving visual sphere into position.
        transform.DOLocalMoveZ(activePosition, 0.1f);
    }


    public void EndCharge() {
        transform.DOLocalMoveZ(inactivePosition, 0.1f);
    }
}
