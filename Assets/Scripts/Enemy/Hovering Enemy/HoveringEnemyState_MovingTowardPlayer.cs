using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoveringEnemyState_MovingTowardPlayer : State {

    [SerializeField] float meanderMaxScale = 10f;
    [SerializeField] float meanderNoiseSpeed = 0.01f;
    [SerializeField] float moveSpeed = 10f;

    PerlinNoise movementNoise;

    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);

        // Get new noise variables.
        movementNoise = new PerlinNoise(meanderNoiseSpeed);
    }


    public override void Run(StateController stateController) {
        base.Run(stateController);
        HoveringEnemy controller = stateController as HoveringEnemy;

        movementNoise.Iterate();

        // Move towards player in a meandering pattern.
        Vector3 moveDirection = Vector3.Normalize(GameManager.player.transform.position - stateController.transform.position);
        moveDirection.y = 0f;
        moveDirection = Quaternion.Euler(0f, movementNoise.MapValue(-meanderMaxScale, meanderMaxScale).x, 0f) * moveDirection;

        controller.m_Rigidbody.MovePosition(transform.position + moveDirection.normalized * moveSpeed * Time.deltaTime);
    }


    public override void End(StateController stateController) {
    }
}
