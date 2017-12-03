using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletMesh : MonoBehaviour {

    float screenTime = 0.2f;
    float timer = 0f;


    public  void SetTransformByEndPoints(Vector3 back, Vector3 front, float thickness) {

        transform.position = Vector3.Lerp(back, front, 0.5f);

        Vector3 lookDirection = (front - back);
        if (lookDirection != Vector3.zero) { transform.rotation = Quaternion.LookRotation(front - back, Vector3.up); }
        transform.Rotate(90f, 0f, 0f);

        transform.localScale = new Vector3(
            thickness,
            Vector3.Distance(back, front) * 0.5f,
            thickness
            );

        timer = 0f;
    }


    public void SetColor(Color newColor) {
        GetComponent<MeshRenderer>().material.SetColor("_Tint", newColor);
    }


    void GoBackToWhereICameFrom() {
        if (!GetComponent<PooledObject>()) { return; }
        GetComponent<PooledObject>().GetReturned();
    }
}
