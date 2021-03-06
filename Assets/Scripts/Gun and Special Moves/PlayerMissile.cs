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
    [SerializeField] private GameObject markerPrefab;


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
    private Vector3 landingPosition;

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


    public void Fire() {

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
        lifetimeTimer = 0f;

        // Get random rotation
        RotateMesh();

        // Adjust size of visuals
        sphereVisuals.transform.localScale = Vector3.one;
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
        RaycastHit hit;
        int layerMask = (1 << 8) | (1 << 14) | (1 << 20) | (1 << 23);
        if (Physics.Raycast(Services.gun.tip.position, Services.gun.tip.forward, out hit, 500f, layerMask)) {
            initialDirection = Vector3.Normalize(hit.point - Services.gun.tip.position);
            transform.forward = initialDirection;
            Debug.DrawLine(transform.position, hit.point, Color.red, 1f);
        } else {
            initialDirection = transform.forward;
        }

        transform.Rotate(new Vector3(-upwardTilt + Random.Range(-spreadY, spreadY), Random.Range(-spreadX, spreadX), 0f));
        velocity = initialDirection * initialSpeed;

        // If the special move mode is the one where you fire missiles while falling, add a marker to the ground where we are destined to land.
        if (Services.specialMoveManager.specialMoveMode == SpecialMoveManager.SpecialMoveMode.ActivateFallingSequence) {
            // landingPosition = transform.position;
            // landingPosition.y = 0.5f;

            // GameObject marker = Instantiate(markerPrefab);
            // marker.transform.position = new Vector3(landingPosition.x, 0.5f, landingPosition.z);

            // transform.position = landingPosition;
            // Detonate();
        }

        currentState = State.NotLockedOn;
    }


    void Update() {
        // Get destroyed if lifetime is expired.
        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= lifetimeDuration) { Destroy(); }

        lockonDelayTimer += Time.deltaTime;

        // switch (Services.specialMoveManager.specialMoveMode) {
        //     case SpecialMoveManager.SpecialMoveMode.FireWhileOnGround:
                // Update position of lock-on trigger (always keep it at ground level and in front of missile)
                float lockOnTriggerRadius = lockOnTrigger.GetComponent<SphereCollider>().radius;
                lockOnTrigger.transform.localPosition = new Vector3(0f, 0f, lockOnTriggerRadius);
                lockOnTrigger.transform.position = new Vector3(lockOnTrigger.transform.position.x, lockOnTriggerRadius / 4f, lockOnTrigger.transform.position.z);
                // break;

            // case SpecialMoveManager.SpecialMoveMode.ActivateFallingSequence:
                // lockOnTrigger.transform.position = landingPosition;
                // break;
        // }

        // Update desired velocity direction based on current state.
        // switch (currentState) {
        //     case State.NotLockedOn:
                MoveWithoutTarget();
        //         break;
        //     case State.LockedOn:
                // SteerTowardsTarget();
                // break;
        // }

        // Steer towards desired velocity.
        desiredVelocity = desiredVelocity.normalized * maxSpeed;

        Vector3 steerForce = desiredVelocity - velocity;
        Debug.DrawRay(transform.position, steerForce, Color.green);
        if (currentState == State.NotLockedOn) { steerForce = steerForce.normalized * turnSpeed; }
        else { steerForce = steerForce.normalized * turnSpeed * lockedOnTurnSpeedMultiplier; }

        // Accelerate
        Vector3 acceleration = Vector3.zero;
        acceleration += steerForce;

        velocity += acceleration;
        velocity *= decelerationOverLifetimeMultiplier;

        // Move
        GetComponent<Rigidbody>().velocity = velocity;
        RotateMesh();

        // Update perlin noise
        noiseTimeX += Time.deltaTime;
        noiseTimeY += Time.deltaTime;
    }


    void MoveWithoutTarget() {
        // Steer towards a random direction using perlin noise.
        Vector3 tempTarget = transform.position + initialDirection;

        // if (meanderNoise > 0) {
        //     tempTarget.y += MyMath.Map(Mathf.PerlinNoise(noiseTimeX + noiseOffsetX, 0f), 0f, 1f, -meanderNoise, meanderNoise);
        //     tempTarget.x += MyMath.Map(Mathf.PerlinNoise(noiseTimeY + noiseOffsetY, 0f), 0f, 1f, -meanderNoise, meanderNoise);
        // }

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
    public void Destroy() {
        if (GetComponentInChildren<TrailRenderer>() != null) {
            GameObject trail = Instantiate(GetComponentInChildren<TrailRenderer>().gameObject);
            trail.GetComponent<TrailRenderer>().autodestruct = true;
            trail.GetComponent<TrailRenderer>().transform.SetParent(null);
        }

        GetComponent<PooledObject>().ReturnToPool();
    }


    void RotateMesh() {
        Vector3 newRotation = sphereVisuals.transform.rotation.eulerAngles;
        newRotation.z = Random.Range(-180f, 180f);
        sphereVisuals.transform.rotation = Quaternion.Euler(newRotation);
    }


    void OnCollisionEnter(Collision collision) {
        Collider collider = collision.collider;
        if (collider.tag == "Obstacle" || LayerMask.LayerToName(collider.gameObject.layer).Contains("Floor") || collider.tag == "Wall") /* || (collider.name == "Floor" && collideWithFloor))*/ {
            Detonate();
            Destroy();
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
