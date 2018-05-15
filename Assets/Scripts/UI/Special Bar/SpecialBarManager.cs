using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpecialBarManager : MonoBehaviour {

    [SerializeField] SpecialBar leftBar;
    [SerializeField] SpecialBar rightBar;

    // GAMEPLAY STUFF
    public float startValue = 0.4f;    // How large of a special the player starts the game with.

    public float stickToFullDuration = 2f;   // How long the bars stick to their maximum value.

    // Relating to the decay of the special bar.
    public float decayRate;     // How quickly the special bar currently shrinks.

    [SerializeField] private float enemyValue = 0.4f; // How much the player's special increases for killing an enemy.
    [SerializeField] private float bulletHitValue = 0.01f;    // How much the player's special increases for hitting an enemy with one bullet.
    [SerializeField] private float getHurtPenalty = 0.1f; // How much the special bar decreases when the player is hurt.

    [SerializeField] private GameObject specialMoveReadyScreen;
    [HideInInspector] public bool screenHidden = false;

    public bool bothBarsFull {
        get {
            return leftBar.currentValue >= 0.99f && rightBar.currentValue >= 0.99f;
        }
    }


    public void Awake() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }


    private void Start() {
        leftBar.Initialize(this);
        rightBar.Initialize(this);
    }


    private void Update() {
        leftBar.Run(this);
        rightBar.Run(this);

        if (bothBarsFull && !screenHidden) {
            if (Services.playerController.state != PlayerController.State.Dead) { specialMoveReadyScreen.SetActive(true); }
            FlashBar();
        }

        else {
            specialMoveReadyScreen.SetActive(false);
        }
    }


    public void PlayerAbsorbedAmmo(float value) {
        leftBar.currentValue += value;
        rightBar.currentValue += value;
    }


    public void FlashBar() {
        //barObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(readyColor1, readyColor2, Random.Range(0f, 1f));
    }


    public void AddValue(float value) {
        // Split value based on current gun value.
        float mappedGunValue = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0f, 1f);
        float leftValue = value * (1 - mappedGunValue);
        float rightValue = value * mappedGunValue;

        leftBar.currentValue += leftValue;
        rightBar.currentValue += rightValue;
    }


    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        leftBar.currentValue -= getHurtPenalty;
        rightBar.currentValue -= getHurtPenalty;
    }


    public void PlayerUsedSpecialMove() {
        leftBar.currentValue = 0f;
        rightBar.currentValue = 0f;
    }


    public void PlayerKilledEnemyHandler(GameEvent gameEvent) {
        GameEvents.PlayerKilledEnemy playerKilledEnemyEvent = gameEvent as GameEvents.PlayerKilledEnemy;

        AddValue(playerKilledEnemyEvent.specialValue);
    }
}
