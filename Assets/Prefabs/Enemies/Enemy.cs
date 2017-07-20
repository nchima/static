using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class Enemy : MonoBehaviour
{
    // USED FOR NAVIGATION
    protected NavMeshAgent navMeshAgent;

    // USED FOR MOVING
    [HideInInspector] public bool willAttack = false;
    protected bool willMove = true;
    protected bool canSeePlayer
    {
        get
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, playerTransform.position - transform.position, out hit, 200f, 1 << 8 | 1 << 16))
            {
                //Debug.Log("Enemy saw " + hit.transform.name);
                if (hit.transform == playerTransform) return true;
            }

            return false;
        }
    }
    protected bool isOnGround
    {
        get
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 4f) && hit.collider.name == "Floor")
            {
                return true;
            }

            else
            {
                return false;
            }
        }
    }
    [SerializeField] protected float moveRandomness = 40f;    // When I move, I move towards a spot inside a circle of this diameter surrounding the player.
    [SerializeField] protected float moveDistanceMin = 1f;  // The shortest distance I will move.
    [SerializeField] protected float moveDistanceMax = 15f; // The longest distance I will move.
    [SerializeField] protected float moveSpeed = 5f;  // How quickly I move.
    [SerializeField] protected Vector3 targetPosition; // The position I am currently moving towards.
    protected float moveTimer = 0f;

    // USED FOR GETTING HURT
    protected bool immovable = false; // Whether I can currently be turned into a physics object.
    bool isPhysicsObject = false;
    float physicsObjectTimer = 0f;
    [SerializeField] protected int _HP = 20; // Health points.
    float colorLerp = 0f;
    protected Color baseColor;
    protected Color baseEmissionColor;
    int maxHP;
    public virtual int HP
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
                foreach (MeshRenderer mr in myGeometry.GetComponentsInChildren<MeshRenderer>())
                {
                    //mr.material.color = Color.red;
                    mr.material.SetColor("_EmissionColor", Color.red);

                    //mr.material.DOColor(Color.Lerp(baseColor, Color.red, MyMath.Map(HP, maxHP, 0, 0f, 1f)), 1.25f);
                    mr.material.DOColor(baseEmissionColor, "_EmissionColor", 0.75f);
                }

                vertexJitterRate += 1f;

                hurtAudio.Play();

                _HP = value;
            }
        }
    }
    [SerializeField] protected GameObject deathParticles;
    [SerializeField] protected AudioSource hurtAudio;
    protected bool isAlive = true;
    List<Vector3[]> originalVertices;
    [HideInInspector] public float vertexJitterRate = 0f;

    // AUDIO
    [SerializeField] protected AudioSource humAudio;


    // USED FOR MATERIAL MODIFICATION
    protected Material myMaterial;
    [SerializeField] protected GameObject myGeometry;
    protected Color originalEmissionColor;
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

        // Remember my starting color.
        baseColor = myGeometry.GetComponentInChildren<MeshRenderer>().material.color;
        baseEmissionColor = myGeometry.GetComponentInChildren<MeshRenderer>().material.GetColor("_EmissionColor");
        originalEmissionColor = baseEmissionColor;
        //Debug.Log(baseColor);

        // Remember max hp.
        maxHP = HP;

        // Remember original vertex locations.
        originalVertices = new List<Vector3[]>();
        MeshFilter[] myMeshFilters = myGeometry.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < myMeshFilters.Length; i++)
        {
            originalVertices.Add(myMeshFilters[i].mesh.vertices);
        }

        // Get a random starting point for Perlin noise.
        noiseTime = Random.Range(-1000f, 1000f);
    }


    protected void Update()
    {
        // Change material tiling using Perlin noise. (For the static effect).
        myMaterial.mainTextureScale = new Vector2(MyMath.Map(Mathf.PerlinNoise(noiseTime, 0), 0f, 1f, -noiseRange, noiseRange), 0);
        noiseTime += noiseSpeed;

        foreach(MeshRenderer mr in myGeometry.GetComponentsInChildren<MeshRenderer>()) mr.material.SetColor("_EmissionColor", baseEmissionColor);

        if (isPhysicsObject)
        {
            physicsObjectTimer -= Time.deltaTime;
            if (physicsObjectTimer <= 0f && isOnGround)
            {
                ReturnToKinematic();
            }
        }

        vertexJitterRate -= 0.5f;
        vertexJitterRate = Mathf.Clamp(vertexJitterRate, 0.1f, 20f);
        if (vertexJitterRate > 0f) JitterVertices();
    }


    void JitterVertices()
    {
        MeshFilter[] myMeshFilters = myGeometry.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < myMeshFilters.Length; i++)
        {
            Vector3[] myVertices = myMeshFilters[i].mesh.vertices;
            for (int j = 0; j < myVertices.Length; j++)
            {
                myVertices[j] = originalVertices[i][j] + Random.insideUnitSphere * vertexJitterRate * Time.deltaTime;
            }

            myMeshFilters[i].mesh.vertices = myVertices;
            myMeshFilters[i].mesh.RecalculateBounds();
        }
    }


    //void OnCollisionEnter(Collision collision)
    //{
    //    // If I hit a non-lethal obstacle, move to a new spot (crude pathfinding).
    //    if (collision.collider.tag == "Obstacle" || collision.collider.tag == "Wall" || collision.collider.tag == "Enemy")
    //    {
    //        currentState = BehaviorState.PreparingToMove;
    //    }
    //}


    protected virtual void Die()
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


    public void BecomePhysicsObject(float duration)
    {
        if (immovable) return;

        navMeshAgent.enabled = false;
        willMove = false;
        GetComponent<Rigidbody>().isKinematic = false;
        physicsObjectTimer = duration;
        isPhysicsObject = true;
    }


    void ReturnToKinematic()
    {
        navMeshAgent.enabled = true;
        willMove = true;
        GetComponent<Rigidbody>().isKinematic = true;
        isPhysicsObject = false;
    }
}
