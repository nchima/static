using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour {

    // MOVEMENT
    [SerializeField] float accelerationSpeedGround;
    [SerializeField] float accelerationSpeedAir;
    [SerializeField] float minSpeed;
    public float maxGroundSpeed;
    public float maxAirSpeed;
    [SerializeField] float maxFallingSpeed;
    [HideInInspector] public bool isMovementEnabled = true;

    [SerializeField] float movementKickForce = 5f;
    Vector2 lastKickDirection = new Vector3(0f, 0f, 1f);
    bool movementKickReady = true;
    float movementKickTimer = 0f;
    float movementKickCooldown = 0.6f;

    // SHOTGUN CHARGE STUFF
    float shotGunChargeSpeed = 300f;
    float shotGunChargeMouseSensitivity = 0.76f;

    // PHYSICS MATERIAL STUFF
    float normalBounciness;

    // TURNING
    [SerializeField] float mouseSensitivity;
    private Quaternion targetRotation;

    // STATE    
    public enum State { Normal, ShotgunCharge, Falling, SpeedFalling }
    public State state;

    // INPUT
    Vector2 directionalInput;
    float mouseInput;

    // MISC
    Rigidbody rigidBody;
    public bool isAboveFloor {
        get {
            bool returnValue = false;

            // See if the player is over a floor tile.
            RaycastHit hit1;
            RaycastHit hit2;

            float colliderRadius = GetComponent<CapsuleCollider>().radius;

            // If we didn't find anything, return false.
            if (!Physics.Raycast(transform.position + transform.forward * colliderRadius * 1.1f, Vector3.down, out hit1, 5f, (1 << 20 | 1 << 24))) { return false; }
            if (!Physics.Raycast(transform.position + transform.forward * -colliderRadius * 1.1f, Vector3.down, out hit2, 5f, (1 << 20 | 1 << 24))) { return false; }

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
    public static Vector3 velocity;
    Vector3 previousPosition;   // Used to calculate velocity.



    private void Start() {
        targetRotation = transform.localRotation;
        rigidBody = GetComponent<Rigidbody>();
        normalBounciness = GetComponent<Collider>().material.bounciness;
    }


    private void Update() {

        /* GET DIRECTIONAL INPUT */
        if (state != State.ShotgunCharge
            && Physics.Raycast(transform.position + transform.forward * 0.75f, Vector3.down, 5f, 1 << 20)
            || (state == State.Falling || state == State.SpeedFalling)) {
            directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        /* HANDLE VIEW ROTATION */
        float _mouseSensitivity = mouseSensitivity;
        if (state == State.ShotgunCharge) _mouseSensitivity = shotGunChargeMouseSensitivity;
        mouseInput = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float rotation = mouseInput;
        targetRotation *= Quaternion.Euler(0f, rotation, 0f);
        transform.localRotation = targetRotation;

        HandleCursorLocking();
    }


    private void FixedUpdate() {

        velocity = (transform.position - previousPosition) / Time.deltaTime;
        previousPosition = transform.position;

        /* HANDLE MOVEMENT */
        if (!isMovementEnabled) { return; }

        if (state == State.Normal || state == State.Falling || state == State.SpeedFalling) {
            float _accelerationSpeed = accelerationSpeedGround;
            float _maxSpeed = maxGroundSpeed;

            if (state == State.Falling || state == State.SpeedFalling) {
                _accelerationSpeed = accelerationSpeedAir;
                _maxSpeed = maxAirSpeed;
            }

            if (directionalInput.sqrMagnitude > 1) directionalInput.Normalize();

            // Get desired movement direction.
            Vector3 desiredMove = transform.forward * directionalInput.y + transform.right * directionalInput.x;

            //Vector3 desiredMove = transform.TransformDirection(new Vector3(directionalInput.x, 0f, directionalInput.y));
            //Debug.Log("Direction Input X: " + directionalInput.x + ", Direction input Y: " + directionalInput.y + "Desired move: " + transform.InverseTransformDirection(desiredMove));

            // Deccelerate.
            Vector3 deccelerateForce = rigidBody.velocity.normalized * rigidBody.velocity.sqrMagnitude * -1.12f;
            deccelerateForce.y = 0;
            rigidBody.AddForce(deccelerateForce, ForceMode.Force);

            // Apply movement force to rigidbody.
            if (desiredMove.magnitude != 0)
                rigidBody.AddForce(desiredMove.normalized * _accelerationSpeed, ForceMode.VelocityChange);

            // Check whether we should add movement kick.
            movementKickTimer += Time.deltaTime;
            if (!movementKickReady) {
                if (directionalInput == Vector2.zero && movementKickTimer >= movementKickCooldown) movementKickReady = true;
                else if (Vector2.Angle(lastKickDirection, directionalInput) > 30f) movementKickReady = true;
            }

            // Add movement kick.
            if (directionalInput != Vector2.zero) {
                if (movementKickReady && state != State.Falling && state != State.SpeedFalling) {
                    lastKickDirection = directionalInput;
                    movementKickTimer = 0f;
                    movementKickReady = false;
                    rigidBody.AddForce(desiredMove.normalized * movementKickForce, ForceMode.Impulse);
                }
            }

            // Clamp velocity to max speed.
            if (state == State.Falling || state == State.SpeedFalling) {
                //rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxFallingSpeed);
                rigidBody.velocity = new Vector3(
                    Mathf.Clamp(rigidBody.velocity.x, -maxAirSpeed * 0.5f, maxAirSpeed * 0.5f),
                    Mathf.Clamp(rigidBody.velocity.y, -maxFallingSpeed, maxFallingSpeed),
                    Mathf.Clamp(rigidBody.velocity.z, -maxAirSpeed * 0.5f, maxAirSpeed * 0.5f)
                    );
            } else {
                rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxGroundSpeed);
            }
        }

        else if (state == State.ShotgunCharge) {
            // Add forward acceleration.
            rigidBody.AddForce(transform.forward * shotGunChargeSpeed, ForceMode.Acceleration);
        }

        if (transform.position.x > 10000f || transform.position.z > 10000f) {
            transform.position = new Vector3(0f, transform.position.y, 0f);
        }
    }


    void HandleCursorLocking()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            UnlockCursor();
        }

        else if (Input.GetMouseButtonUp(0))
        {
            LockCursor();
        }
    }


    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
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

    private void OnCollisionEnter(Collision collision) {
        // If the player collides with a wall while shotgun charging, end the shotgun charge.
        if (state == State.ShotgunCharge && collision.collider.name.ToLower().Contains("wall") || collision.collider.name.ToLower().Contains("obstacle")) {
            //FindObjectOfType<Gun>().EndShotgunCharge();
        }
    }
}
