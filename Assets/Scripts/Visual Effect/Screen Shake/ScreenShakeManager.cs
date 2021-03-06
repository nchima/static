using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeManager : MonoBehaviour {

    [SerializeField] float playerHurtShakeAmount = 2f;

    ScreenShake[] screenShakeScripts;

    private void Awake() {
        screenShakeScripts = FindObjectsOfType<ScreenShake>();
    }

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }

    public void IncreaseShake(float value) {
        foreach(ScreenShake screenShake in screenShakeScripts) {
            screenShake.IncreaseShake(value);
        }
    }

    public void SetShake(float value, float time) {
        foreach(ScreenShake screenShake in screenShakeScripts) {
            screenShake.SetShake(value, time);
        }
    }

    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        if (Services.healthManager.isInvincible) { return; }
        IncreaseShake(playerHurtShakeAmount);
    }
}
