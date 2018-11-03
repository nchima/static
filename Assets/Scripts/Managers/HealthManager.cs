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
    [SerializeField] GameObject regainPrompt;

    /* MISC */
    // Player's health.
    int _currentHealth = 3;
    [HideInInspector] public int CurrentHealth {
        get { return _currentHealth; }
        set {
            _currentHealth = value;
            Debug.Log("current health set to: " + _currentHealth);
        }
    }
    public int currentMaxHealth = 3;
    float healthRechargeTimer = 0f;

    bool isInWarningState = false;

    // Health bonuses
    bool isFirstHealthBonusApplied;
    int subsequentHealthBonusesApplied = 0;
    int nextBonus { get { return firstHealthBonusScore + subsequentHealthBonusesApplied * subsequentHealthBonusesScore; } }
    int pointsAppliedTowardsBonus;

    // Invincibility frames.
    [HideInInspector] public bool forceInvincibility;    // Used by other scripts to force player invincibility at certain times.
    [HideInInspector] public bool isInvincible;
    float invincibilityTimer = 0f;

    public bool PlayerIsDead { get { return CurrentHealth <= 0 || currentMaxHealth <= 0; } }

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameStarted>(GameStartedHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameStarted>(GameStartedHandler);
    }

    private void Awake() {
        UpdateHealthBoxes();
    }

    private void Update() {
        // Handle health recharging.
        if (CurrentHealth < currentMaxHealth) {
            healthRechargeTimer += Time.deltaTime;
            if (healthRechargeTimer >= healthRechargeRate) {
                // Add one health point
                CurrentHealth++;
                UpdateHealthBoxes();
                healthRechargeTimer = 0f;

                // If current health has reached max health, exit warning state.
                CheckWarningState();
            }
        }

        // Update regain prompt
        if (regainPrompt.activeInHierarchy) {
            regainPrompt.GetComponentInChildren<TextMesh>().text = Mathf.Abs(nextBonus - pointsAppliedTowardsBonus).ToString();
        }

        // Check for score bonuses.
        if (pointsAppliedTowardsBonus >= nextBonus) {
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
        Debug.Log("Updating health boxes. Current health: " + CurrentHealth.ToString() + ". Current max health: " + currentMaxHealth.ToString());

        for (int i = 0; i < healthBlocks.Length; i++) {
            if (i >= currentMaxHealth) { healthBlocks[i].BecomeXedOut(); }
            else { healthBlocks[i].BecomeUnXedOut(); }
            if (i >= CurrentHealth) { healthBlocks[i].BecomeEmpty(); }
            else { healthBlocks[i].BecomeFull(); }
        }

        MoveRegainPrompt();
    }

    public void AddMaxHealth() {
        currentMaxHealth = Mathf.Clamp(currentMaxHealth + 1, 0, 5);
        CurrentHealth = currentMaxHealth;
        UpdateHealthBoxes();
        CheckWarningState();
    }

    void CheckWarningState() {
        if (CurrentHealth == currentMaxHealth) {
            isInWarningState = false;
            Services.uiManager.healthWarningScreen.SetActive(false);
            Services.colorPaletteManager.LoadLevelPalette();
        }
    }

    public void MoveRegainPrompt() {
        Vector3 promptOffset = new Vector3(1.3f, 0f, 0f);

        if (currentMaxHealth >= 5) {
            regainPrompt.SetActive(false);
            return;
        } else if (currentMaxHealth < 0) {
            return;
        }

        regainPrompt.SetActive(true);
        GameObject target = healthBlocks[currentMaxHealth].gameObject;
        regainPrompt.transform.localPosition = target.transform.localPosition + promptOffset;
    }

    public void ApplyPointsToBonus(int points) {
        if (currentMaxHealth == 5) { return; }
        pointsAppliedTowardsBonus += points;
    }

    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        if (isInvincible) { return; }

        // If the player is in warning state, die immediately.
        //if (isInWarningState) {
        //    CurrentHealth = 0;
        //    currentMaxHealth = 0;
        //}

        //Services.uiManager.healthWarningScreen.SetActive(true);

        currentMaxHealth--;
        CurrentHealth = currentMaxHealth;
        //CurrentHealth = 1;

        invincibilityTimer = 0f;
        isInvincible = true;
        //healthRechargeTimer = 0f;

        //isInWarningState = true;

        UpdateHealthBoxes();

        if (PlayerIsDead) {
            GameEventManager.instance.FireEvent(new GameEvents.GameOver());
        }
    }

    public void GameStartedHandler(GameEvent gameEvent) {
        UpdateHealthBoxes();
    }
}
