using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashManager : MonoBehaviour {

    [SerializeField] float gunFlashDuration = 0.1f;
    [SerializeField] FloatRange gunFlashIntensityRange = new FloatRange(0.4f, 1f);
    [SerializeField] float painFlashDuration;

    [SerializeField] GameObject gunFlashPlane;
    [SerializeField] GameObject painFlashPlane;

    bool isFlashing;
    Coroutine flashCoroutine;


    private void Awake() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerFiredGun>(PlayerFiredGunHandler);
    }


    public void Flash(Color flashColor, GameObject flashPlane, float flashDuration) {
        if (flashCoroutine != null) { StopCoroutine(flashCoroutine); }
        flashCoroutine = StartCoroutine(FlashCoroutine(flashColor, flashPlane, flashDuration));
    }


    IEnumerator FlashCoroutine(Color flashColor, GameObject flashPlane, float flashDuration) {
        flashPlane.GetComponent<MeshRenderer>().material.color = flashColor;
        flashPlane.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(flashColor.a, flashColor.a, flashColor.a));
        yield return new WaitForSeconds(flashDuration);
        flashPlane.GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f, 0f);
        flashPlane.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
        yield return null;
    }


    public void PlayerFiredGunHandler(GameEvent gameEvent) {
        float currentFlash = MyMath.Map(GunValueManager.currentValue, -1f, 1f, gunFlashIntensityRange.max, gunFlashIntensityRange.min);
        Flash(new Color(currentFlash, currentFlash, currentFlash, currentFlash), gunFlashPlane, gunFlashDuration);
    }


    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        painFlashPlane.GetComponent<Animator>().SetTrigger("Pain Flash");
    }
}
