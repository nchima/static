using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryEnemyState_PoweringUp : State {

    [SerializeField] private float openUpTime = 1f;
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

        // Have body open and orb rise
        controller.emergeAudio.Play();
        controller.m_AnimationController.OpenTop(100f, openUpTime);
        controller.m_AnimationController.OrbRise(openUpTime);
        controller.m_AnimationController.BloomOrb(100f, openUpTime);
        yield return new WaitForSeconds(openUpTime);

        targettingLine1.enabled = true;
        targettingLine2.enabled = true;

        // Show targetting lines and have them converge on the player's position.
        controller.powerUpAudio.Play();
        controller.powerUpAudio.volume = 1f;
        controller.m_AnimationController.MakeOrbAngry(100f, powerUpTime * 0.9f);
        float reusableTimer = 0f;
        yield return new WaitUntil(() => {
            reusableTimer += Time.deltaTime;

            // Make targetting lines creep towards player
            float progress = MyMath.Map(reusableTimer, 0f, powerUpTime, 0f, 1f);

            targettingLine1.SetPosition(0, controller.enemyTop.transform.position);
            targettingLine1.SetPosition(1, GetSecondLinePosition(targettingLine1.GetPosition(0), 90f, progress));

            targettingLine2.SetPosition(0, controller.enemyTop.transform.position);
            targettingLine2.SetPosition(1, GetSecondLinePosition(targettingLine2.GetPosition(0), -90f, progress));

            controller.powerUpAudio.pitch = MyMath.Map(progress, 0f, 0.75f, 1f, 3f);

            if (reusableTimer < powerUpTime + warningTime) {
                return false;
            }

            else {
                return true;
            }
        });

        // Fade targetting lines out.
        reusableTimer = 0f;
        float originalWidthMultiplier = targettingLine1.widthMultiplier;
        yield return new WaitUntil(() => {
            reusableTimer += Time.deltaTime;

            targettingLine1.SetPosition(0, controller.enemyTop.transform.position);
            targettingLine1.SetPosition(1, GetSecondLinePosition(targettingLine1.GetPosition(0), 90f, 1f));

            targettingLine2.SetPosition(0, controller.enemyTop.transform.position);
            targettingLine2.SetPosition(1, GetSecondLinePosition(targettingLine2.GetPosition(0), -90f, 1f));

            targettingLine1.widthMultiplier = MyMath.Map(reusableTimer, 0f, warningTime * 0.5f, originalWidthMultiplier, 0f);
            targettingLine2.widthMultiplier = MyMath.Map(reusableTimer, 0f, warningTime * 0.5f, originalWidthMultiplier, 0f);

            controller.powerUpAudio.volume = MyMath.Map(reusableTimer, 0f, warningTime, 1f, 0f);
            controller.powerUpAudio.pitch = MyMath.Map(reusableTimer, 0f, warningTime, 3f, 1f);

            if (reusableTimer >= warningTime) { return true; }
            else { return false; }
        });

        targettingLine1.enabled = false;
        targettingLine2.enabled = false;

        controller.powerUpAudio.Stop();
        controller.powerUpAudio.volume = 1f;
        controller.powerUpAudio.pitch = 1f;

        yield return new WaitForSeconds(warningTime * 0.7f);

        controller.emergeAudio.Play();

        yield return new WaitForSeconds(warningTime * 0.3f);

        // Fire the laser:
        controller.shootAudio.Play();

        targettingLine1.enabled = true;
        targettingLine2.enabled = false;

        targettingLine1.SetPosition(0, controller.enemyTop.transform.position);
        targettingLine1.SetPosition(1, GetSecondLinePosition(targettingLine1.GetPosition(0), 90f, 1f));

        //targettingLine2.SetPosition(0, controller.enemyTop.transform.position);
        //targettingLine2.SetPosition(1, GetSecondLinePosition(targettingLine2.GetPosition(0), -90f, 1f));

        yield return new WaitForEndOfFrame();

        targettingLine1.widthMultiplier = laserWidth;

        // Spawn an explosion at the end of the laser.
        Explosion explosion = Instantiate(explosionPrefab).GetComponent<Explosion>();
        explosion.gameObject.transform.position = targettingLine1.GetPosition(1);

        // Begin the laser decay animation:
        targettingLine1.positionCount = 10;

        reusableTimer = 0f;
        Vector3 lineEndPosition = targettingLine1.GetPosition(1);
        yield return new WaitUntil(() => {
            reusableTimer += Time.deltaTime;
            float t = MyMath.Map01(reusableTimer, 0f, firingTime);
            float lerpValue = t * t * t;

            for (int i = 1; i < targettingLine1.positionCount; i++) {
                Vector3 newPosition = Vector3.Lerp(targettingLine1.GetPosition(0), lineEndPosition, MyMath.Map01(i, 0, targettingLine1.positionCount - 1));
                newPosition += Random.insideUnitSphere * MyMath.Map(lerpValue, 0f, firingTime, 0f, 20f);
                targettingLine1.SetPosition(i, newPosition);
            }

            targettingLine1.widthMultiplier = MyMath.Map(lerpValue, 0f, firingTime, laserWidth, 0f);

            if (lerpValue >= firingTime) { return true; }
            else { return false; }
        });

        targettingLine1.positionCount = 2;
        targettingLine1.widthMultiplier = originalWidthMultiplier;
        targettingLine2.widthMultiplier = originalWidthMultiplier;

        targettingLine1.enabled = false;

        // Retract back to normal shape.
        controller.m_AnimationController.OpenTop(0f, afterFiringPause * 0.8f);
        controller.m_AnimationController.OrbDescend(afterFiringPause * 0.8f);
        controller.m_AnimationController.BloomOrb(0f, afterFiringPause * 0.8f);
        controller.m_AnimationController.MakeOrbAngry(0f, afterFiringPause * 0.8f);

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
