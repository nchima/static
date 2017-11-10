using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {

    [HideInInspector] public bool beingDestroyed = false;

	public void DestroyByPlayerFalling()
    {
        if (beingDestroyed) return;
        beingDestroyed = true;

        // Destroy any other obstacles that are overlapping with me.
        Collider[] overlappingColliders = Physics.OverlapBox(transform.position, transform.localScale/2, transform.rotation, 1<<8);
        if (overlappingColliders.Length > 0)
        {
            for (int i = 0; i < overlappingColliders.Length; i++)
            {
                if (overlappingColliders[i].tag == "Obstacle")
                {
                    //if (!overlappingColliders[i].GetComponent<Obstacle>().beingDestroyed)
                    //{
                        overlappingColliders[i].GetComponent<Obstacle>().DestroyByPlayerFalling();
                    //}
                }
            }
        }

        // Destroy self.
        Destroy(gameObject);
    }
}
