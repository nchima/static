using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TankEnemy : Enemy {

    public float runAwayDistance = 30f;

    public NavMeshAgent m_NavMeshAgent { get { return GetComponent<NavMeshAgent>(); } }
}
