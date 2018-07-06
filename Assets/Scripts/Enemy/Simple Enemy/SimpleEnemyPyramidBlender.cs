using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemyPyramidBlender : MonoBehaviour {

    [SerializeField] float blendSpeed = 1f;
    float sineValue;
    SkinnedMeshRenderer skinnnedMeshRenderer { get { return GetComponent<SkinnedMeshRenderer>(); } }


    private void Start() {
        sineValue = Random.Range(-1000f, 1000f);
    }

    private void Update() {
        sineValue += Time.deltaTime * blendSpeed;
        skinnnedMeshRenderer.SetBlendShapeWeight(0, MyMath.Map(Mathf.Sin(sineValue), -1f, 1f, 0f, 100f));
    }
}
