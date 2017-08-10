using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revolver : MonoBehaviour {

    [SerializeField] float rotationSpeedY;
    [SerializeField] float wobbleSpeed = 10f;
    [SerializeField] float maxZWobble = 10f;
    float sineOffset;
    float sineTime;


    private void Start()
    {
        sineOffset = Random.Range(-1000f, 1000f);
    }


    private void Update()
    {
        sineTime += wobbleSpeed * Time.deltaTime;
        float zWobble = MyMath.Map(Mathf.Sin(sineTime + sineOffset), -1f, 1f, 10f, maxZWobble);
        transform.rotation = Quaternion.Euler(new Vector3(
            zWobble,
            transform.localRotation.eulerAngles.y,
            transform.localRotation.eulerAngles.z + rotationSpeedY * Time.deltaTime
            ));
    }
}
