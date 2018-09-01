using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMissile : MonoBehaviour {

    /* INSPECTOR */

    // Tweening variables.
    [SerializeField] private FloatRange initialSpeedRange = new FloatRange(1f, 20f);
    [SerializeField] private FloatRange turnSpeedRange = new FloatRange(0f, 4f);
    [SerializeField] private FloatRange lockedOnTurnSpeedMultiplierRange = new FloatRange(0f, 20f);
    [SerializeField] private FloatRange maxSpeedRange = new FloatRange(10f, 40f);
    [SerializeField] private FloatRange lifetimeDurationRange = new FloatRange(1f, 3f);
    [SerializeField] private FloatRange decelerationOverLifetimeMultiplierRange = new FloatRange(0.9f, 1f);
    [SerializeField] private FloatRange meanderNoiseRange = new FloatRange(0.1f, 0.9f);
    [SerializeField] private FloatRange visualWidthRange = new FloatRange(1f, 2f);
    [SerializeField] private FloatRange colliderWidthRange = new FloatRange(2f, 4f);
    [SerializeField] private FloatRange upwardTiltRange = new FloatRange(4f, 9.5f);
    [SerializeField] private FloatRange spreadXRange = new FloatRange(20f, 35f);
    [SerializeField] private FloatRange spreadYRange = new FloatRange(0f, 7.5f);

    // References
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject lockOnTrigger;
    [SerializeField] private GameObject sphereVisuals;


    /* PRIVATE VARIALBES */

    // State
    private enum State { NotLockedOn, LockedOn }
    private State currentState = State.NotLockedOn;

    // Firing
    private float upwardTilt;
    private float spreadX;
    private float spreadY;

    // Movement
    private float initialSpeed;
    private float turnSpeed;
    private float lockedOnTurnSpeedMultiplier;
    private float maxSpeed;
    private float decelerationOverLifetimeMultiplier;
    private Vector3 acceleration;
    private Vector3 velocity;
    private Vector3 desiredVelocity;
    private Vector3 initialDirection;
    private float rotationSpeed = 40f;

    // Locking on
    private Transform targetEnemy;
    private GameObject lockedOnEnemy;
    private Vector3 lockOnTarget;
    private Vector3 initialTargetPosition;
    private float lockOnDistance = 15f;

    // Perlin noise
    float noiseOffsetX;
    float noiseOffsetY;
    float noiseTimeX = 0f;
    float noiseTimeY = 0f;
    float meanderNoise;

    // Timer
    private float lifetimeDuration;
    private float lifetimeTimer;
    const float LOCKON_DELAY = 0.25f;
    float lockonDelayTimer = 0f;

    // References
    SkinnedMeshRenderer m_SkinnedMeshRenderer;

    // Misc
    private bool collideWithFloor = false;


    private void Awake() {
        m_SkinnedMeshRenderer = sphereVisuals.GetComponentInChildren<SkinnedMeshRenderer>();
    }


    public void GetFired() {
        // Set up variables modified by player's gun value.
        initialSpeed = GunValueManager.MapToFloatRange(initialSpeedRange);
        turnSpeed = GunValueManager.MapToFloatRange(turnSpeedRange);
        lockedOnTurnSpeedMultiplier = GunValueManager.MapToFloatRange(lockedOnTurnSpeedMultiplierRange);
        maxSpeed = GunValueManager.MapToFloatRange(maxSpeedRange);
        meanderNoise = GunValueManager.MapToFloatRange(meanderNoiseRange);
        lifetimeDuration = GunValueManager.MapToFloatRange(lifetimeDurationRange);
        upwardTilt = GunValueManager.MapToFloatRange(upwardTiltRange);
        spreadX = GunValueManager.MapToFloatRange(spreadXRange);
        spreadY = GunValueManager.MapToFloatRange(spreadYRange);
        decelerationOverLifetimeMultiplier = GunValueManager.MapToFloatRange(decelerationOverLifetimeMultiplierRange);

        // Get random rotation
        RotateMesh();

        // Adjust size of visuals
        Vector3 newVisualScale = sphereVisuals.transform.localScale;
        newVisualScale *= GunValueManager.MapToFloatRange(visualWidthRange);
        //newVisualScale.y = GunValueManager.MapToFloatRangeInverted(visualWidthRange);
        sphereVisuals.transform.localScale = newVisualScale;
        m_SkinnedMeshRenderer.SetBlendShapeWeight(0, GunValueManager.MapTo(100f, 0f));

        // Adjust size of collider
        Vector3 newColliderSize = GetComponent<BoxCollider>().size;
        newColliderSize.x = GunValueManager.MapToFloatRange(colliderWidthRange);
        newColliderSize.y = GunValueManager.MapToFloatRange(colliderWidthRange);
        GetComponent<BoxCollider>().size = newColliderSize;

        // Add this missile's circular sprite to the billboard manager.
        Services.billboardManager.FindAllBillboards();

        // Set up perlin noise
        noiseOffsetX = Random.Range(-100f, 100f);
        noiseOffsetY = Random.Range(-100f, 100f);

        // Get initial direction
        transform.rotation = Services.playerTransform.rotation;

        transform.Rotate(new Vector3(-upwardTilt + Random.Range(-spreadY, spreadY), Random.Range(-spreadX, spreadX), 0f));
        initialDirection = transform.forward;
        velocity = initialDirection * initialSpeed;
    }


    void Update() {
        // Get destroyed if lifetime is expired.
        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= lifetimeDuration) { GetDestroyed(); }

        lockonDelayTimer += Time.deltaTime;

        // Update position of lock-on trigger (always keep it at ground level and in front of missile)
        float lockOnTriggerRadius = lockOnTrigger.GetComponent<SphereCollider>().radius;
        lockOnTrigger.transform.localPosition = new Vector3(0f, 0f, lockOnTriggerRadius);
        lockOnTrigger.transform.position = new Vector3(lockOnTrigger.transform.position.x, lockOnTriggerRadius / 4f, lockOnTrigger.transform.position.z);

        // Update desired velocity direction based on current state.
        switch (currentState) {
            case State.NotLockedOn:
                MoveWithoutTarget();
                break;
            case State.LockedOn:
                SteerTowardsTarget();
                break;
        }

        // Steer towards desired velocity.
        desiredVelocity = desiredVelocity.normalized * maxSpeed;
        Vector3 steerForce = desiredVelocity - velocity;
        if (currentState == State.NotLockedOn) { steerForce = Vector3.ClampMagnitude(steerForce, turnSpeed); } 
        else { steerForce = Vector3.ClampMagnitude(steerForce, turnSpeed * lockedOnTurnSpeedMultiplier); }

        // Accelerate
        acceleration += steerForce;
        velocity += acceleration;
        velocity *= decelerationOverLifetimeMultiplier;

        // Move
        GetComponent<Rigidbody>().MovePosition(transform.position + velocity * Time.deltaTime);
        RotateMesh();

        // Update perlin noise
        noiseTimeX += Time.deltaTime;
        noiseTimeY += Time.deltaTime;
    }


    void MoveWithoutTarget() {
        // Steer towards a random direction using perlin noise.
        Vector3 tempTarget = transform.position + initialDirection;
        tempTarget.y += MyMath.Map(Mathf.PerlinNoise(noiseTimeX + noiseOffsetX, 0f), 0f, 1f, -meanderNoise, meanderNoise);
        tempTarget.x += MyMath.Map(Mathf.PerlinNoise(noiseTimeY + noiseOffsetY, 0f), 0f, 1f, -meanderNoise, meanderNoise);
        desiredVelocity = tempTarget - transform.position;

        sphereVisuals.transform.LookAt(transform.position + desiredVelocity);

        // Rotation [May need to bring this code back if I decide to use a directional model]
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(Vector3.Normalize(transform.position - (transform.position + desiredVelocity))), 360f);
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(transform.position - (transform.position + desiredVelocity)), 1f);
    }


    void SteerTowardsTarget() {
        // If this missile's target has been destroyed, switch out of unlock state.
        if (lockedOnEnemy == null) {
            currentState = State.NotLockedOn;
            return;
        }

        sphereVisuals.transform.LookAt(lockedOnEnemy.transform.position);

        desiredVelocity = lockedOnEnemy.transform.position - transform.position;
    }


    // Instantiates an explosion prefab
    void Detonate() {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }


    // Deletes game object.
    public void GetDestroyed() {
        Destroy(gameObject);
        if (GetComponentInChildren<TrailRenderer>() == null) return;
        GetComponentInChildren<TrailRenderer>().autodestruct = true;
        GetComponentInChildren<TrailRenderer>().transform.SetParent(null);
    }


    void RotateMesh() {
        Vector3 newRotation = sphereVisuals.transform.rotation.eulerAngles;
        newRotation.z = Random.Range(-180f, 180f);
        sphereVisuals.transform.rotation = Quaternion.Euler(newRotation);
    }


    void OnTriggerEnter(Collider collider) {
        if (collider.tag == "Obstacle") /*|| collider.tag == "Wall" || (collider.name == "Floor" && collideWithFloor))*/ {
            Detonate();
            GetDestroyed();
        }

        else if (collider.tag == "Enemy") {
            Detonate();
        }
    }


    void OnTriggerEnterChild(Collider collider) {
        // If an enters the lock-on trigger, lock on.
        if (collider.tag == "Enemy") {
            if (lockonDelayTimer < LOCKON_DELAY) { return; }
            if (currentState == State.NotLockedOn) {
                lockedOnEnemy = collider.gameObject;
                currentState = State.LockedOn;
            }
        }
    }


    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject == lockedOnEnemy)
    //    {
    //        lockedOnEnemy = null;
    //        currentState = State.MovingForward;
    //    }
    //}
}
