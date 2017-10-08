using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour {

    // MOVEMENT
    [SerializeField] float accelerationSpeedGround;
    [SerializeField] float accelerationSpeedFalling;
    [SerializeField] float minSpeed;
    public float maxGroundSpeed;
    public float maxAirSpeed;
    [SerializeField] float maxFallingSpeed;
    private Vector3 velocity;

    // FOV
    FloatRange fieldOfViewRange = new FloatRange(58f, 85f);
    FloatRange orthographicSizeRange = new FloatRange(23f, 40f);
    [SerializeField] Camera[] perspectiveCams;
    [SerializeField] Camera[] orthographicCams;

    // TURNING
    [SerializeField] float mouseSensitivity;
    private Quaternion targetRotation;

    // STATE
    public enum State { Normal, ShotgunCharge, Falling }
    public State state;

    // MISC
    Rigidbody rigidBody;


    private void Start()
    {
        targetRotation = transform.localRotation;
        rigidBody = GetComponent<Rigidbody>();
    }


    private void Update()
    {
        /* HANDLE VIEW ROTATION */

        float rotation = Input.GetAxis("Mouse X") * mouseSensitivity;
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
            _accelerationSpeed = accelerationSpeedFalling;
            _maxSpeed = maxAirSpeed;
        }

        // Get input.
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input.sqrMagnitude > 1) input.Normalize();

        // Get desired movement direction.
        Vector3 desiredMove = desiredMove = transform.forward * input.y + transform.right * input.x;
        desiredMove.Normalize();

        // Apply movement force to rigidbody.
        if (desiredMove.magnitude != 0)
            rigidBody.AddForce(desiredMove * _accelerationSpeed, ForceMode.VelocityChange);

        // Add movement kick if player is pressing a direction and the controller is not moving at it's minimum speed.
        if (input.magnitude != 0 && rigidBody.velocity.magnitude < minSpeed) rigidBody.velocity = desiredMove.normalized * minSpeed;

        rigidBody.velocity = new Vector3(
                    Mathf.Clamp(rigidBody.velocity.x, -_maxSpeed, _maxSpeed),
                    Mathf.Clamp(rigidBody.velocity.y, -maxFallingSpeed, maxFallingSpeed),
                    Mathf.Clamp(rigidBody.velocity.z, -_maxSpeed, _maxSpeed)
                );
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
