using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionMonitor : MonoBehaviour {

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            Debug.Log("Player hit floor.");
            GameManager.fallingSequenceManager.playerTouchedDown = true;
        }
    }
}
