using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {

    Vector3 rotateDirection;
    FloatRange rotateSpeedRange = new FloatRange(5f, 10f);
    float rotateSpeed;
    public float speedScale = 1f;
    [SerializeField] Vector3 directionModifier = Vector3.one;

    float noiseTime = 0f;
    float noiseSpeed = 0.1f;
    float noiseOffset, noiseOffset2, noiseOffset3, noiseOffset4;


    private void Start()
    {
        noiseOffset = Random.Range(-1000f, 1000f);
        noiseOffset2 = Random.Range(-1000f, 1000f);
        noiseOffset3 = Random.Range(-1000f, 1000f);
        noiseOffset4 = Random.Range(-1000f, 1000f);
    }


    private void Update()
    {
        noiseTime += noiseSpeed * Time.deltaTime;
        rotateSpeed = MyMath.Map(Mathf.PerlinNoise(noiseTime + noiseOffset, 0f), 0f, 1f, rotateSpeedRange.min, rotateSpeedRange.max) * speedScale;
        rotateDirection = new Vector3(
            rotateDirection.x + MyMath.Map(Mathf.PerlinNoise(noiseTime + noiseOffset2, 0f), 0f, 1f, -1f, 1f) * Time.deltaTime * directionModifier.x,
            rotateDirection.y + MyMath.Map(Mathf.PerlinNoise(noiseTime + noiseOffset3, 0f), 0f, 1f, -1f, 1f) * Time.deltaTime * directionModifier.y,
            rotateDirection.z + MyMath.Map(Mathf.PerlinNoise(noiseTime + noiseOffset4, 0f), 0f, 1f, -1f, 1f) * Time.deltaTime * directionModifier.z
            ).normalized;

        transform.Rotate(rotateDirection, rotateSpeed, Space.Self);
    }
}
