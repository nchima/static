using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAudioSourceCloseToPlayer : MonoBehaviour {

    Collider myCollider;

    private void Start()
    {
        myCollider = transform.parent.GetComponentInChildren<Collider>();
    }

    private void Update()
    {
        Vector3 closestPointToPlayer = myCollider.ClosestPoint(Services.playerTransform.position);
        transform.position = closestPointToPlayer;
    }
}
