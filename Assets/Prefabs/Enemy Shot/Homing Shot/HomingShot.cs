using UnityEngine;
using System.Collections;

public class HomingShot : EnemyShot {

    enum HomingShotState { NotLockedOn, LockedOn, Dormant, WasShot };
    HomingShotState state;

    // MOVEMENT
    private Vector3 currentTarget;  // The initial point that I travel towards.
    private float inaccuracy = 35f;  // I will be fired towards a random point within this distance from the player.
    private float playerLockOnDistance = 50f;   // If the player is within this distance of me, I will lock onto them and start to follow.
    private float playerLockOffDistance = 75f;  // If the player moves out of this range while I am following them, I will stop following them.
    private float leading = 25f;
    [SerializeField] private float minSpeed = 1f;
	[SerializeField] private float maxSpeed = 2f;  // The maximum speed at which I travel.
    [SerializeField] private float turnSpeed = 0.5f;  // The speed at which I accelerate towards my target.

    private Vector3 acceleration = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    private Vector3 desiredVelocity = Vector3.zero;

    [SerializeField] bool canBeShotDown = false;

    // VISUALS
    private Vector3 originalScale;
    [SerializeField] float scaleMin = 0.5f;
    [SerializeField] float scaleMax = 1.5f;

    [SerializeField] Color colorMin = Color.red;
    [SerializeField] Color colorMax = Color.yellow;

    // AUDIO
    [SerializeField] AudioSource blipAudioSource;

    // EXPLOSION
    [SerializeField] GameObject explosionPrefab;

    private Transform playerTransform;


	new void Start ()
    {
        base.Start();

        state = HomingShotState.NotLockedOn;

        gameManager.UpdateBillboards();
        playerTransform = GameObject.Find("FPSController").transform;

        // Get an initial target position on the floor within a certain distance from the player.
        //Debug.Log(gameManager.playerVelocity);

        bool foundTarget = false;

        while (!foundTarget)
        {
            currentTarget = new Vector3(
                (playerTransform.position.x + GameManager.instance.playerVelocity.x * leading) + Random.insideUnitCircle.x * inaccuracy * 2f,
                0f,
                (playerTransform.position.z + GameManager.instance.playerVelocity.z * leading) + Random.insideUnitCircle.y * inaccuracy * 2f);

            leading -= 1f;
            leading = Mathf.Clamp(leading, 0f, 99f);

            if (GameManager.instance.PositionIsInLevelBoundaries(currentTarget))
            {
                foundTarget = true;
                break;
            }
        }

        //velocity = Vector3.Normalize(playerTransform.position - transform.position) * minSpeed;
        velocity = new Vector3(0f, 30f, 0f);
        desiredVelocity = velocity.normalized * maxSpeed;

        originalScale = transform.Find("Inner Sphere").localScale;
	}


    new void Update()
    {
        base.Update();

        /* MOVEMENT */

        switch (state)
        {
            case HomingShotState.NotLockedOn:
                NotLockedOn();
                break;
            case HomingShotState.LockedOn:
                LockedOn();
                break;
            case HomingShotState.Dormant:
                Dormant();
                break;
            default:
                break;
        }

        /* VISUALS */

        // Set scale based on velocity.
        float scaleScalar = MyMath.Map(Vector3.Angle(velocity, desiredVelocity), 0f, 180f, scaleMax, scaleMin);
        Vector3 newScale = originalScale + (Random.insideUnitSphere * scaleScalar);
        transform.Find("Inner Sphere").localScale = newScale;
        //Debug.Log(newScale);

        // Set color based on velocity.
        //Color newColor = Color.Lerp(colorMin, colorMax, MyMath.Map(velocity.magnitude, minSpeed, maxSpeed, 0f, 1f));
        //GetComponent<MeshRenderer>().material.color = newColor;
    }


    void NotLockedOn()
    {
        // See if the player is near me.
        if (Vector3.Distance(transform.position, playerTransform.position) <= playerLockOnDistance)
        {
            //Debug.Log("Homing shot locking onto player.");
            //velocity = Vector3.zero;
            currentTarget = playerTransform.position;
            state = HomingShotState.LockedOn;
            return;
        }


        // See if I should go dormant
        if (transform.position.y < 1f)
        {
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Rigidbody>().useGravity = true;
            state = HomingShotState.Dormant;
        }

        // Lock on when I'm at the peak of my arc.
        //if (velocity.y <= 0f)
        //{
        //    Debug.Log("Homing shot locking onto player.");
        //    currentTarget = playerTransform.position;
        //    state = HomingShotState.LockedOn;
        //}

        MoveToTarget();
    }


    void LockedOn()
    {
        // See if the player has gone out of range.
        if (Vector3.Distance(transform.position, playerTransform.position) >= playerLockOffDistance)
        {
            // Get a target position straight ahead and just move towards it.
            //RaycastHit hit;
            //if (Physics.Raycast(transform.position, transform.forward, out hit, 200f))
            //{
            //    currentTarget = hit.point;
            //}

            //// If the raycast didn't hit anything, just move toward the floor.
            //else
            //{
            //    currentTarget = new Vector3(transform.position.x, 0f, transform.position.z);
            //}

            Debug.Log("Homing shot lost target.");
            currentTarget = gameManager.player.transform.position;
            state = HomingShotState.NotLockedOn;
            return;
        }

        currentTarget = gameManager.player.transform.position;
            //+ gameManager.playerVelocity * Vector3.Distance(gameManager.player.transform.position, transform.position);

        MoveToTarget();

        blipAudioSource.pitch = MyMath.Map(Vector3.Distance(playerTransform.position, transform.position), 15f, 60f, 1f, 0.3f);
    }


    void Dormant()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) <= playerLockOnDistance)
        {
            Debug.Log("Dormant homing shot locking onto player.");
            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().useGravity = false;
            velocity = Vector3.up*5f;
            currentTarget = playerTransform.position;
            state = HomingShotState.LockedOn;
            return;
        }
    }


    void MoveToTarget()
    {
        acceleration = Vector3.zero;

        desiredVelocity = Vector3.Normalize(currentTarget - transform.position) * maxSpeed;
        Vector3 steerForce = Vector3.Normalize(desiredVelocity - velocity) * turnSpeed;

        acceleration += steerForce * Time.deltaTime;

        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;
    }


    public void GotShot(Vector3 forcePoint)
    {
        if (!canBeShotDown) return;

        Debug.Log("Got shot");
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<Rigidbody>().AddExplosionForce(7f, forcePoint, 2f, 1f,  ForceMode.Impulse);
        blipAudioSource.pitch = 3f;
        Invoke("Detonate", 4f);
        state = HomingShotState.WasShot;
    }


    public override void Detonate()
    {
        if (state != HomingShotState.LockedOn && state != HomingShotState.WasShot) return;

        base.Detonate();

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }
}
