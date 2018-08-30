using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpecialBarManager : MonoBehaviour {

    [SerializeField] bool debug_AlwaysMax;

    [SerializeField] SpecialBar leftBar;
    [SerializeField] SpecialBar rightBar;

    [SerializeField] GameObject[] savedShotBoxes;
    [SerializeField] SpecialMoveEarnedPrompt earnedPrompt;

    // GAMEPLAY STUFF
    public float startValue = 0.4f;    // How large of a special the player starts the game with.

    public float stickToFullDuration = 2f;   // How long the bars stick to their maximum value.

    // Relating to the decay of the special bar.
    public float decayRate;     // How quickly the special bar currently shrinks.

    [SerializeField] private float enemyValue = 0.4f; // How much the player's special increases for killing an enemy.
    [SerializeField] private float bulletHitValue = 0.01f;    // How much the player's special increases for hitting an enemy with one bullet.
    [SerializeField] private float getHurtPenalty = 0.1f; // How much the special bar decreases when the player is hurt.

    [HideInInspector] public bool screenHidden = false;

    public bool BothFirstBarsFull {
        get {
            if (debug_AlwaysMax) { return true; }
            return leftBar.CurrentValue >= 0.99f && rightBar.CurrentValue >= 0.99f;
        }
    }
    public bool BothSecondBarsFull {
        get {
            if (debug_AlwaysMax) { return true; }
            return leftBar.CurrentValue >= 1.99f && rightBar.CurrentValue >= 1.99f;
        }
    }

    [SerializeField] int maxSavedShots = 2;
    private int shotsSaved;
    public int ShotsSaved {
        get {
            return shotsSaved;
        }

        set {
            shotsSaved = Mathf.Clamp(value, 0, maxSavedShots+1);
            //if (shotsSaved == 1) {
            //    savedShotBoxes[0].SetActive(true);
            //    earnedPrompt.gameObject.SetActive(true);
            //    earnedPrompt.Activate();
            //}
            //else { savedShotBoxes[0].SetActive(false); }
        }
    }

    public void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }

    private void Start() {
        leftBar.Initialize(this);
        rightBar.Initialize(this);
    }

    private void Update() {
        leftBar.Run(this);
        rightBar.Run(this);

        if (!screenHidden && Services.playerController.state != PlayerController.State.Dead) {
            if (BothFirstBarsFull && ShotsSaved < 1) {
                AddAmmoCharge();
            }

            if (BothSecondBarsFull && ShotsSaved < 2) {
                AddAmmoCharge();
            }

            if (!BothSecondBarsFull && ShotsSaved > 1) {
                RemoveAmmoCharge();
            }

            if (!BothFirstBarsFull && ShotsSaved > 0) {
                RemoveAmmoCharge();
            }

            //if (BothFirstBarsFull) {
            //    if (ShotsSaved < maxSavedShots) {
            //        ShotsSaved++;
            //        leftBar.CurrentValue = 0f;
            //        rightBar.CurrentValue = 0f;
            //    } else {
            //        earnedPrompt.gameObject.SetActive(true);
            //        earnedPrompt.Activate();
            //        savedShotBoxes[1].SetActive(true);
            //    }
            //}
        }
    }

    void AddAmmoCharge() {
        ShotsSaved++;
        earnedPrompt.gameObject.SetActive(true);
        earnedPrompt.Activate();
        savedShotBoxes[ShotsSaved - 1].SetActive(true);
    }

    void RemoveAmmoCharge() {
        Debug.Log("removing ammo");
        ShotsSaved--;
        savedShotBoxes[ShotsSaved].SetActive(false);
        //if (ShotsSaved == maxSavedShots && bothFirstBarsFull) {
        //    savedShotBoxes[1].SetActive(false);
        //} else {
        //    ShotsSaved--;
        //}
    }

    public void PlayerAbsorbedAmmo(float value) {
        leftBar.CurrentValue += value;
        rightBar.CurrentValue += value;
    }

    public void FlashBar() {
        //barObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(readyColor1, readyColor2, Random.Range(0f, 1f));
    }

    public void AddValue(float value) {
        // Split value based on current gun value.
        float mappedGunValue = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0f, 1f);
        float leftValue = value * (1 - mappedGunValue);
        float rightValue = value * mappedGunValue;

        leftBar.CurrentValue += leftValue;
        rightBar.CurrentValue += rightValue;
    }

    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        if (Services.healthManager.isInvincible) { return; }

        //leftBar.CurrentValue -= getHurtPenalty;
        //rightBar.CurrentValue -= getHurtPenalty;
    }

    public void PlayerUsedSpecialMove() {
        leftBar.CurrentValue -= 1f;
        rightBar.CurrentValue -= 1f;
    }

    public void PlayerKilledEnemyHandler(GameEvent gameEvent) {
        GameEvents.PlayerKilledEnemy playerKilledEnemyEvent = gameEvent as GameEvents.PlayerKilledEnemy;
        AddValue(playerKilledEnemyEvent.specialValue);
    }
}
