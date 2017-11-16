using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletPooler : MonoBehaviour {

    [SerializeField] int numberOfBullets = 1000;
    [SerializeField] GameObject bulletPrefab;

    List<GameObject> bullets;


    private void Start() {
        // Instantiate all bullets.
        for (int i = 0; i < numberOfBullets; i++) {
            GameObject newBullet = Instantiate(bulletPrefab);
            newBullet.transform.parent = transform;
        }
    }
}
