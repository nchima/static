using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialMoveManager : MonoBehaviour {

    /* INSPECTOR */

    // Variables
    [SerializeField] FloatRange missileFireIntervalRange = new FloatRange(0f, 0.01f);
    [SerializeField] FloatRange shieldExplosionRadiusRange = new FloatRange(20f, 0f);
    [SerializeField] int missilesPerBurst = 16;

    // References
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] GameObject specialMoveShieldPrefab;

    /* GENERAL SPECIAL MOVE STUFF */
    [HideInInspector] public bool shotgunChargeIsReady;
    [HideInInspector] public bool missilesAreReady;

    /* MISSILE STUFF */
    int missilesFired = 0;
    float missileTimer;
    bool firingMissiles = false;


    private void Update() {
        // See if the player has fired a special move & if so, initialize proper variables.
        if (InputManager.specialMoveButtonDown && Services.gun.canShoot && Services.specialBarManager.bothBarsFull && !firingMissiles) {
            Services.specialBarManager.PlayerUsedSpecialMove();
            missilesFired = 0;
            missileTimer = 0f;
            firingMissiles = true;
        }

        else if (firingMissiles) {
            FireMissiles();
        }
    }


    void FireMissiles() {
        // Spawn shield explosion.
        Explosion specialMoveShield = Instantiate(specialMoveShieldPrefab, Services.playerTransform.position, Quaternion.identity, Services.playerTransform).GetComponent<Explosion>();
        specialMoveShield.explosionRadius = GunValueManager.MapToFloatRange(shieldExplosionRadiusRange);

        // Fire a missile every x seconds.
        if (missileTimer >= GunValueManager.MapToFloatRange(missileFireIntervalRange)) {
            FireMissile();
        } else {
            missileTimer += Time.deltaTime;
        }

        // If the gun is 100% in shotgun mode, fire all missiles at once.
        while (GunValueManager.MapToFloatRange(missileFireIntervalRange) == 0 && missilesFired < missilesPerBurst) {
            FireMissile();
        }

        // If we have fired all missiles, end firing sequence.
        if (missilesFired >= missilesPerBurst) {
            firingMissiles = false;
        }
    }


    void FireMissile() {
        missilesFired++;
        missileTimer = 0;
        PlayerMissile newMissile = Instantiate(missilePrefab, Services.gun.tip.position, Services.playerTransform.rotation).GetComponent<PlayerMissile>();
        newMissile.GetFired();
    }
}
