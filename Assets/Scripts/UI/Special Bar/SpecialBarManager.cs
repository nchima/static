using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpecialBarManager : MonoBehaviour {

    [SerializeField] SpecialBar leftBar;
    [SerializeField] SpecialBar rightBar;

    // GAMEPLAY STUFF
    public float startValue = 0.4f;    // How large of a special the player starts the game with.
    public float baseDecay = 0.01f;  // How quickly the special bar shrinks (increases as the player's multiplier increases)

    public float stickToFullDuration = 2f;   // How long the bars stick to their maximum value.

    // Relating to the decay of the special bar.
    [HideInInspector] public float decayRate;     // How quickly the special bar currently shrinks.

    [SerializeField] private float enemyValue = 0.4f; // How much the player's special increases for killing an enemy.
    [SerializeField] private float bulletHitValue = 0.01f;    // How much the player's special increases for hitting an enemy with one bullet.
    [SerializeField] private float getHurtPenalty = 0.1f; // How much the special bar decreases when the player is hurt.

    [SerializeField] private GameObject specialMoveReadyScreen;


    private void Start() {
        leftBar.Initialize(this);
        rightBar.Initialize(this);
    }


    private void Update() {
        leftBar.Run(this);
        rightBar.Run(this);
    }


    public void PlayerAbsorbedAmmo(float value) {
        leftBar.currentValue += value;
        rightBar.currentValue += value;
    }


    public void FlashBar() {
        //barObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(readyColor1, readyColor2, Random.Range(0f, 1f));
    }


    public void PlayerKilledEnemy() {
        leftBar.currentValue += enemyValue;
        rightBar.currentValue += enemyValue;
    }


    public void BulletHitEnemy() {
        leftBar.currentValue += bulletHitValue;
        rightBar.currentValue += bulletHitValue;
    }


    public void PlayerWasHurt() {
        leftBar.currentValue -= getHurtPenalty;
        rightBar.currentValue -= getHurtPenalty;
    }


    public void PlayerUsedSpecialMove() {
        leftBar.currentValue = 0f;
        rightBar.currentValue = 0f;
    }
}
