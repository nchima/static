using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {

    [SerializeField] float maxHoverOffset = 1f;
    [SerializeField] float hoverSpeed = 1f;

    [SerializeField] protected float stayDuration = 6f; // How long this pickup stays in the world before disappearing.

    [SerializeField] protected GameObject pickupTrigger;
    [SerializeField] Transform pickupTrailsParent;
    [SerializeField] Renderer[] flashingRenderers;

    Transform[] pickupTrails;

    protected float kickForce = 300f;   // How fast this pickup begins moving toward the player.
    protected float moveForce = 500f;   // How quickly this pickup accelerates toward the player.

    float sineTimer;
    float sineOffset;

    float stayTimer = 0f;

    enum State { Hover, MoveToPlayer }
    State state = State.Hover;

    Vector3 directionToPlayer { get { return Vector3.Normalize(Services.playerTransform.position - transform.position); } }


    private void Awake() {
        sineOffset = Random.Range(-1000f, 1000f);
        pickupTrails = pickupTrailsParent.GetComponentsInChildren<Transform>();
        foreach (Transform trail in pickupTrails) { trail.gameObject.SetActive(false); }
    }


    private void Update() {
        if (state == State.Hover) {
            // Hovering...

            // Move up to um, whatever height is good for hovering at I guess.
            float hoverHeight = 4f;
            hoverHeight += MyMath.Map(Mathf.Sin(sineTimer + sineOffset), -1f, 1f, -maxHoverOffset, maxHoverOffset);
            transform.position = new Vector3(transform.position.x, hoverHeight, transform.position.z);

            sineTimer += hoverSpeed * Time.deltaTime;

            // Keep pickup trigger at ground level.
            pickupTrigger.transform.position = new Vector3(pickupTrigger.transform.position.x, 0f, pickupTrigger.transform.position.z);

            // Delete self if I stay for too long
            stayTimer += Time.deltaTime;
            if (stayTimer >= stayDuration) { Destroy(gameObject); }
        }
        
        else if (state == State.MoveToPlayer) {
            // Move towards the player or whatever.
            GetComponent<Rigidbody>().AddForce(directionToPlayer * moveForce * Time.deltaTime, ForceMode.Acceleration);
        }
    }

    protected virtual void GetAbsorbed() {
        // Show some particles or something, IDK, play some sound whatever fine.
        Destroy(gameObject);
    }

    public virtual void BeginMovingTowardsPlayer() {
        //GetComponent<Rigidbody>().AddForce(directionToPlayer * kickForce, ForceMode.Impulse);
        //foreach(Renderer renderer in flashingRenderers) { renderer.gameObject.layer = LayerMask.NameToLayer("Enemy Sheathe"); }
        GameEventManager.instance.FireEvent(new GameEvents.PickupObtained());
        foreach(Transform trail in pickupTrails) {
            trail.gameObject.SetActive(true);
            if (trail.GetComponent<PickupTrail>() != null) { trail.GetComponent<PickupTrail>().BeginMovingTowardsPlayer(); }
        }
        Destroy(gameObject);
        state = State.MoveToPlayer;
    }

    public void Delete() {
        Destroy(gameObject);
    }

    public void OnTriggerStayChild(Collider other) {
        // If the player crossed into my radius, start moving towards them.
        if (state == State.Hover && other.gameObject == Services.playerGameObject) {
            BeginMovingTowardsPlayer();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        // If I touch the player, get absorbed or whatever.
        if (collision.collider.gameObject == Services.playerGameObject) {
            GetAbsorbed();
        }
    }
}
