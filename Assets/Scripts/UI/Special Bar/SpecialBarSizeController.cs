using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpecialBarSizeController : MonoBehaviour {

    [SerializeField] Transform barTransform;
    [SerializeField] FloatRange sizeRange = new FloatRange(0.1f, 2f);   // How large and small the bar can be.
    [SerializeField] ObjectPooler feedbackLinePooler;

    float _percentageFilled;
    [HideInInspector] public float PercentageFilled {
        get {
            return _percentageFilled;
        }
        set {
            // Spawn feedback line
            if (value >= _percentageFilled) {
                float edge1 = GetBarTopEdge(_percentageFilled);
                float edge2 = GetBarTopEdge(value);
                SpawnFeedbackLine(edge1, edge2);
            }

            // Set the bar's scale based on its percentage.
            float newYScale = GetNewYScale(value);
            barTransform.DOScaleY(newYScale, tweenSpeed);

            _percentageFilled = value;
        }
    }

    [SerializeField] private float originPosition = -2.9f;   // The local position of the special bar's lowest point.
    [SerializeField] private float tweenSpeed = 0.4f;    // How quickly the special bar changes size.

    private void Update() {
        // Set the bar's Y position so that it still starts from its original bottom.
        float newXPos = originPosition + barTransform.lossyScale.y * 0.5f;
        barTransform.localPosition = new Vector3(
                newXPos,
                barTransform.localPosition.y,
                barTransform.localPosition.z
            );
    }

    public void SpawnFeedbackLine(float edge1, float edge2) {
        GameObject feedbackLine = feedbackLinePooler.GrabObject();
        if (!Services.gameManager.isGameStarted) { return; }

        feedbackLine.transform.parent = transform.parent;
        feedbackLine.transform.localRotation = Quaternion.Euler(90f, 90f, 90f);

        float height = MyMath.Map(PercentageFilled, 0f, 1f, 10f, 35f);
        height *= Random.Range(0.75f, 1.25f);

        StartCoroutine(feedbackLine.GetComponent<FeedbackLine>().Sequence(edge1, edge2, height, -barTransform.right));
    }

    float GetBarTopEdge(float percentage) {        
        return (new Vector3(originPosition, transform.localPosition.y, transform.localPosition.z) + (-barTransform.right * GetNewYScale(percentage))).x;
    }

    float GetNewYScale(float percentage) {
        float newYScale = MyMath.Map(percentage, 0f, 1f, sizeRange.min, sizeRange.max);
        newYScale = Mathf.Clamp(newYScale, sizeRange.min, sizeRange.max);
        return newYScale;
    }
}
