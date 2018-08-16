using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpecialMoveManager : MonoBehaviour {

    /* INSPECTOR */

    // Variables
    [SerializeField] FloatRange missileFireIntervalRange = new FloatRange(0f, 0.01f);
    [SerializeField] FloatRange shieldExplosionRadiusRange = new FloatRange(20f, 0f);
    [SerializeField] int missilesPerBurst = 16;

    // References
    [SerializeField] Transform cameraNearPoint;
    [SerializeField] Transform cameraMidPoint;
    [SerializeField] Transform cameraFarPoint;
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] GameObject specialMoveShieldPrefab;

    /* OTHER STUF */
    public bool HasAmmo { get { return Services.specialBarManager.BothFirstBarsFull || Services.specialBarManager.ShotsSaved > 0; } }

    int missilesFired = 0;
    float missileTimer;
    bool firingMissiles = false;

    Vector3 originalCameraPosition;
    Quaternion originalCameraRotation;
    Coroutine cameraMovementCoroutine;
    enum CameraState { Idle, PullingBack, FollowingMouse, WaitingForMoveToFinish, Returning }
    CameraState cameraState = CameraState.Idle;


    private void Start() {
        originalCameraPosition = Services.fieldOfViewController.transform.localPosition;
        originalCameraRotation = Quaternion.Euler(Vector3.zero);
    }


    private void Update() {

        if (cameraState == CameraState.Idle) {
            // See if the player has fired a special move & if so, initialize proper variables.
            if (InputManager.specialMoveButtonDown && Services.gun.canShoot && !firingMissiles && HasAmmo) {
                cameraMovementCoroutine = StartCoroutine(MoveCameraToPositionCoroutine(GetCameraPullbackPosition(), GetCameraPullbackRotation().eulerAngles, CameraState.FollowingMouse));
                cameraState = CameraState.PullingBack;
            }
        }

        else if (cameraState == CameraState.PullingBack) {
            // Wait for the coroutine to complete.
            //cameraState = CameraState.FollowingMouse;
        }

        else if (cameraState == CameraState.FollowingMouse) {
            Services.fieldOfViewController.transform.localPosition = GetCameraPullbackPosition();
            Services.fieldOfViewController.transform.localRotation = GetCameraPullbackRotation();

            if (!InputManager.specialMoveButton) {
                // Fire the special move.
                Services.specialBarManager.PlayerUsedSpecialMove();
                GameEventManager.instance.FireEvent(new GameEvents.PlayerUsedSpecialMove());
                missilesFired = 0;
                missileTimer = 0f;
                firingMissiles = true;

                cameraState = CameraState.WaitingForMoveToFinish;
            }
        }

        else if (cameraState == CameraState.WaitingForMoveToFinish) {
            //You know   
        }


        else if (cameraState == CameraState.Returning) {

        }

        // Any state:
        if (firingMissiles) {
            FireMissiles();
        }
    }


    void FireMissiles() {
        // Spawn shield explosion.
        Explosion specialMoveShield = Instantiate(specialMoveShieldPrefab, Services.playerTransform.position, Quaternion.identity, Services.playerTransform).GetComponent<Explosion>();
        specialMoveShield.explosionRadius = GunValueManager.MapToFloatRange(shieldExplosionRadiusRange);

        // Fire a missile every x seconds.
        if (missileTimer >= GunValueManager.MapToFloatRange(missileFireIntervalRange)) {
            int missilesToFireThisFrame = 1 + Mathf.CeilToInt(Time.deltaTime / GunValueManager.MapToFloatRange(missileFireIntervalRange));
            for (int i = 0; i < missilesToFireThisFrame; i++) {
                FireMissile();
            }
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
            cameraMovementCoroutine = StartCoroutine(MoveCameraToPositionCoroutine(originalCameraPosition, originalCameraRotation.eulerAngles, CameraState.Idle));
            cameraState = CameraState.Returning;
        }
    }


    void FireMissile() {
        missilesFired++;
        missileTimer = 0;
        Vector3 newPosition = Services.gun.tip.position + Random.insideUnitSphere * 2f;
        newPosition.y -= 2f;
        PlayerMissile newMissile = Instantiate(missilePrefab, newPosition, Services.gun.tip.rotation).GetComponent<PlayerMissile>();
        newMissile.GetFired();
    }


    IEnumerator MoveCameraToPositionCoroutine(Vector3 position, Vector3 rotation, CameraState completionState) {

        float duration = 0.34f;
        Services.fieldOfViewController.transform.DOLocalMove(position, duration).SetEase(Ease.OutExpo);
        Services.fieldOfViewController.transform.DOLocalRotate(rotation, duration).SetEase(Ease.OutExpo);
        yield return new WaitForSeconds(duration);

        cameraState = completionState;

        yield return null;
    }


    Vector3 GetCameraPullbackPosition() {
        Vector3 position = cameraMidPoint.localPosition;

        if (GunValueManager.currentValue < 0f) {
            float fakeGunValue = Mathf.Clamp(GunValueManager.currentValue, -1f, 0f);
            position = Vector3.Lerp(cameraNearPoint.localPosition, cameraMidPoint.localPosition, MyMath.Map(fakeGunValue, -1f, 0f, 0f, 1f));
        }

        else {
            float fakeGunValue = Mathf.Clamp(GunValueManager.currentValue, 0f, 1f);
            position = Vector3.Lerp(cameraMidPoint.localPosition, cameraFarPoint.localPosition, fakeGunValue);
        }

        return position;
    }


    Quaternion GetCameraPullbackRotation() {
        Quaternion rotation = cameraMidPoint.transform.localRotation;

        if (GunValueManager.currentValue < 0f) {
            float fakeGunValue = Mathf.Clamp(GunValueManager.currentValue, -1f, 0f);
            rotation = Quaternion.Slerp(cameraNearPoint.localRotation, cameraMidPoint.localRotation, MyMath.Map(fakeGunValue, -1f, 0f, 0f, 1f));
        } else {
            float fakeGunValue = Mathf.Clamp(GunValueManager.currentValue, 0f, 1f);
            rotation = Quaternion.Slerp(cameraMidPoint.localRotation, cameraFarPoint.localRotation, fakeGunValue);
        }

        return rotation;
    }
}
