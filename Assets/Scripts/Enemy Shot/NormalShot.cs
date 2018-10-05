using UnityEngine;
using System.Collections;

public class NormalShot : EnemyShot {

	[SerializeField] private float speed = 2f;  // The speed at which I travel.
    private Vector3 direction;   // The direction in which I travel.
    public Vector3 Direction {
        get { return direction; }
        set {
            direction = value;
            meshTransform.LookAt(transform.position + direction);
        }
    }
    float inaccuracy = 30f;
    Transform meshTransform { get { return GetComponent<MeshRenderer>().transform; } }


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

        Vector3 newRotation = meshTransform.localRotation.eulerAngles;
        newRotation.z += 1000f * Time.deltaTime;
        meshTransform.localRotation = Quaternion.Euler(newRotation);

        // Move in my precalculated direction.
        transform.position = transform.position + Direction * speed * Time.deltaTime;
	}
}
