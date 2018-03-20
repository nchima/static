using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashManager : MonoBehaviour {

    [SerializeField] float flashTime = 0.1f;
    [SerializeField] GameObject backgroundFlash;
    bool isFlashing;

    Coroutine flashCoroutine;

    public void Flash(Color flashColor) {
        if (flashCoroutine != null) { StopCoroutine(flashCoroutine); }
        flashCoroutine = StartCoroutine(FlashCoroutine(flashColor));
    }

    IEnumerator FlashCoroutine(Color flashColor) {
        backgroundFlash.GetComponent<MeshRenderer>().material.color = flashColor;
        backgroundFlash.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(flashColor.a, flashColor.a, flashColor.a));
        yield return new WaitForSeconds(flashTime);
        backgroundFlash.GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f, 0f);
        backgroundFlash.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
        yield return null;
    }
}
