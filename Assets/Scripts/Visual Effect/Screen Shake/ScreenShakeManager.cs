﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeManager : MonoBehaviour {

    [SerializeField] float playerHurtShakeAmount = 2f;

    ScreenShake[] screenShakeScripts;

    private void Awake() {
        screenShakeScripts = FindObjectsOfType<ScreenShake>();
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }

    public void IncreaseShake(float value) {
        foreach(ScreenShake screenShake in screenShakeScripts) {
            screenShake.IncreaseShake(value);
        }
    }

    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        IncreaseShake(playerHurtShakeAmount);
    }
}