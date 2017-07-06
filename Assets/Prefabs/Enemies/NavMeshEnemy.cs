using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshEnemy : MonoBehaviour {

    // USED FOR NAVIGATION
    NavMeshAgent navMeshAgent;

    // USED FOR MOVING
	[SerializeField] protected float moveRandomness = 40f;    // When I move, I move towards a spot inside a circle of this diameter surrounding the player.
	[SerializeField] protected float moveDistanceMin = 1f;  // The shortest distance I will move.
	[SerializeField] protected float moveDistanceMax = 15f; // The longest distance I will move.
	[SerializeField] protected float moveSpeed = 5f;  // How quickly I move.
    protected Vector3 targetPosition; // The position I am currently moving towards.

    // USED FOR SHOOTING
	[SerializeField] protected float shotTimerMin = 0.7f;   // The minimum amount of time in between shots.
	[SerializeField] protected float shotTimerMax = 5f;   // The maximum amount of time in between shots.
	[SerializeField] protected float preShotDelay = 0.7f; // How long I pause motionless before firing a shot.
	[SerializeField] protected float postShotDelay = 0.4f;    // How long I pause motionless after firing a shot.
	[SerializeField] protected GameObject shotPrefab;
    protected Timer shotTimer;    // Keeps track of how long it's been since I last fired a shot.

    // USED FOR GETTING HURT
	[SerializeField] protected int _HP = 20; // Health points.
    public int HP
    {
        get
        {
            return _HP;
        }

        set
        {
            // See if I should die.
            if (value <= 0)
            {
                Die();
            }

            else
            {
                hurtAudio.Play();
                _HP = value;
            }
        }
    }
	[SerializeField] protected GameObject deathParticles;
    [SerializeField] protected AudioSource hurtAudio;
    protected bool isAlive = true;

    // USED FOR MATERIAL MODIFICATION
    protected Material myMaterial;
    protected float noiseTime;
    [SerializeField] protected float noiseSpeed = 0.01f;
    protected float noiseRange = 100f;

    // BEHAVIOR STATES
    protected enum BehaviorState { PreparingToMove, Moving, PreShooting, Shooting, PostShooting };
    protected BehaviorState currentState;

    // REFERENCES
    protected GameManager gameManager;
    protected Rigidbody myRigidbody;
    protected Animator myAnimator;
    protected Transform playerTransform;


    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        // Store miscellaneous references.
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        myRigidbody = GetComponent<Rigidbody>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        myAnimator = GetComponent<Animator>();
        myMaterial = GetComponentInChildren<MeshRenderer>().material;

        // Not sure why I'm doing this - look into it later. (Does it have to do with the enemies firing shots at the wrong point during their animation?)
        myAnimator.speed = 1.0f / preShotDelay;

        // Get a random starting point for Perlin noise.
        noiseTime = Random.Range(-1000f, 1000f);

        // Get a random time to fire the next shot.
        shotTimer = new Timer(Random.Range(shotTimerMin, shotTimerMax));

        currentState = BehaviorState.PreparingToMove;
    }


    void Update()
    {
        // Change material tiling using Perlin noise. (For the static effect).
        myMaterial.mainTextureScale = new Vector2(MyMath.Map(Mathf.PerlinNoise(noiseTime, 0), 0f, 1f, -noiseRange, noiseRange), 0);
        noiseTime += noiseSpeed;

        // Perform actions according to current BehaviorState
        if (currentState == BehaviorState.PreparingToMove)
        {
            PrepareToMove();
        }

        else if (currentState == BehaviorState.Moving)
        {
            Move();
        }

        else if (currentState == BehaviorState.PreShooting)
        {
            PreShoot();
        }

        else if (currentState == BehaviorState.Shooting)
        {
            Shoot();
        }

        else if (currentState == BehaviorState.PostShooting)
        {
            PostShoot();
        }
    }


    void PrepareToMove()
    {
        // Get a random point in a circle around the player.
        Vector3 nearPlayer = playerTransform.position + Random.insideUnitSphere * moveRandomness;
        nearPlayer.y = transform.position.y;

        // Get a direction to that point
        Vector3 direction = nearPlayer - transform.position;
        direction.Normalize();

        // Scale that direction to a random magnitude
        targetPosition = transform.position + direction * Random.Range(moveDistanceMin, moveDistanceMax);

        targetPosition.y = transform.position.y;

        navMeshAgent.SetDestination(targetPosition);

        currentState = BehaviorState.Moving;
    }


    void Move()
    {
        // See if it's time to shoot at the player
        shotTimer.Run();
        if (shotTimer.finished)
        {
            // Set timer for pre shot delay
            shotTimer = new Timer(preShotDelay);

            // Begin the charging up animation.
            myAnimator.SetTrigger("ChargeUp");

            currentState = BehaviorState.PreShooting;

            return;
        }

        // Move towards target position
        //Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        Vector3 newPosition = transform.position + Vector3.Normalize(navMeshAgent.desiredVelocity) * moveSpeed * Time.deltaTime;
        myRigidbody.MovePosition(newPosition);
        //myRigidbody.MovePosition(transform.position + navMeshAgent.desiredVelocity);

        // If we've reached the target position, find a new target position
        if (newPosition == targetPosition)
        {
            currentState = BehaviorState.PreparingToMove;
            return;
        }
    }


    void PreShoot()
    {
        // Do nothing and wait for charge-up animation to finish.
    }


    void Shoot()
    {
        // Fire a shot.
        GameObject newShot = Instantiate(shotPrefab, new Vector3(transform.position.x, 1.75f, transform.position.z), Quaternion.identity);
        newShot.GetComponent<EnemyShot>().firedEnemy = gameObject;

        // Set the shot timer for the post shot delay.
        shotTimer = new Timer(postShotDelay);

        currentState = BehaviorState.PostShooting;
    }


    void PostShoot()
    {
        // Se if we've waited long enough.
        shotTimer.Run();
        if (shotTimer.finished)
        {
            // Determite how long until the next bullet is fired.
            shotTimer = new Timer(Random.Range(shotTimerMin, shotTimerMax));

            currentState = BehaviorState.PreparingToMove;
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        // If I hit a non-lethal obstacle, move to a new spot (crude pathfinding).
        if (collision.collider.tag == "Obstacle" || collision.collider.tag == "Wall" || collision.collider.tag == "Enemy")
        {
            currentState = BehaviorState.PreparingToMove;
        }
    }


    void ChargeUpAnimationFinished()
    {
        currentState = BehaviorState.Shooting;
    }


    protected void Die()
    {
        // isAlive is used to make sure that this function is not called more than once.
        if (isAlive)
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
            gameManager.KilledEnemy();
            isAlive = false;
            Destroy(gameObject);
        }
    }

}
