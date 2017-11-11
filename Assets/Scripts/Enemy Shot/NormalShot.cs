using UnityEngine;
using System.Collections;

public class NormalShot : EnemyShot {

	[SerializeField] private float speed = 2f;  // The speed at which I travel.
    public Vector3 direction;   // The direction in which I travel.
    float inaccuracy = 30f;


	new void Start ()
    {
        base.Start();

        // Get the direction towards the player.
        //Vector3 targetPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        //targetPosition.x += Random.Range(-inaccuracy, inaccuracy);
        //targetPosition.z += Random.Range(-inaccuracy, inaccuracy);
        //targetPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
        //direction = targetPosition - transform.position;
        //direction.Normalize();
        //direction = Quaternion.Euler(0, Random.Range(-inaccuracy, inaccuracy), 0) * direction;
	}
	

	new void Update ()
    {
        base.Update();

        // Move in my precalculated direction.
        transform.position = transform.position + direction * speed * Time.deltaTime;
	}
}
