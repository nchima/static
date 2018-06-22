using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeDashLine : MonoBehaviour {

    [SerializeField] Transform endPoint;

    private Transform originalParent;
    private Quaternion originalLocalRotation;
    private Vector3 originalLocalPosition;

    private const float DEFAULT_DISTANCE = 60f;
    [HideInInspector] public float distance;
    private FloatRange minMaxDistance = new FloatRange(5f, 200f);

	private LineRenderer m_LineRenderer { get { return GetComponent<LineRenderer>(); } }

    private void Awake() {
        SetActive(false);
        originalParent = transform.parent;
        originalLocalRotation = transform.localRotation;
        originalLocalPosition = transform.localPosition;
    }

    private void Update() {
        float inputMod = 100f;
        if (InputManager.inputMode == InputManager.InputMode.Controller) { inputMod = -2.9f; }
        inputMod *= (1 / Time.timeScale);
        distance += InputManager.gunTuningValue * inputMod * Time.deltaTime;
        distance = minMaxDistance.Clamp(distance);

        m_LineRenderer.SetPosition(1, new Vector3(0f, 0f, distance));
        endPoint.localPosition = m_LineRenderer.GetPosition(1);
        CircleDrawer.Draw(endPoint.GetComponent<LineRenderer>(), 10f, 10f, 9, 1f);
    }


    public void AttachToParent(bool value) {
        if (value == true) {
            transform.parent = originalParent;
            transform.localRotation = originalLocalRotation;
            transform.localPosition = originalLocalPosition;
        }

        else if (value == false) {
            transform.parent = null;
        }
    }


    public void SetActive(bool value) {
        m_LineRenderer.enabled = value;
        endPoint.gameObject.SetActive(value);
        if (value == true) { distance = DEFAULT_DISTANCE; }
    }
}
