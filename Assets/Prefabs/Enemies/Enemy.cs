using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    // USED FOR NAVIGATION
    protected NavMeshAgent navMeshAgent;

    // USED FOR MOVING
    [HideInInspector] public bool willAttack = false;
    [SerializeField] protected float moveRandomness = 40f;    // When I move, I move towards a spot inside a circle of this diameter surrounding the player.
    [SerializeField] protected float moveDistanceMin = 1f;  // The shortest distance I will move.
    [SerializeField] protected float moveDistanceMax = 15f; // The longest distance I will move.
    [SerializeField] protected float moveSpeed = 5f;  // How quickly I move.
    [SerializeField] protected Vector3 targetPosition; // The position I am currently moving towards.
    protected float moveTimer = 0f;

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

    // REFERENCES
    protected GameManager gameManager;
    protected Rigidbody myRigidbody;
    protected Animator myAnimator;
    protected Transform playerTransform;


    protected void Start()
    {
        // Hook up references
        navMeshAgent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        myRigidbody = GetComponent<Rigidbody>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        myAnimator = GetComponent<Animator>();
        myMaterial = GetComponentInChildren<MeshRenderer>().material;

        // Get a random starting point for Perlin noise.
        noiseTime = Random.Range(-1000f, 1000f);
    }


    void Update()
    {
        // Change material tiling using Perlin noise. (For the static effect).
        myMaterial.mainTextureScale = new Vector2(MyMath.Map(Mathf.PerlinNoise(noiseTime, 0), 0f, 1f, -noiseRange, noiseRange), 0);
        noiseTime += noiseSpeed;
    }


    //void OnCollisionEnter(Collision collision)
    //{
    //    // If I hit a non-lethal obstacle, move to a new spot (crude pathfinding).
    //    if (collision.collider.tag == "Obstacle" || collision.collider.tag == "Wall" || collision.collider.tag == "Enemy")
    //    {
    //        currentState = BehaviorState.PreparingToMove;
    //    }
    //}


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
