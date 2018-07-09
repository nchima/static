using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour {

    /* INSPECTOR */
    [SerializeField] float healthRechargeRate; // How quickly each 'block' of health recharges.
    [SerializeField] float invincibilityTime = 1f;  // How long the player is invincible after being hit.
    [SerializeField] bool godMode;  // Used for debugging only.
    [SerializeField] int firstHealthBonusScore = 10000;  // The score at which the player earns their first health bonus.
    [SerializeField] int subsequentHealthBonusesScore = 15000;   // After the first health bonus, the player earns health every ____ points.

    // References
    [SerializeField] HealthBox[] healthBlocks;
    [SerializeField] AudioSource getHurtAudio;

    /* MISC */
    // Player's health.
    [HideInInspector] public int currentHealth = 5;
    int currentMaxHealth = 5;
    float healthRechargeTimer = 0f;

    bool isInWarningState = false;

    // Health bonuses
    bool isFirstHealthBonusApplied;
    int subsequentHealthBonusesApplied = 0;

    // Invincibility frames.
    [HideInInspector] public bool forceInvincibility;    // Used by other scripts to force player invincibility at certain times.
    [HideInInspector] public bool isInvincible;
    float invincibilityTimer = 0f;

    public bool PlayerIsDead { get { return currentHealth <= 0 || currentMaxHealth <= 0; } }


    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }


    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }


    private void Update() {
        // Handle health recharging.
        if (currentHealth < currentMaxHealth) {
            healthRechargeTimer += Time.deltaTime;
            if (healthRechargeTimer >= healthRechargeRate) {
                // Add one health point
                currentHealth++;
                UpdateHealthBoxes();
                healthRechargeTimer = 0f;

                // If current health has reached max health, exit warning state.
                if (currentHealth == currentMaxHealth) {
                    isInWarningState = false;
                    Services.uiManager.healthWarningScreen.SetActive(false);
                    Services.colorPaletteManager.RestoreSavedPalette();
                }
            }
        }

        // Check for score bonuses.
        if (Services.scoreManager.score >= firstHealthBonusScore + subsequentHealthBonusesApplied * subsequentHealthBonusesScore) {
            AddMaxHealth();
            subsequentHealthBonusesApplied++;
        }

        // Check invincibility frames.
        invincibilityTimer += Time.deltaTime;
        if (invincibilityTimer < invincibilityTime) {
            isInvincible = true;
        } else {
            isInvincible = false;
        }

        if (forceInvincibility || godMode) isInvincible = true;
    }


    void UpdateHealthBoxes() {
        for (int i = 0; i < healthBlocks.Length; i++) {
            if (i >= currentMaxHealth) { healthBlocks[i].BecomeXedOut(); }
            else { healthBlocks[i].BecomeUnXedOut(); }
            if (i >= currentHealth) { healthBlocks[i].BecomeEmpty(); }
            else { healthBlocks[i].BecomeFull(); }
        }
    }


    void AddMaxHealth() {
        currentMaxHealth = Mathf.Clamp(currentMaxHealth++, 0, 5);
        currentHealth = currentMaxHealth;
        UpdateHealthBoxes();
    }


    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        if (isInvincible) { return; }

        // If the player is in warning state, die immediately.
        if (isInWarningState) {
            currentHealth = 0;
            currentMaxHealth = 0;
        }

        Services.uiManager.healthWarningScreen.SetActive(true);

        currentMaxHealth--;
        currentHealth = 1;

        invincibilityTimer = 0f;
        isInvincible = true;
        healthRechargeTimer = 0f;

        isInWarningState = true;

        UpdateHealthBoxes();

        if (PlayerIsDead) {
            GameEventManager.instance.FireEvent(new GameEvents.GameOver());
        }
    }

}
