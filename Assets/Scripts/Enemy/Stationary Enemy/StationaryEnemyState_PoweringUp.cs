using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryEnemyState_PoweringUp : State {

    [SerializeField] private float powerUpTime = 3f;
    [SerializeField] private float warningTime = 0.5f;
    [SerializeField] private float firingTime = 0.7f;
    [SerializeField] private float afterFiringPause = 0.8f;
    [SerializeField] private float laserWidth = 6f;
    [SerializeField] GameObject powerUpParticles;
    [SerializeField] LineRenderer targettingLine1;
    [SerializeField] LineRenderer targettingLine2;
    [SerializeField] GameObject explosionPrefab;

    private Coroutine powerUpCoroutine;
 
    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);

        powerUpParticles.SetActive(true);

        if (powerUpCoroutine != null) { StopCoroutine(powerUpCoroutine); }
        powerUpCoroutine = StartCoroutine(PowerUpCoroutine(stateController as StationaryEnemy));
    }

    IEnumerator PowerUpCoroutine(StationaryEnemy controller) {

        targettingLine1.enabled = true;
        targettingLine2.enabled = true;

        // Wait to charge up
        float powerUpTimer = 0f;
        yield return new WaitUntil(() => {
            powerUpTimer += Time.deltaTime;

            // Make targetting lines creep towards player
            float progress = MyMath.Map(powerUpTimer, 0f, powerUpTime, 0f, 1f);

            targettingLine1.SetPosition(0, controller.enemyTop.transform.position);
            targettingLine1.SetPosition(1, GetSecondLinePosition(targettingLine1.GetPosition(0), 90f, progress));

            targettingLine2.SetPosition(0, controller.enemyTop.transform.position);
            targettingLine2.SetPosition(1, GetSecondLinePosition(targettingLine2.GetPosition(0), -90f, progress));

            if (powerUpTimer < powerUpTime + warningTime) {
                return false;
            }

            else {
                return true;
            }
        });

        // Fade targetting lines out.
        powerUpTimer = 0f;
        float originalWidthMultiplier = targettingLine1.widthMultiplier;
        yield return new WaitUntil(() => {
            powerUpTimer += Time.deltaTime;

            targettingLine1.SetPosition(0, controller.enemyTop.transform.position);
            targettingLine1.SetPosition(1, GetSecondLinePosition(targettingLine1.GetPosition(0), 90f, 1f));

            targettingLine2.SetPosition(0, controller.enemyTop.transform.position);
            targettingLine2.SetPosition(1, GetSecondLinePosition(targettingLine2.GetPosition(0), -90f, 1f));

            targettingLine1.widthMultiplier = MyMath.Map(powerUpTimer, 0f, warningTime * 0.5f, originalWidthMultiplier, 0f);
            targettingLine2.widthMultiplier = MyMath.Map(powerUpTimer, 0f, warningTime * 0.5f, originalWidthMultiplier, 0f);

            if (powerUpTimer >= warningTime) { return true; }
            else { return false; }
        });

        targettingLine1.enabled = false;
        targettingLine2.enabled = false;

        yield return new WaitForSeconds(warningTime);

        // Fire the laser:
        targettingLine1.enabled = true;
        targettingLine2.enabled = true;

        targettingLine1.SetPosition(0, controller.enemyTop.transform.position);
        targettingLine1.SetPosition(1, GetSecondLinePosition(targettingLine1.GetPosition(0), 90f, 1f));

        targettingLine2.SetPosition(0, controller.enemyTop.transform.position);
        targettingLine2.SetPosition(1, GetSecondLinePosition(targettingLine2.GetPosition(0), -90f, 1f));

        yield return new WaitForEndOfFrame();

        targettingLine1.widthMultiplier = laserWidth;
        targettingLine2.widthMultiplier = laserWidth;

        // Raycast towards the player and see if we hit em
        Explosion explosion = Instantiate(explosionPrefab).GetComponent<Explosion>();
        explosion.gameObject.transform.position = targettingLine1.GetPosition(1);

        yield return new WaitForSeconds(firingTime);

        targettingLine1.widthMultiplier = originalWidthMultiplier;
        targettingLine2.widthMultiplier = originalWidthMultiplier;

        targettingLine1.enabled = false;
        targettingLine2.enabled = false;

        yield return new WaitForSeconds(afterFiringPause);

        GetComponent<TriggerTransition>().isTriggerSet = true;

        yield return null;
    }

    private Vector3 GetSecondLinePosition(Vector3 firstPosition, float offsetAngle, float progress) {
        Vector3 playerPosition = Services.playerTransform.position;
        playerPosition.y = -0.98f;

        Vector3 toPlayerFromStart = playerPosition - firstPosition;

        Vector3 position2 = firstPosition + ((Quaternion.Euler(0, offsetAngle, 0) * toPlayerFromStart.normalized) * 50f);
        position2.y = 0f;
        position2 = Vector3.Lerp(position2, playerPosition, progress);

        Vector3 directionToPosition2 = Vector3.Normalize(position2 - firstPosition);
        RaycastHit hit;
        if (Physics.Raycast(firstPosition, directionToPosition2, out hit, 10000f, (1 << 20) | (1 << 8))) {
            position2 = hit.point;
        }

        return position2;
    }

    public override void End(StateController stateController) {
        if (powerUpCoroutine != null) StopCoroutine(powerUpCoroutine);
        targettingLine1.enabled = false;
        targettingLine2.enabled = false;
        base.End(stateController);
    }
}
