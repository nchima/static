using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMissile : MonoBehaviour {

    /* MOVEMENT */
    [SerializeField] private float accelerationSpeed = 1f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float maxSpeed = 65f;
    private float lockOnDistance = 15f;
    private Vector3 acceleration;
    private Vector3 velocity;
    private Vector3 desiredVelocity;
    private Vector3 targetPosition;
    private Vector3 initialTargetPosition;
    Transform targetEnemy;

    bool collideWithFloor = true;

    [SerializeField] private GameObject explosionPrefab;


    private void Start()
    {
        // Orient self according to player's current rotation.
        //transform.rotation = GameManager.instance.player.transform.rotation;

        GameManager.instance.UpdateBillboards();

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 200f, 1 << 8))
        {
            initialTargetPosition = hit.point;
            targetPosition = hit.point;
        }

        transform.Rotate(new Vector3(Random.Range(-2f, -15f), Random.Range(-30f, 30f), 0f));
        //Debug.Break();
    }


    private void Update()
    {
        if (targetEnemy == null)
        {
            targetPosition = initialTargetPosition;

            if (Vector3.Distance(GameManager.instance.player.transform.position, transform.position) > lockOnDistance)
            {
                // See if I can find a target.
                Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, lockOnDistance, 1 << 14);
                if (nearbyEnemies.Length > 0)
                {
                    Collider nearestEnemy = null;
                    for (int i = 0; i < nearbyEnemies.Length; i++)
                    {
                        if (nearestEnemy == null)
                        {
                            nearestEnemy = nearbyEnemies[i];
                            break;
                        }

                        bool thisEnemyIsCloser =
                            nearestEnemy != null &&
                            Vector3.Distance(transform.position, nearbyEnemies[i].transform.position) < Vector3.Distance(transform.position, nearestEnemy.transform.position);

                        if (thisEnemyIsCloser)
                        {
                            nearestEnemy = nearbyEnemies[i];
                        }
                    }

                    targetEnemy = nearestEnemy.transform;
                    targetPosition = targetEnemy.position;
                }
            }
        }

        else
        {
            targetPosition = targetEnemy.position;
        }

        desiredVelocity = Vector3.Normalize(targetPosition - transform.position) * maxSpeed;

        Vector3 steerForce = Vector3.Normalize(desiredVelocity - velocity) * accelerationSpeed;
        if (targetEnemy != null) steerForce = Vector3.Normalize(desiredVelocity - velocity) * turnSpeed;

        //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(Vector3.Normalize(transform.position - targetPosition)), 45f);
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPosition - transform.position), 0.2f);

        acceleration += steerForce;

        velocity += acceleration;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * 0.5f + (velocity * Time.deltaTime));
    }


    void Detonate()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }


    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Obstacle" || collider.tag == "Wall" || (collider.name == "Floor" && collideWithFloor))
        {
            //Debug.Log("I bumped into an obstacle.");
            Detonate();
        }

        else if (collider.tag == "Enemy")
        {
            Detonate();
        }

        //else if (collider.tag == "Player")
        //{
        //    // Destroy self.
        //    Detonate();
        //}
    }
}
