using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeDashLine : MonoBehaviour {

    private const float DEFAULT_DISTANCE = 20f;
    [HideInInspector] public float distance;
    private FloatRange minMaxDistance = new FloatRange(5f, 200f);

	private LineRenderer m_LineRenderer { get { return GetComponent<LineRenderer>(); } }

    private void Awake() {
        SetActive(false);
    }

    private void Update() {
        float inputMod = 100f;
        if (InputManager.inputMode == InputManager.InputMode.Controller) { inputMod = -2.9f; }
        inputMod *= (1 / Time.timeScale);
        distance += InputManager.gunTuningValue * inputMod * Time.deltaTime;
        distance = minMaxDistance.Clamp(distance);

        m_LineRenderer.SetPosition(1, new Vector3(0f, 0f, distance));
    }


    public void SetActive(bool value) {
        m_LineRenderer.enabled = value;
        if (value == true) { distance = DEFAULT_DISTANCE; }
    }
}
