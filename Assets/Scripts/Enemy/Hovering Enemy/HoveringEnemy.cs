using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HoveringEnemy : Enemy {

    public float hoverHeight = 4f;
    public HoveringEnemyAnimationController m_AnimationController;

    public Rigidbody m_Rigidbody { get { return GetComponent<Rigidbody>(); } }
}
