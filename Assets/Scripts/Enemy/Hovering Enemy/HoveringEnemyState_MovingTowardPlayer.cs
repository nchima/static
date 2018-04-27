using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoveringEnemyState_MovingTowardPlayer : State {

    [SerializeField] float meanderMaxScale = 10f;
    [SerializeField] float meanderNoiseSpeed = 0.01f;
    [SerializeField] float moveSpeed = 10f;

    Vector3 basePosition;
    float meanderSineTime = 0f;
    float meanderSineDirection;

    PerlinNoise movementNoise;


    private void Start() {
        //meanderSineTime = UnityEngine.Random.Range(-1000f, 1000f);
    }


    public override void Initialize(StateController stateController) {
        base.Initialize(stateController);
        HoveringEnemy controller = stateController as HoveringEnemy;

        basePosition = stateController.transform.position;

        // Get new noise variables.
        ResetMeanderSine();
        movementNoise = new PerlinNoise(meanderNoiseSpeed);
    }


    public override void Run(StateController stateController) {
        base.Run(stateController);
        HoveringEnemy controller = stateController as HoveringEnemy;

        controller.transform.position = new Vector3(controller.transform.position.x, controller.hoverHeight, controller.transform.position.z); 

        Vector3 playerGroundPosition = Services.playerTransform.position;
        playerGroundPosition.y = controller.hoverHeight;

        controller.transform.LookAt(playerGroundPosition);

        meanderSineTime += Time.deltaTime;
        float meanderScale = MyMath.Map(Vector3.Distance(basePosition, playerGroundPosition), 20f, 100f, 0f, meanderMaxScale);
        meanderScale = Mathf.Clamp(meanderScale, 0f, meanderMaxScale);
        float meanderAngle = MyMath.Map(Mathf.Sin(meanderSineTime), 0f, 1f, -meanderScale, meanderScale);

        Vector3 moveDirection = Quaternion.Euler(0f, meanderAngle, 0f) * controller.transform.forward;

        RaycastHit hit;
        if (Physics.SphereCast(controller.transform.position, controller.GetComponent<SphereCollider>().radius, moveDirection, out hit, 10f, (1 << 8) | (1 << 14) | (1 << 22) | (1 << 23) )) {

            // Directions perpendicular left and right from the detected wall.
            Vector3 direction1 = Quaternion.Euler(0f, 90f, 0f) * hit.normal;
            Vector3 direction2 = Quaternion.Euler(0f, -90f, 0f) * hit.normal;

            // Travel in the perpendicular direction that forms an obtuse angle with the direction to the player.
            if (Vector3.Angle(direction1, moveDirection) < Vector3.Angle(direction2, moveDirection)) {
                moveDirection = direction1;
            } else {
                moveDirection = direction2;
            }

            // If this new direction will still lead us into a wall, give up and change direction completely.
            //if (Physics.Raycast(controller.transform.position, directionTowardsPlayer, 20f, (1 << 8) | (1 << 24))) {
            //    GetNewFlankingAngle();
            //    return;
            //}
        }

        controller.m_Rigidbody.MovePosition(controller.transform.position + moveDirection * moveSpeed * Time.deltaTime);
    }


    //public override void Run(StateController stateController) {
    //    base.Run(stateController);
    //    HoveringEnemy controller = stateController as HoveringEnemy;

    //    movementNoise.Iterate();

    //    // Move base position towards player.
    //    Vector3 playerGroundPosition = Services.playerTransform.position;
    //    playerGroundPosition.y = controller.hoverHeight;
    //    Vector3 baseDirection = Vector3.Normalize(playerGroundPosition - controller.transform.position);
    //    baseDirection.y = 0f;
    //    basePosition += baseDirection * moveSpeed * Time.deltaTime;

    //    // Meander from base position.
    //    float meanderScale = MyMath.Map(Vector3.Distance(basePosition, playerGroundPosition), 20f, 100f, 0f, meanderMaxScale);
    //    meanderScale = Mathf.Clamp(meanderScale, 0f, meanderMaxScale);
    //    float meander = MyMath.Map(Mathf.Sin(meanderSineTime), 0f, 1f, -meanderScale, meanderScale);

    //    Vector3 newPosition = basePosition;
    //    Vector3 meanderDirection = Vector3.Normalize(Quaternion.Euler(0f, 90f, 0f) * baseDirection);
    //    meanderDirection.y = 0f;
    //    newPosition += meanderDirection * meander;

    //    // Raycast to look for solid objects in front of us.
    //    Vector3 directionToNewPosition = Vector3.Normalize(newPosition - controller.transform.position);
    //    RaycastHit hit;
    //    if (Physics.SphereCast(controller.transform.position, controller.GetComponent<SphereCollider>().radius, newPosition - controller.transform.position, out hit, 10f, (1 << 8) | (1 << 22))) {

    //        // Directions perpendicular left and right from the detected wall.
    //        Vector3 direction1 = Quaternion.Euler(0f, 90f, 0f) * hit.normal;
    //        Vector3 direction2 = Quaternion.Euler(0f, -90f, 0f) * hit.normal;

    //        // Travel in the perpendicular direction that forms an obtuse angle with the direction to the player.
    //        if (Vector3.Angle(direction1, baseDirection) < Vector3.Angle(direction2, baseDirection)) {
    //            directionToNewPosition = direction1;
    //        } else {
    //            directionToNewPosition = direction2;
    //        }

    //        meanderSineDirection *= -1f;

    //        // If this new direction will still lead us into a wall, give up and change direction completely.
    //        //if (Physics.Raycast(controller.transform.position, directionTowardsPlayer, 20f, (1 << 8) | (1 << 24))) {
    //        //    GetNewFlankingAngle();
    //        //    return;
    //        //}

    //    } else {
    //        meanderSineTime += meanderNoiseSpeed;
    //    }

    //    controller.m_Rigidbody.MovePosition(newPosition);
    //}

    void ResetMeanderSine() {
        meanderSineTime = 0f;
        meanderSineDirection = UnityEngine.Random.value;
        if (meanderSineDirection <= 0.5f) { meanderSineDirection = -1f; }
        else { meanderSineDirection = 1f; }
    }


    public override void End(StateController stateController) {
    }
}
