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
    private Vector3 velocity;

    [SerializeField] float movementKickForce = 5f;
    Vector2 lastKickDirection = new Vector3(0f, 0f, 1f);
    bool movementKickReady = true;
    float movementKickTimer = 0f;
    float movementKickCooldown = 0.6f;

    // PHYSICS MATERIAL STUFF
    float normalBounciness;

    // FOV
    FloatRange fieldOfViewRange = new FloatRange(58f, 85f);
    FloatRange orthographicSizeRange = new FloatRange(23f, 40f);
    [SerializeField] Camera[] perspectiveCams;
    [SerializeField] Camera[] orthographicCams;

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


    private void Start()
    {
        targetRotation = transform.localRotation;
        rigidBody = GetComponent<Rigidbody>();
        normalBounciness = GetComponent<Collider>().material.bounciness;
    }


    private void Update()
    {
        /* GET INPUT */
        //if (state != State.SpeedFalling)
        //{
            directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        //}

        /* HANDLE VIEW ROTATION */
        mouseInput = Input.GetAxis("Mouse X") * mouseSensitivity;
        float rotation = mouseInput;
        targetRotation *= Quaternion.Euler(0f, rotation, 0f);
        transform.localRotation = targetRotation;

        HandleCursorLocking();
    }


    private void FixedUpdate()
    {
        /* HANDLE MOVEMENT */
        float _accelerationSpeed = accelerationSpeedGround;
        float _maxSpeed = maxGroundSpeed;

        if (state == State.Falling)
        {
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

        // Add movement kick.
        movementKickTimer += Time.deltaTime;
        if (!movementKickReady)
        {
            if (directionalInput == Vector2.zero && movementKickTimer >= movementKickCooldown) movementKickReady = true;
            else if (Vector2.Angle(lastKickDirection, directionalInput) > 30f) movementKickReady = true;
        }

        if (directionalInput != Vector2.zero)
        {
            if (movementKickReady)
            {
                lastKickDirection = directionalInput;
                movementKickTimer = 0f;
                movementKickReady = false;
                rigidBody.AddForce(desiredMove.normalized * movementKickForce, ForceMode.Impulse);
            }
        }

        // Clamp velocity to max speed.
        if (state == State.Falling || state == State.SpeedFalling)
        {
            //rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxFallingSpeed);
            rigidBody.velocity = new Vector3(
                Mathf.Clamp(rigidBody.velocity.x, -maxAirSpeed*0.5f, maxAirSpeed*0.5f),
                Mathf.Clamp(rigidBody.velocity.y, -maxFallingSpeed, maxFallingSpeed),
                Mathf.Clamp(rigidBody.velocity.z, -maxAirSpeed*0.5f, maxAirSpeed*0.5f)
                );
        }

        else
        {
            rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxGroundSpeed);
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

    public void AddRecoil(float forceAmt)
    {
        //transform.Find("FirstPersonCharacter").transform.DOBlendableLocalRotateBy(new Vector3(-0.1f, 0f, 0f), 0.1f, RotateMode.Fast).SetLoops(1, LoopType.Yoyo);
        foreach (ScreenShake screenShake in FindObjectsOfType<ScreenShake>()) screenShake.SetShake(forceAmt * 0.0005f, 0.1f);

        Vector3 forceVector = -transform.forward;
        forceVector.y = 0.5f;
        forceVector *= forceAmt;
        GetComponent<Rigidbody>().AddForce(forceVector, ForceMode.Impulse);
    }


    public void SetFieldOfView(float value)
    {
        float newFov = MyMath.Map(value, 1f, -1f, fieldOfViewRange.min, fieldOfViewRange.max);
        float newSize = MyMath.Map(value, 1f, -1f, orthographicSizeRange.min, orthographicSizeRange.max);

        for (int i = 0; i < perspectiveCams.Length; i++)    
            perspectiveCams[i].fieldOfView = newFov;

        for (int i = 0; i < orthographicCams.Length; i++)
            orthographicCams[i].orthographicSize = newSize;
    }
}
