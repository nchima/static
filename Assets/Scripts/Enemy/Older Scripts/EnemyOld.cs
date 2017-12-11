using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class EnemyOld : MonoBehaviour
{
    // USED FOR NAVIGATION
    protected NavMeshAgent navMeshAgent;

    // USED FOR MOVING
    [HideInInspector] public bool willAttack = false;
    protected bool willMove = true;
    [SerializeField] bool isFlyingEnemy;
    public bool canSeePlayer {
        get {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, playerTransform.position - transform.position, out hit, 200f, 1 << 8 | 1 << 16)) {
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
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f) && hit.collider.name == "Floor")
            {
                return true;
            }

            else
            {
                //Debug.Log("Ground check returned false. ");
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
    [SerializeField] protected int _HP = 20; // Max health points (set in inspector)
    [HideInInspector] public int maxHP;  // Max health (is a different variable from the one set in the inspector for ....... a reason?
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
                //foreach (MeshRenderer mr in myGeometry.GetComponentsInChildren<MeshRenderer>())
                //{
                    //mr.material.color = Color.red;
                    //mr.material.SetColor("_EmissionColor", Color.red);

                    //mr.material.DOColor(Color.Lerp(baseColor, Color.red, MyMath.Map(HP, maxHP, 0, 0f, 1f)), 1.25f);
                    //mr.material.DOColor(originalEmissionColor, "_EmissionColor", 0.75f);
                //}

                hurtColorCurrent = hurtColorMax;
                hurtColorLerp = 1f;
                hurtPulseRate = MyMath.Map((float)HP, 0f, (float)maxHP, 5f, MAX_HURT_PULSE_RATE);

                vertexJitterRate += 1f;

                hurtAudio.Play();

                _HP = value;
            }
        }
    }
    float hurtPulseTime = 0f;
    float hurtPulseRate = 0f;
    const float MAX_HURT_PULSE_RATE = 10f;
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

    float colorLerp = 0f;
    protected Color originalColor;
    protected Color originalEmissionColor;
    protected Color currentColor;
    protected Color currentEmissionColor;
    protected Color attackingColorMax = new Color(0.5f, 1f, 1f, 0.9f);
    protected Color attackingColorCurrent;
    protected Color hurtColorMax = new Color(1f, 0f, 0f, 1f);
    protected Color hurtColorCurrent;
    float hurtColorLerp = 0f;
    float attackingColorLerp = 0f;

    protected float noiseTime;
    [SerializeField] protected float noiseSpeed = 0.01f;
    protected float noiseRange = 100f;

    // USED FOR SCORE CALCULATION
    public int killValue;  // How many points the player gets for killing this enemy.
    //[SerializeField] int timeBonusValue;    // How many points this enemy adds to the max time bonus value.
    public float bonusTimeAdded;    // How much time this enemy adds to the bonus timer at level generation.

    // DROPPING SPECIAL MOVE AMMO
    [SerializeField] GameObject specialMoveAmmoPrefab;
    [SerializeField] int specialMoveAmmoToDrop = 3;
    [SerializeField] float specialValue = 0.3f;

    // REFERENCES
    protected GameManager gameManager;
    protected Rigidbody myRigidbody;
    protected Animator myAnimator;
    protected Transform playerTransform;


    protected virtual void Start()
    {
        // Hook up references
        navMeshAgent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        myRigidbody = GetComponent<Rigidbody>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        myAnimator = GetComponent<Animator>();
        myMaterial = GetComponentInChildren<MeshRenderer>().material;

        GetComponent<EnemyPropertyBlockControl>()._renderers = myGeometry.GetComponentsInChildren<Renderer>();

        // Remember my starting color.
        originalColor = Color.black;
        originalEmissionColor = Color.black;
        currentColor = originalColor;
        currentEmissionColor = originalEmissionColor;
        attackingColorCurrent = originalColor;
        hurtColorCurrent = originalColor;
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


    protected virtual void Update()
    {
        // Change material tiling using Perlin noise. (For the static effect).
        myMaterial.mainTextureScale = new Vector2(MyMath.Map(Mathf.PerlinNoise(noiseTime, 0), 0f, 1f, -noiseRange, noiseRange), 0);
        noiseTime += noiseSpeed;

        hurtColorLerp -= 0.01f * Time.deltaTime;
        hurtColorLerp = Mathf.Clamp01(hurtColorLerp);
        hurtColorCurrent = Color.Lerp(hurtColorCurrent, originalColor, hurtColorLerp);
        //attackingColorLerp -= 0.01f * Time.deltaTime;
        //attackingColorLerp = Mathf.Clamp01(attackingColorLerp);
        //attackingColorCurrent = Color.Lerp(attackingColorCurrent, originalColor, attackingColorLerp);

        if (hurtPulseRate > 0)
        {
            hurtPulseTime += hurtPulseRate * Time.deltaTime;
            if (Mathf.Sin(hurtPulseTime) > 0.75f || (Mathf.Sin(hurtPulseTime) > 1f && Mathf.Sin(hurtPulseTime) < 1.25f)) hurtColorCurrent = hurtColorMax;
            else hurtColorCurrent = originalColor;
            //hurtColorCurrent = Color.Lerp(originalColor, hurtColorMax, MyMath.Map(Mathf.Sin(hurtPulseTime), -1f, 1f, 0f, 1f));
        }

        currentColor = attackingColorCurrent + hurtColorCurrent;
        currentEmissionColor = attackingColorCurrent + hurtColorCurrent;

        GetComponent<EnemyPropertyBlockControl>().MainColor = currentColor;
        GetComponent<EnemyPropertyBlockControl>().EmissionColor = currentEmissionColor;

        //foreach (MeshRenderer mr in myGeometry.GetComponentsInChildren<MeshRenderer>())
        //{
        //    if (mr.transform.childCount > 0)
        //    {
        //        foreach (MeshRenderer mrs in mr.GetComponentsInChildren<MeshRenderer>())
        //        {
        //            SetColor(mrs);
        //        }
        //    }

        //    SetColor(mr);
        //}

        if (isPhysicsObject)
        {
            //Debug.Log(gameObject.name + " is physics object.");

            physicsObjectTimer -= Time.deltaTime;
            if (physicsObjectTimer <= 0f && (isFlyingEnemy || isOnGround))
            {
                //Debug.Log(gameObject.name + " returning to kinematic.");
                ReturnToKinematic();
            }
        }

        //vertexJitterRate -= 0.5f;
        //vertexJitterRate = Mathf.Clamp(vertexJitterRate, 0f, 20f);
        //if (vertexJitterRate > 0f) JitterVertices();
    }


    void SetColor(MeshRenderer mr)
    {
        mr.material.color = currentColor;
        mr.material.SetColor("_EmissionColor", currentEmissionColor);
    }


    void JitterVertices() {
        MeshFilter[] myMeshFilters = myGeometry.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < myMeshFilters.Length; i++) {
            //myMeshRenderers[i].material.SetFloat("_VertexJitterX", Random.Range(-1f, 1f) * vertexJitterRate * Time.deltaTime);
            //myMeshRenderers[i].material.SetFloat("_VertexJitterY", Random.Range(-1f, 1f) * vertexJitterRate * Time.deltaTime);
            //myMeshRenderers[i].material.SetFloat("_VertexJitterZ", Random.Range(-1f, 1f) * vertexJitterRate * Time.deltaTime);

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


    protected virtual void Die() {
        // isAlive is used to make sure that this function is not called more than once.
        if (!isAlive) { return; }

        //DropSpecialMoveAmmo();

        Instantiate(deathParticles, transform.position, Quaternion.identity);
        gameManager.PlayerKilledEnemy(killValue, specialValue);
        isAlive = false;
        Destroy(gameObject);
    }


    void DropSpecialMoveAmmo() {
        for (int i = 0; i < specialMoveAmmoToDrop; i++) {
            GameObject droppedAmmo = Instantiate(specialMoveAmmoPrefab);
            droppedAmmo.transform.position = transform.position + Random.insideUnitSphere;
            Vector3 forceDirection = Vector3.Normalize(Random.insideUnitSphere);
            droppedAmmo.GetComponent<Rigidbody>().AddForce(forceDirection * 1f, ForceMode.Impulse);
        }
    }


    public virtual void BecomePhysicsObject(float duration)
    {
        if (immovable) return;

        if (navMeshAgent != null) navMeshAgent.enabled = false;
        willMove = false;
        GetComponent<Rigidbody>().isKinematic = false;
        physicsObjectTimer = duration;
        isPhysicsObject = true;
    }


    protected void ReturnToKinematic()
    {
        if (navMeshAgent != null) navMeshAgent.enabled = true;
        willMove = true;
        GetComponent<Rigidbody>().isKinematic = true;
        isPhysicsObject = false;
    }


    private void OnTriggerEnter(Collider other) {
        if (other.name.ToLower().Contains("player fall catcher")) {
            Die();
        }
    }
}
