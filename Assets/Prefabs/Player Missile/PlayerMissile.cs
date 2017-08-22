using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMissile : MonoBehaviour {

    enum State { MovingForward, SteeringTowardsTarget }
    State currentState = State.MovingForward;

    /* MOVEMENT */
    [SerializeField] private float initialSpeed = 10f;
    [SerializeField] private float accelerationSpeed = 1f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float lockedOnTurnSpeedMultiplier = 5f;
    [SerializeField] private float maxSpeed = 65f;

    private Vector3 acceleration;
    private Vector3 velocity;
    private Vector3 desiredVelocity;

    Transform targetEnemy;
    private GameObject lockedOnEnemy;
    private Vector3 lockOnTarget;
    private float lockOnDistance = 15f;

    private Vector3 initialTargetPosition;
    Vector3 initialDirection;

    bool collideWithFloor = false;

    [SerializeField] private GameObject explosionPrefab;
    GameObject lockOnTriggerObject;

    [SerializeField] float stayAliveFor = 10f;
    float lifeTimer = 0f;

    float noiseOffsetX;
    float noiseOffsetY;
    float noiseTimeX = 0f;
    float noiseTimeY = 0f;

    private void Start()
    {
        // Orient self according to player's current rotation.
        //transform.rotation = GameManager.instance.player.transform.rotation;

        GameManager.instance.UpdateBillboards();

        lockOnTriggerObject = transform.Find("Lock On Trigger").gameObject;

        //RaycastHit hit;
        //if (Physics.Raycast(transform.position, transform.forward, out hit, 200f, 1 << 8))
        //{
        //    initialTargetPosition = hit.point;
        //}

        noiseOffsetX = Random.Range(-100f, 100f);
        noiseOffsetY = Random.Range(-100f, 100f);

        transform.rotation = GameManager.instance.player.transform.rotation;
        transform.Rotate(new Vector3(Random.Range(-2f, -15f), Random.Range(-20f, 20f), 0f));

        initialDirection = transform.forward;
        //initialDirection.y = 1f;
        velocity = initialDirection * initialSpeed;

    }


    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= stayAliveFor) GetDestroyed();

        // Update position of lock on trigger (always keep it at ground level and in front of missile)
        float lockOnTriggerRadius = lockOnTriggerObject.GetComponent<SphereCollider>().radius;
        lockOnTriggerObject.transform.localPosition = new Vector3(0f, 0f, lockOnTriggerRadius);
        lockOnTriggerObject.transform.position = new Vector3(lockOnTriggerObject.transform.position.x, lockOnTriggerRadius / 4f, lockOnTriggerObject.transform.position.z);
        //lockOnTriggerObject.transform.position = newLockOnTriggerPosition;

        switch (currentState)
        {
            case State.MovingForward:
                MoveForward();
                break;
            case State.SteeringTowardsTarget:
                SteerTowardsTarget();
                break;
        }

        desiredVelocity = desiredVelocity.normalized * maxSpeed;

        Vector3 steerForce = desiredVelocity - velocity;
        if (currentState == State.MovingForward) steerForce = Vector3.ClampMagnitude(steerForce, turnSpeed);
        else steerForce = Vector3.ClampMagnitude(steerForce, turnSpeed * lockedOnTurnSpeedMultiplier);

        acceleration += steerForce;

        velocity += acceleration;
        //velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        GetComponent<Rigidbody>().MovePosition(transform.position + velocity * Time.deltaTime);

        noiseTimeX += Time.deltaTime;
        noiseTimeY += Time.deltaTime;
    }


    void MoveForward()
    {
        Vector3 tempTarget = transform.position + initialDirection;
        tempTarget.y += MyMath.Map(Mathf.PerlinNoise(noiseTimeX + noiseOffsetX, 0f), 0f, 1f, -0.9f, 0.9f);
        tempTarget.x += MyMath.Map(Mathf.PerlinNoise(noiseTimeY + noiseOffsetY, 0f), 0f, 1f, -0.9f, 0.9f);
        desiredVelocity = tempTarget - transform.position;
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(Vector3.Normalize(transform.position - targetPosition)), 45f);
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 0.2f);
        return;
    }


    void SteerTowardsTarget()
    {
        // See if my target has been destroyed.
        if (lockedOnEnemy == null)
        {
            currentState = State.MovingForward;
            return;
        }

        desiredVelocity = lockedOnEnemy.transform.position - transform.position;
        //if (targetEnemy != null) steerForce = Vector3.Normalize(desiredVelocity - velocity) * turnSpeed;
    }


    void Detonate()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        //Destroy(gameObject);
    }


    void GetDestroyed()
    {
        Destroy(gameObject);
        if (GetComponentInChildren<TrailRenderer>() == null) return;
        GetComponentInChildren<TrailRenderer>().autodestruct = true;
        GetComponentInChildren<TrailRenderer>().transform.SetParent(null);
    }


    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Obstacle") /*|| collider.tag == "Wall" || (collider.name == "Floor" && collideWithFloor))*/
        {
            //Debug.Log("I bumped into an obstacle.");
            Detonate();
            GetDestroyed();
        }

        else if (collider.tag == "Enemy")
        {
            Detonate();
        }
    }


    void OnTriggerEnterChild(Collider collider)
    {
        if (collider.tag == "Enemy")
        {
            if (currentState == State.MovingForward)
            {
                lockedOnEnemy = collider.gameObject;
                currentState = State.SteeringTowardsTarget;
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
