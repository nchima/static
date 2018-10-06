using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TougherEnemy : Enemy {

    public float shootingDistance = 30f;
    public float stoppingDistance = 10f;
    public float distantMovementSpeed = 100f;
    public float shootingMovementSpeed = 50f;
    public float turningSpeed = 10f;
    public GameObject shotPrefab;
    public Transform shotOriginLeft;
    public Transform shotOriginRight;

    public Rigidbody m_Rigidbody { get { return GetComponent<Rigidbody>(); } }
}
