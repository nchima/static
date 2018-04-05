using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerBulletMesh : MonoBehaviour {

    public float screenTime = 0.2f;
    float blendValue = 0f;


    private void Update() {

        // Tween blend shape weight
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();

        smr.SetBlendShapeWeight(0, blendValue);
    }


    public  void SetTransformByEndPoints(Vector3 back, Vector3 front, float thickness) {

        transform.position = Vector3.Lerp(back, front, 0.5f);

        Vector3 lookDirection = (front - back);
        if (lookDirection != Vector3.zero) { transform.rotation = Quaternion.LookRotation(front - back, Vector3.up); }

        // Give mesh a random z rotation
        Vector3 newRotation = transform.localRotation.eulerAngles;
        newRotation.z = Random.Range(-180f, 180f);
        transform.rotation = Quaternion.Euler(newRotation);

        // Give mesh a random blend shape value
        GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, Random.Range(0f, 25f));

        transform.localScale = new Vector3(
            thickness,
            thickness,
            Vector3.Distance(back, front) * 0.5f
            );

        blendValue = 0f;
        DOTween.To(() => blendValue, x => blendValue = x, 100f, screenTime).SetEase(Ease.InQuart);
    }


    public void SetColor(Color newColor) {
        GetComponent<SkinnedMeshRenderer>().material.SetColor("_Tint", newColor);
    }


    void GoBackToWhereICameFrom() {
        if (!GetComponent<PooledObject>()) { return; }
        GetComponent<PooledObject>().GetReturned();
    }
}
