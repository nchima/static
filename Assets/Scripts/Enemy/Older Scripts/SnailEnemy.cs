using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnailEnemy : Enemy {

    public Vector3 directionToPlayer { get { return Vector3.Scale(Vector3.Normalize(GameManager.player.transform.position - transform.position), new Vector3(1f, 0f, 1f)); } }
}
