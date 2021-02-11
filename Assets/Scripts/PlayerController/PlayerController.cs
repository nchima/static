using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerController : MonoBehaviour {

    // MOVEMENT
    [SerializeField] float accelerationSpeedGround;
    [SerializeField] float accelerationSpeedAir;
    [SerializeField] float deccelerationSpeed = 150f;
    [SerializeField] float minSpeed;
    public float maxGroundSpeed;
    public float maxAirSpeed;
    [SerializeField] float maxFallingSpeed;
    [HideInInspector] public bool isMovementEnabled = false;
    Vector3 unrotatedVelocity = Vector3.zero;

    [SerializeField] float movementKickForce = 5f;
    Vector2 lastKickDirection = new Vector3(0f, 0f, 1f);
    bool movementKickReady = true;
    float movementKickTimer = 0f;
    float movementKickCooldown = 0.3f;

    float knockbackModifier = 1f;
    float knockbackTimer = 0f;

    // SHOTGUN CHARGE STUFF
    float shotGunChargeSpeed = 1000f;

    // DASHING STUFF (this all deprecated but I'll leave it in because the code could be usefully repurposed)
    [SerializeField] float dashDistance = 20f;
    [SerializeField] float dashCooldown = 0.5f;
    [SerializeField] float dashSpeed = 10f;
    [SerializeField] float superDashHoldDuration = 0.1f;   // How long the player must hold the dash button to activate a super dash
    const float DASH_CHARGING_MOVEMENT_MOD = 0.4f;
    const float DASH_RECHARGE_TIME = .001f;
    public float dashRechargeTimer = 5f;
    [HideInInspector] public bool isSuperDashCharging;
    float superDashHoldTimer;
    float dashCooldownTimer;
    Coroutine dashCoroutine;

    // FALLING THROUGH FLOOR STUFF
    [SerializeField] float fallThroughFloorCooldown = 5f;
    [HideInInspector] public float fallThroughFloorTimer = 0f;

    // PHYSICS MATERIAL STUFF
    float normalBounciness;

    // TURNING
    private Quaternion targetRotation;

    // STATE    
    public enum State { Normal, ShotgunCharge, Falling, SpeedFalling, Dashing, GettingTased, Dead }
    public State state;
    public enum FallSpeedControlType { Mouse, Button }
    public FallSpeedControlType fallSpeedControlType = FallSpeedControlType.Button;

    // INPUT
    Vector2 directionalInput;
    float mouseInput;

    // MISC
    Rigidbody m_Rigidbody;
    public bool isAboveFloor {
        get {
            bool returnValue = false;

            // See if the player is over a floor tile.
            RaycastHit hit1;
            RaycastHit hit2;

            float colliderRadius = GetComponent<CapsuleCollider>().radius;

            // If we didn't find anything, return false.
            if (!Physics.Raycast(transform.position + transform.forward * colliderRadius*0.9f, Vector3.down, out hit1, 20f, (1 << 20 | 1 << 24))) { return false; }
            if (!Physics.Raycast(transform.position + transform.forward * -colliderRadius*0.9f, Vector3.down, out hit2, 20f, (1 << 20 | 1 << 24))) { return false; }

            // If both things hit something and it was the floor, we're all good baby!
            if (hit1.transform.name.ToLower().Contains("floor") && hit2.transform.name.ToLower().Contains("floor")) {
                returnValue = true;
            }

            // If it wasn't the floor, return false.
            else {
                return false;
            }

            return returnValue;
        }
    }
    public static Vector3 currentVelocity;
    Vector3 previousPosition;   // Used to calculate velocity.
    [HideInInspector] public bool skipRotationForThisFrame = false;
    
    private void Awake() {
        m_Rigidbody = GetComponent<Rigidbody>();
        dashCooldownTimer = dashCooldown;
    }

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameOver>(GameOverHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameStarted>(GameStartedHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameOver>(GameOverHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameStarted>(GameStartedHandler);
    }

    private void Start() {
        targetRotation = transform.localRotation;
        normalBounciness = GetComponent<Collider>().material.bounciness;
    }

    private void Update() {

        if (GameManager.isGamePaused || state == State.Dead) { return; }

        dashRechargeTimer += Time.deltaTime;
        dashRechargeTimer = Mathf.Clamp(dashRechargeTimer, 0f, DASH_RECHARGE_TIME);

        /* GET DIRECTIONAL INPUT */
        if (state != State.ShotgunCharge
            && Physics.Raycast(transform.position + transform.forward * 0.75f, Vector3.down, 5f, 1 << 20)
            || (state == State.Falling || state == State.SpeedFalling)) {
            directionalInput = InputManager.movementAxis;
        }

        /* HANDLE VIEW ROTATION */
        if (skipRotationForThisFrame) {
            skipRotationForThisFrame = false;
            targetRotation = Quaternion.identity;
        } else {
            //if (InputManager.inputMode == InputManager.InputMode.Controller) { _mouseSensitivity = controllerSensitivity; }
            //if (state == State.ShotgunCharge) _mouseSensitivity = shotGunChargeMouseSensitivity;

            mouseInput = InputManager.turningValue * (1 / Time.timeScale) * Time.deltaTime;
            if (InputManager.inputMode ==  InputManager.InputMode.Controller && (!Services.gameManager.isGameStarted || GameManager.isGamePaused)) {
                mouseInput = InputManager.movementAxis.x * (1 / Time.timeScale) * Time.deltaTime;
            }
            float rotation = mouseInput * 50f;
            targetRotation *= Quaternion.Euler(0f, rotation, 0f);
            transform.localRotation = targetRotation;
        }

        /* HANDLE DASHING INPUT */
        if (state == State.Normal) {
            //dashCooldownTimer += Time.deltaTime;
            //if (dashCooldownTimer >= dashCooldown && dashRechargeTimer >= DASH_RECHARGE_TIME) {
            //    if (InputManager.dashButtonDown && !isSuperDashCharging) {
            //        BeginDash(false);
            //    }

            //    if (InputManager.dashButton && !isSuperDashCharging) {
            //        superDashHoldTimer += Time.deltaTime;
            //        if (superDashHoldTimer > superDashHoldDuration /*&& Services.specialBarManager.bothBarsFull*/) {
            //            if (!Services.specialMoveManager.HasAmmo) { return; }
            //            Services.specialBarManager.PlayerUsedSpecialMove();
            //            Services.levelManager.SetFloorCollidersActive(false);
            //            //FindObjectOfType<ShotgunCharge>().BeginSequence();
            //            //Services.fallingSequenceManager.BeginFalling();
            //            //isSuperDashCharging = true;
            //            //BeginDash(true);
            //        }
            //    } else {
            //        superDashHoldTimer = 0f;
            //    }
            //}

            // Increment fall through floor cooldown timer.
            if (!Services.fallingSequenceManager.isPlayerFalling) { fallThroughFloorTimer += Time.deltaTime; }

            if (InputManager.dashButtonDown && fallThroughFloorTimer >= fallThroughFloorCooldown) {
                //Services.comboManager.EndCombo();
                //Services.specialBarManager.PlayerUsedSpecialMove();
                var newVelocity = m_Rigidbody.velocity;
                newVelocity.y = -maxFallingSpeed;
                m_Rigidbody.velocity = newVelocity;
                GameEventManager.instance.FireEvent(new GameEvents.PlayerUsedFallThroughFloorMove());
            }
        }

        HandleCursorLocking();
    }

    private void FixedUpdate() {

        currentVelocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;

        /* HANDLE MOVEMENT */
        if (state == State.Dead || state == State.GettingTased) { return; }
        if (!isMovementEnabled) return;

        // Get desired movement from input.
        Vector3 desiredMove = transform.forward * directionalInput.y + transform.right * directionalInput.x;

        // Normal movement state
        if (state == State.Normal) {

            // Ground check
            if (!isAboveFloor) {
                m_Rigidbody.useGravity = true;
                return;
            } else {
                m_Rigidbody.useGravity = false;
            }

            // Deceleration
            float decelerateTo = maxGroundSpeed;
            if (directionalInput.magnitude == 0) { decelerateTo = 0f; }
            if (m_Rigidbody.velocity.magnitude > decelerateTo) {
                Vector3 deccelerateForce = m_Rigidbody.velocity.normalized * -deccelerationSpeed;
                deccelerateForce.y = 0;
                m_Rigidbody.AddForce(deccelerateForce, ForceMode.Acceleration);
            }

            // If we're going slowly enough, just stop instantly.
            if (m_Rigidbody.velocity.magnitude < 5) {
                m_Rigidbody.velocity = Vector3.zero;
            }

            // Get actual walking force.
            Vector3 walkForce = desiredMove.normalized * accelerationSpeedGround;
            walkForce.y = 0f;
            m_Rigidbody.AddForce(walkForce, ForceMode.Acceleration);

            // Apply movement kick.
            CheckMovementKick();
            if (directionalInput != Vector2.zero && movementKickReady) {
                AddMovementKick(desiredMove);
            }

            // Keep player at floor level.
            if (isAboveFloor) { m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, 0f, m_Rigidbody.velocity.z); }

            // Add charging movement.
            float tempSpeed = maxGroundSpeed;
            if (isSuperDashCharging) { tempSpeed *= DASH_CHARGING_MOVEMENT_MOD; }

            // Add knockback & decrease modifier over time.
            tempSpeed *= knockbackModifier;
            if (knockbackModifier != 1 && knockbackTimer >= 0.65f) {
                knockbackModifier = 1f;
                AddMovementKick(desiredMove);
            } else {
                knockbackTimer += Time.deltaTime;
            }

            m_Rigidbody.velocity = Vector3.ClampMagnitude(m_Rigidbody.velocity, tempSpeed);
        }

        
        // Handle air movement.
        else if (state == State.Falling || state == State.SpeedFalling) {
            if (desiredMove == Vector3.zero) {
                m_Rigidbody.velocity = new Vector3(0f, m_Rigidbody.velocity.y, 0f);
            }
            else {
                // Enable the following line to have air speed be based on gun value
                // float tempAirSpeed = GunValueManager.MapTo(maxAirSpeed, 50f);
                float tempAirSpeed = maxAirSpeed;
                m_Rigidbody.MovePosition(transform.position + desiredMove.normalized * tempAirSpeed * Time.fixedDeltaTime);
            }

            switch (fallSpeedControlType)
            {
                case FallSpeedControlType.Button:
                    m_Rigidbody.velocity = Vector3.ClampMagnitude(m_Rigidbody.velocity, 1400f);
                    break;

                case FallSpeedControlType.Mouse:
                    float tempFallingSpeed = GunValueManager.MapTo(200f, 1400f);
                    m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, -tempFallingSpeed, m_Rigidbody.velocity.z);
                    break;
            }
        }
        
        // Handle shotgun charge movement.
        else if (state == State.ShotgunCharge) {
            // Add forward acceleration.
            m_Rigidbody.AddForce(transform.forward * shotGunChargeSpeed, ForceMode.Acceleration);
        }

        // Handle dashing movement.
        else if (state == State.Dashing) {

        }

        // Failsafe for getting teleported outside level.
        if (transform.position.x > 10000f || transform.position.z > 10000f) {
            transform.position = new Vector3(0f, transform.position.y, 0f);
        }
    }

    void CheckMovementKick() {
        // Check whether we should add movement kick.
        movementKickTimer += Time.fixedDeltaTime;
        if (!movementKickReady) {
            if (directionalInput == Vector2.zero && movementKickTimer >= movementKickCooldown) movementKickReady = true;
            else if (Vector2.Angle(lastKickDirection, directionalInput) > 30f) movementKickReady = true;
        }
    }

    void AddMovementKick(Vector3 desiredMove) {
        lastKickDirection = directionalInput;
        movementKickTimer = 0f;
        movementKickReady = false;
        Vector3 kickForce = desiredMove.normalized * movementKickForce;
        kickForce.y = 0f;
        m_Rigidbody.AddForce(kickForce, ForceMode.VelocityChange);
    }

    void HandleCursorLocking() {
        if (!Services.gameManager.isGameStarted) { return; }

        if (InputManager.pauseButtonDown) {
            UnlockCursor();
        }

        else if (InputManager.fireButtonDown) {
            LockCursor();
        }
    }

    public void LockCursor() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void AddRecoil(float forceAmt) {
        //transform.Find("FirstPersonCharacter").transform.DOBlendableLocalRotateBy(new Vector3(-0.1f, 0f, 0f), 0.1f, RotateMode.Fast).SetLoops(1, LoopType.Yoyo);
        foreach (ScreenShake screenShake in FindObjectsOfType<ScreenShake>()) screenShake.SetShake(forceAmt * 0.0005f, 0.1f);

        Vector3 forceVector = -transform.forward;
        forceVector.y = 0.5f;
        forceVector *= forceAmt;
        GetComponent<Rigidbody>().AddForce(forceVector, ForceMode.Impulse);
    }

    void BeginDash(bool buttonHeld) {
        state = State.Dashing;
        dashCoroutine = StartCoroutine(DashCoroutine(buttonHeld));
    }

    IEnumerator DashCoroutine(bool buttonHeld) {
        Vector3 dashDirection = transform.forward * directionalInput.y + transform.right * directionalInput.x;
        bool railingCollisionsIgnored = false;

        if (dashDirection.magnitude < 0.2f) {
            // Do nothing
            //dashDirection = transform.forward;
        } else {
            m_Rigidbody.velocity = dashDirection * dashSpeed;
            m_Rigidbody.useGravity = false;
            Vector3 startingPosition = transform.position;
            if (buttonHeld) {
                IgnoreCollisionsWithRailings(true);
                railingCollisionsIgnored = true;
            }

            yield return new WaitUntil(() => {
                if ((Vector3.Distance(startingPosition, transform.position) >= dashDistance)) {
                    return true;
                } else { return false; }
            });
        }

        dashCooldownTimer = 0f;
        movementKickReady = true;
        m_Rigidbody.useGravity = true;
        if (railingCollisionsIgnored) { IgnoreCollisionsWithRailings(false); }
        state = State.Normal;

        yield return null;
    }

    void EndDashEarly() {
        if (state == State.Falling || state == State.SpeedFalling) { return; }
        if (dashCoroutine != null) { StopCoroutine(dashCoroutine); }
        m_Rigidbody.velocity = Vector3.zero;
        IgnoreCollisionsWithRailings(false);
        dashCooldown = 0f;
        movementKickReady = true;
        state = State.Normal;
    }

    void IgnoreCollisionsWithRailings(bool ignore) {
        foreach(GameObject railing in GameObject.FindGameObjectsWithTag("Railing")) {
            Physics.IgnoreCollision(GetComponent<Collider>(), railing.GetComponent<Collider>(), ignore);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        // If the player collides with a wall while shotgun charging, end the shotgun charge.
        if (state == State.ShotgunCharge && collision.collider.name.ToLower().Contains("wall") || collision.collider.name.ToLower().Contains("obstacle")) {
            //FindObjectOfType<Gun>().EndShotgunCharge();
        }

        if (state == State.Dashing) {
            if (collision.collider.name.ToLower().Contains("wall") || collision.collider.name.ToLower().Contains("obstacle") || collision.collider.tag == "Enemy"
                || collision.collider.name.ToLower().Contains("railing")) {
                EndDashEarly();
            }
        }
    }

    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        if (Services.healthManager.isInvincible) { return; }

        EndDashEarly();
        knockbackModifier = 0.3f;
        knockbackTimer = 0f;
        movementKickReady = true;
    }

    public void GameOverHandler(GameEvent gameEvent) {
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        state = State.Dead;
        this.enabled = false;
    }

    public void GameStartedHandler(GameEvent gameEvent) {
        isMovementEnabled = true;
    }
}
