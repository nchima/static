using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashManager : MonoBehaviour {

    [SerializeField] FloatRange gunFlashDurationRange = new FloatRange(0.05f, 0.1f);
    [SerializeField] FloatRange gunFlashIntensityRange = new FloatRange(0.4f, 1f);
    [SerializeField] float painFlashDuration;

    Color originalGunColor;

    [SerializeField] GameObject gunFlashPlane;
    [SerializeField] GameObject painFlashPlane;

    bool isFlashing;
    Coroutine flashCoroutine;


    private void Awake() {
        originalGunColor = gunFlashPlane.GetComponent<MeshRenderer>().material.color;
    }

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerFiredGun>(PlayerFiredGunHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerFiredGun>(PlayerFiredGunHandler);
    }


    public void Flash(Color flashColor, GameObject flashPlane, float flashDuration) {
        if (flashCoroutine != null) { StopCoroutine(flashCoroutine); }
        flashCoroutine = StartCoroutine(FlashCoroutine(flashColor, flashPlane, flashDuration));
    }


    IEnumerator FlashCoroutine(Color flashColor, GameObject flashPlane, float flashDuration) {
        flashPlane.GetComponent<MeshRenderer>().material.color = flashColor;
        flashPlane.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", flashColor);
        yield return new WaitForSeconds(flashDuration);
        flashPlane.GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f, 0f);
        flashPlane.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
        yield return null;
    }


    public void PlayerFiredGunHandler(GameEvent gameEvent) {
        float currentFlash = MyMath.Map(GunValueManager.currentValue, -1f, 1f, gunFlashIntensityRange.min, gunFlashIntensityRange.max);
        float currentDuration = MyMath.Map(GunValueManager.currentValue, -1f, 1f, gunFlashDurationRange.min, gunFlashDurationRange.max);

        gunFlashPlane.GetComponent<MeshRenderer>().material.mainTextureOffset = new Vector2(Random.Range(0f, 5f), Random.Range(0f, 5f));

        Vector2 newScale = new Vector2(Random.Range(1f, 1.5f), Random.Range(1f, 1.5f));
        gunFlashPlane.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(Random.Range(1f, 1.5f) * MyMath.Either1orNegative1, Random.Range(1f, 1.5f) * MyMath.Either1orNegative1);

        Color flashColor = originalGunColor;
        flashColor.a = currentFlash;
        
        Flash(flashColor, gunFlashPlane, currentDuration);
    }


    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        if (Services.healthManager.isInvincible) { return; }
        painFlashPlane.GetComponent<Animator>().SetTrigger("Pain Flash");
    }
}
