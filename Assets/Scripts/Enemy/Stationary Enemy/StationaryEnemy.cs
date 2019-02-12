using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationaryEnemy : Enemy {
    public GameObject enemyTop;

    public StationaryEnemyAnimationController m_AnimationController {
        get { return myGeometry.GetComponent<StationaryEnemyAnimationController>(); }
    }
}
