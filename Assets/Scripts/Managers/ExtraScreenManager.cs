using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ExtraScreenManager : MonoBehaviour {

    [SerializeField] GameObject[] extraScreens;

    public void SetScreensActive(bool value) {
        for (int i = 0; i < extraScreens.Length; i++) {
            extraScreens[i].SetActive(value);
        }
    }

    public void SetRotationScale(float newScale) {
        for (int i = 0; i < extraScreens.Length; i++) {
            extraScreens[i].GetComponent<RotateRandomly>().rotationScale = newScale;
        }
    }

    Coroutine returnToZeroAndDeactivate;
    public void ReturnToZeroAndDeactivate(float duration) {
        if (returnToZeroAndDeactivate != null) { StopCoroutine(returnToZeroAndDeactivate); }
        returnToZeroAndDeactivate = StartCoroutine(ReturnToZeroAndDeactivateCoroutine(duration));
    }

    private IEnumerator ReturnToZeroAndDeactivateCoroutine(float duration) {
        for (int i = 0; i < extraScreens.Length; i++) {
            extraScreens[i].GetComponent<RotateRandomly>().rotationScale = 0f;
            extraScreens[i].transform.DORotate(Vector3.zero, duration);
        }
        yield return new WaitForSeconds(duration);

        SetScreensActive(false);

        yield return null;
    }
}
