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
    [SerializeField] float hangTime = 5f;
    [SerializeField] float nearFOV;
    [SerializeField] float middleFOV;
    [SerializeField] float farFOV;
    [SerializeField] float nearOrthoSize;
    [SerializeField] float middleOrthoSize;
    [SerializeField] float farOrthoSize;
    [SerializeField] public SpecialMoveMode specialMoveMode;

    // References
    [SerializeField] Transform cameraNearPoint;
    [SerializeField] Transform cameraMidPoint;
    [SerializeField] Transform cameraFarPoint;
    [SerializeField] GameObject specialMoveShieldPrefab;
    [SerializeField] ObjectPooler fireFromGroundMissilePooler;
    [SerializeField] ObjectPooler fireWhileFallingMissilePooler;
    [SerializeField] bool debugInfiniteAmmo = false;

    /* OTHER STUF */
    public bool HasAmmo { 
        get { 
            if (debugInfiniteAmmo == true) return true;
            else return Services.specialBarManager.BothFirstBarsFull || Services.specialBarManager.ShotsSaved > 0; 
        } 
    }

    int missilesFired = 0;
    float missileTimer;
    bool isFiringMissiles = false;
    [HideInInspector] public bool fireMissilesWhenFallingTrigger;    // Trigger used to activate the special missiles as soon as the now entering screen disappears
    bool returnCameraTrigger = false;    // Used to exit missile firing state early.
    [HideInInspector] public bool canShoot = false;
    Vector3 originalCameraPosition;
    Quaternion originalCameraRotation;
    float originalFOV;
    float originalOrthoSize;
    Coroutine cameraMovementCoroutine;
    enum CameraState { Idle, PullingBack, PulledBack, WaitingForMoveToFinish, Returning }
    CameraState cameraState = CameraState.Idle;
    public enum SpecialMoveMode { FireWhileOnGround, ActivateFallingSequence }
    private ObjectPooler missilePooler;

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerLanded>(PlayerLandedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerLookedDown>(PlayerLookedDownHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerLanded>(PlayerLandedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerLookedDown>(PlayerLookedDownHandler);
    }

    public void LevelCompletedHandler(GameEvent gameEvent) {
        returnCameraTrigger = true;
    }

    public void PlayerLandedHandler(GameEvent gameEvent) {
        isFiringMissiles = false;
    }

    public void PlayerLookedDownHandler(GameEvent gameEvent) {
        if (fireMissilesWhenFallingTrigger) {
            isFiringMissiles = true;
            fireMissilesWhenFallingTrigger = false;
        }
    }

    private void Awake() {
        switch (specialMoveMode)
        {
            case SpecialMoveMode.ActivateFallingSequence:
                missilePooler = fireWhileFallingMissilePooler;
                Destroy(fireFromGroundMissilePooler.gameObject);
                break;

            case SpecialMoveMode.FireWhileOnGround:
                missilePooler = fireFromGroundMissilePooler;
                Destroy(fireWhileFallingMissilePooler.gameObject);
                break;
        }
    }

    private void Start() {
        originalCameraPosition = Services.fieldOfViewController.transform.localPosition;
        originalCameraRotation = Quaternion.Euler(Vector3.zero);
        originalFOV = Services.fieldOfViewController.CurrentFOV;
        originalOrthoSize = Services.fieldOfViewController.orthographicCams[0].orthographicSize;
    }

    private void Update() {

        if (cameraState == CameraState.Idle) {
            // See if the player has fired a special move & if so, initialize proper variables.
            
            if (GetIsActivated()) {    
                missilesFired = 0;
                missileTimer = 0f;
                returnCameraTrigger = false;
                GameEventManager.instance.FireEvent(new GameEvents.PlayerUsedSpecialMove());
                Services.specialBarManager.PlayerUsedSpecialMove();
            
                switch (specialMoveMode) {
                    case SpecialMoveMode.FireWhileOnGround:
                        // Begin the on-ground firing sequence
                        //cameraMovementCoroutine = StartCoroutine(MoveCameraToPositionCoroutine(GetCameraPullbackPosition(), GetCameraPullbackRotation().eulerAngles, CameraState.PulledBack));
                        Services.gun.canShoot = false;
                        cameraMovementCoroutine = StartCoroutine(FireWhileOnGroundCoroutine());
                        isFiringMissiles = true;
                        //cameraState = CameraState.WaitingForMoveToFinish;
                        break;

                    case SpecialMoveMode.ActivateFallingSequence:
                        if (Services.fallingSequenceManager.isPlayerFalling) {
                            isFiringMissiles = true;
                        }
                        else {
                            fireMissilesWhenFallingTrigger = true;
                        }
                        break;
                }

            }
        }

        // Deprecated:
        // else if (cameraState == CameraState.PullingBack) {
            // Wait for the coroutine to complete.
            //cameraState = CameraState.FollowingMouse;
        // }

        // else if (cameraState == CameraState.PulledBack) {
            //Services.fieldOfViewController.transform.localPosition = GetCameraPullbackPosition();
            //Services.fieldOfViewController.transform.localRotation = GetCameraPullbackRotation();

            //if (InputManager.fireButton && canFireMissiles) {
                // Fire the special move.
                //Services.specialBarManager.PlayerUsedSpecialMove();
                //GameEventManager.instance.FireEvent(new GameEvents.PlayerUsedSpecialMove());
                //missilesFired = 0;
                //missileTimer = 0f;
                //canFireMissiles = true;

            //    cameraState = CameraState.WaitingForMoveToFinish;
            //}
        // }

        //else if (cameraState == CameraState.WaitingForMoveToFinish) {}

        //else if (cameraState == CameraState.Returning) {}

        // Decide whether to fire a missile
        bool shouldFire = false;
        switch (specialMoveMode) {
            case SpecialMoveMode.FireWhileOnGround:
                shouldFire = isFiringMissiles;
                break;

            case SpecialMoveMode.ActivateFallingSequence:
                shouldFire = isFiringMissiles;
                shouldFire &= Services.fallingSequenceManager.GetCurrentState() is FallIntoLevelState;
                shouldFire &= missilesFired <= 50;
                if (missilesFired >= 50) isFiringMissiles = false;
                break;
        }

        if (shouldFire) {
            // Services.gun.canShoot = false;
            FireMissiles();
        }
    }

    void FireMissiles() {
        // Spawn shield explosion.
        //Explosion specialMoveShield = Instantiate(specialMoveShieldPrefab, Services.playerTransform.position, Quaternion.identity, Services.playerTransform).GetComponent<Explosion>();
        //specialMoveShield.explosionRadius = GunValueManager.MapToFloatRange(shieldExplosionRadiusRange);

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
        //if (missilesFired >= missilesPerBurst) {
        //    canFireMissiles = false;
        //    cameraMovementCoroutine = StartCoroutine(MoveCameraToPositionCoroutine(originalCameraPosition, originalCameraRotation.eulerAngles, CameraState.Idle));
        //    cameraState = CameraState.Returning;
        //}
    }

    void FireMissile() {
        missilesFired++;
        missileTimer = 0;
        Vector3 newPosition = Services.gun.tip.localPosition + Random.insideUnitSphere * 2f;
        newPosition.x += MyMath.Either1orNegative1 * 4f;
        newPosition.y += 4f;
        newPosition = Services.gun.tip.parent.TransformPoint(newPosition);
        PlayerMissile newMissile = missilePooler.GrabObject().GetComponent<PlayerMissile>();
        newMissile.transform.position = newPosition;
        newMissile.transform.rotation = Services.gun.tip.rotation;
        newMissile.Fire();
    }

    IEnumerator MoveCameraToPositionCoroutine(Vector3 position, Vector3 rotation, float fov, float orthoSize, CameraState completionState) {
        float duration = 0.34f;
        Services.fieldOfViewController.transform.DOLocalMove(position, duration).SetEase(Ease.OutExpo);
        Services.fieldOfViewController.transform.DOLocalRotate(rotation, duration).SetEase(Ease.OutExpo);
        Services.fieldOfViewController.TweenFieldOfView(fov, orthoSize, duration);
        yield return new WaitForSeconds(duration);

        cameraState = completionState;

        yield return null;
    }

    IEnumerator FireWhileOnGroundCoroutine() {
        // Pull camera back.
        float duration = 0.45f;
        Services.fieldOfViewController.transform.DOLocalMove(GetCameraPullbackPosition(), duration).SetEase(Ease.OutExpo);
        Services.fieldOfViewController.transform.DOLocalRotate(GetCameraPullbackRotation().eulerAngles, duration).SetEase(Ease.OutExpo);
        Services.fieldOfViewController.TweenFieldOfView(GetPullbackFOV(), GetPullbackOrthoSize(), duration);
        yield return new WaitForSeconds(duration);

        // Hang out while player can you know fire their missiles or whatever it is they do up there.
        float timer = 0;
        yield return new WaitUntil(() => {
            timer += Time.deltaTime;
            if (timer < hangTime) {
                if  (returnCameraTrigger) {
                    returnCameraTrigger = false;
                    return true;
                }

                Services.fieldOfViewController.transform.localPosition = GetCameraPullbackPosition();
                Services.fieldOfViewController.transform.localRotation = GetCameraPullbackRotation();
                Services.fieldOfViewController.SetFieldOfView(GetPullbackFOV(), GetPullbackOrthoSize());
                return false;
            }

            else {
                return true;
            }
        });
        //yield return new WaitForSeconds(hangTime);

        Services.gun.canShoot = true;
        isFiringMissiles = false;
        Services.taserManager.forcePauseSpecialMove = false;

        // Move the camera back to where it normally is.
        duration = 0.7f;
        Services.fieldOfViewController.transform.DOLocalMove(originalCameraPosition, duration).SetEase(Ease.OutExpo);
        Services.fieldOfViewController.transform.DOLocalRotate(originalCameraRotation.eulerAngles, duration).SetEase(Ease.OutExpo);
        Services.fieldOfViewController.TweenToNormalFOV();
        yield return new WaitForSeconds(duration);

        yield return null;
    }

    Vector3 GetCameraPullbackPosition() {
        Vector3 position = cameraMidPoint.localPosition;

        // Override's mouse position:
        //float fakeGunValue = 0;

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

        // Override's mouse position
        //float fakeGunValue = 0;

        if (GunValueManager.currentValue < 0f) {
            float fakeGunValue = Mathf.Clamp(GunValueManager.currentValue, -1f, 0f);
            rotation = Quaternion.Slerp(cameraNearPoint.localRotation, cameraMidPoint.localRotation, MyMath.Map(fakeGunValue, -1f, 0f, 0f, 1f));
        } else {
            float fakeGunValue = Mathf.Clamp(GunValueManager.currentValue, 0f, 1f);
            rotation = Quaternion.Slerp(cameraMidPoint.localRotation, cameraFarPoint.localRotation, fakeGunValue);
        }

        return rotation;
    }

    float GetPullbackFOV() {
        float fov = Services.fieldOfViewController.CurrentFOV;

        if (GunValueManager.currentValue < 0f) {
            float fakeGunValue = Mathf.Clamp(GunValueManager.currentValue, -1f, 0f);
            fov = Mathf.Lerp(nearFOV, middleFOV, MyMath.Map(fakeGunValue, -1f, 0f, 0f, 1f));
        } else {
            float fakeGunValue = Mathf.Clamp(GunValueManager.currentValue, 0f, 1f);
            fov = Mathf.Lerp(middleFOV, farFOV, fakeGunValue);
        }

        return fov;
    }

    float GetPullbackOrthoSize() {
        float orthoSize = Services.fieldOfViewController.CurrentOrthoSize;

        if (GunValueManager.currentValue < 0f) {
            float fakeGunValue = Mathf.Clamp(GunValueManager.currentValue, -1f, 0f);
            orthoSize = Mathf.Lerp(nearOrthoSize, middleOrthoSize, MyMath.Map(fakeGunValue, -1f, 0f, 0f, 1f));
        } else {
            float fakeGunValue = Mathf.Clamp(GunValueManager.currentValue, 0f, 1f);
            orthoSize = Mathf.Lerp(middleOrthoSize, farOrthoSize, fakeGunValue);
        }

        return orthoSize;
    }

    // Returns true when the player has activated the special move
    bool GetIsActivated() {
        bool isActivated;
        isActivated = !GameManager.isGamePaused;  
        isActivated &= !isFiringMissiles;
        isActivated &= HasAmmo;
        isActivated &= Services.playerController.state != PlayerController.State.Dead;
        isActivated &= InputManager.specialMoveButtonDown;
        if (specialMoveMode == SpecialMoveMode.FireWhileOnGround) {
            isActivated &= !(Services.fallingSequenceManager.GetCurrentState() is FallIntoLevelState);
        }
        return isActivated;
    }
}
