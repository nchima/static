using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour {

    // How quickly each 'block' of health recharges.
    [SerializeField] float healthRechargeRate;
    float timer;

    // Player's health.
    int _playerHealth = 5;
    public int playerHealth {
        get {
            return _playerHealth;
        }

        set {
            // If health is decreasing.
            if (value < _playerHealth) {
                if (currentlyInvincible) { return; }
                invincibilityTimer += invincibilityTime;

                // Audio/visuals
                getHurtAudio.Play();
                GameObject.Find("Screen").BroadcastMessage("IncreaseShake", 1f);
                GameManager.colorPaletteManager.LoadVulnerablePalette();
                GameManager.player.GetComponent<Rigidbody>().velocity *= 0.01f;
                GameObject.Find("Pain Flash").GetComponent<Animator>().SetTrigger("Pain Flash");

                // Delete health box.
                for (int i = Mathf.Clamp(value, 0, 4); i < 5; i++) healthBlocks[i].SetActive(false);
            }

            // If health is increasing.
            else {
                // Reactivate health box.
                if (value <= 5) healthBlocks[Mathf.Clamp(value - 1, 0, 5)].SetActive(true);
                if (value == 5) GameManager.colorPaletteManager.RestoreSavedPalette();
            }

            _playerHealth = value;
        }
    }

    // Used for invincibility frames.
    [SerializeField] bool godMode;  // For debugging.
    [HideInInspector] public bool forceInvincibility;    // Used by other scripts to force player invincibility at certain times.
    bool currentlyInvincible;
    [SerializeField] float invincibilityTime = 0.5f;
    float invincibilityTimer = 0f;

    // Misc references.
    [SerializeField] GameObject[] healthBlocks;
    [SerializeField] AudioSource getHurtAudio;


    private void Update() {

        // Handle health recharging.
        if (playerHealth < 5) {
            timer += Time.deltaTime;
            if (timer >= healthRechargeRate) {
                playerHealth++;
                timer = 0f;
            }
        }

        // Check invincibility frames.
        invincibilityTimer = Mathf.Clamp(invincibilityTimer, 0f, invincibilityTime);
        if (invincibilityTimer > 0) {
            currentlyInvincible = true;
            invincibilityTimer -= Time.deltaTime;
        } else {
            currentlyInvincible = false;
        }

        if (forceInvincibility || godMode) currentlyInvincible = true;
    }
}
