using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShot : MonoBehaviour {

    protected float maxLifetime = 30f;   // How long I live before I am deleted.
    protected float currentLifetime;  // Used to track how long I have currently been alive.

    [SerializeField] bool collideWithFloor = false;

    [SerializeField] protected GameObject strikeParticles;    // The particles that spawn when I hit an enemy.

    public GameObject firedEnemy;    // The enemy which fired this bullet.

    protected GameManager gameManager;


    public void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }


    public void Update()
    {
        // See if I have lived long enough and should be deleted.
        if (currentLifetime < maxLifetime)
        {
            currentLifetime += Time.deltaTime;
        }

        else
        {
            Destroy(gameObject);
        }
    }


    public virtual void Detonate()
    {
        // Destroy self.
        Instantiate(strikeParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }


    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Obstacle" || collider.tag == "Wall" || (collider.name.ToLower().Contains("floor") && collideWithFloor))
        {
            //Debug.Log("I bumped into an obstacle.");
            Detonate();
        }

        else if (collider.tag.Contains("Enem") && collider.gameObject != firedEnemy)
        {
            if (collider.GetComponent<EnemyOld>()) { collider.GetComponent<EnemyOld>().HP -= Random.Range(5, 15); }
            else { collider.GetComponent<Enemy>().currentHealth -= Random.Range(5, 15); }
            Detonate();
        }

        else if (collider.tag == "Player")
        {
            gameManager.PlayerWasHurt();

            // Destroy self.
            Detonate();
        }
    }
}