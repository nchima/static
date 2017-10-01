using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour {

    // MOVEMENT
    [SerializeField] float accelerationSpeed;
    [SerializeField] float deceleration;
    [SerializeField] float minSpeed;
    public float maxSpeed;
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

        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input.sqrMagnitude > 1) input.Normalize();

        Vector3 desiredMove = desiredMove = transform.forward * input.y + transform.right * input.x;
        desiredMove.Normalize();

        // Add movement kick.
        if (rigidBody.velocity.magnitude < minSpeed && input.magnitude != 0) rigidBody.velocity = desiredMove * minSpeed;

        // Apply movement force to rigidbody.
        rigidBody.AddForce(desiredMove * accelerationSpeed, ForceMode.Acceleration);

        // Limit speed if player is not currently falling.
        if (state != State.Falling)
        {
            //Debug.Log("Limiting player velocity.");
            rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, maxSpeed);
        }

        //Vector3 acceleration = desiredMove * accelerationSpeed;

        //if (acceleration == Vector3.zero) velocity *= deceleration * Time.deltaTime;
        //else velocity += acceleration * Time.deltaTime;

        //velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        //if (velocity.magnitude < minSpeed && input.magnitude != 0) velocity = velocity.normalized * minSpeed;

        //GetComponent<Rigidbody>().MovePosition(transform.position + velocity * Time.deltaTime);
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
