using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShotgunCharge : MonoBehaviour {

	[SerializeField] GameObject sphere;

    [SerializeField] int collideDamage = 20;

    [SerializeField] float sphereInactivePosition = 0.9f;
    [SerializeField] float sphereActivePosition = 0.25f;

    FloatRange materialTilingRangeX = new FloatRange(11f, 17f);
    FloatRange materialTilingRangeY = new FloatRange(-18.7f, 18.7f);

    List<GameObject> capturedEnemies = new List<GameObject>();

    bool isReturningToFullSpeed;
    bool isFiringShockwave = false;
    float slowMoDuration = 0.25f;
    float sloMoTimer = 0f;
    [SerializeField] GameObject shockwavePrefab;

    bool isCharging = false;
    float chargeTimer = 0f;
    float minimumChargeDuration = 0.5f;
    public bool isReadyToEndCharge {
        get {
            bool returnValue = false;

            // See if the player is over a floor tile.
            RaycastHit hit1;
            RaycastHit hit2;

            // If we didn't find anything, return false.
            if (!Physics.Raycast(player.transform.position + player.transform.forward * 1.5f, Vector3.down, out hit1, 5f)) { return false; }
            if (!Physics.Raycast(player.transform.position + player.transform.forward * -1.5f, Vector3.down, out hit2, 5f)) { return false; }

            // If both things hit something and it was the floor, we're all good baby!
            if (hit1.transform.name.ToLower().Contains("floor") && hit2.transform.name.ToLower().Contains("floor")) {
                returnValue = true;
            // If it wasn't the floor, return false.
            } else {
                return false;
            }


            // Pee pee.
            if (chargeTimer >= minimumChargeDuration) { returnValue = true; }
            else { return false; }

            return returnValue;
        }
    }

    Transform player;


    private void Awake() {
        player = FindObjectOfType<PlayerController>().transform;
    }


    private void Update()
    {
        // Rotate visual sphere
        sphere.transform.rotation = Random.rotation;

        if (isCharging) {
            chargeTimer += Time.deltaTime;
        }

        // Do texture stuff on the visual sphere.
        foreach (MeshRenderer mr in sphere.GetComponentsInChildren<MeshRenderer>()) {
            mr.material.mainTextureOffset = new Vector2(0f, Random.Range(0f, 100f));
            mr.material.mainTextureScale = new Vector2(materialTilingRangeX.Random, materialTilingRangeY.Random);
            Color newColor = mr.material.color;
            newColor.r = Random.Range(0.8f, 1f);
            newColor.g = Random.Range(0.8f, 1f);
            mr.material.color = newColor;
        }

        // Keep captured enemies in front of player.
        for (int i = 0; i < capturedEnemies.Count; i++) {
            if (capturedEnemies[i] != null)
            {
                Vector3 forceDirection = Vector3.Normalize((transform.parent.position + transform.parent.forward * 15f) - capturedEnemies[i].transform.position);
                capturedEnemies[i].GetComponent<Rigidbody>().AddForce(forceDirection * 20f, ForceMode.Impulse);
            }

            else
            {
                capturedEnemies.Remove(capturedEnemies[i]);
            }
        }

        if (isFiringShockwave)
        {
            GameManager.instance.currentSine = -1f;

            GameManager.instance.gun.FireBurst();

            sloMoTimer += Time.deltaTime;
            if (sloMoTimer >= slowMoDuration && !isReturningToFullSpeed)
            {
                GameManager.instance.ReturnToFullSpeed();
                isReturningToFullSpeed = true;
            }

            // Finish shockwave sequence.
            else if (sloMoTimer >= slowMoDuration + 0.25f)
            {
                isFiringShockwave = false;
                isReturningToFullSpeed = false;
                GameManager.instance.forceInvincibility = false;
                player.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                sloMoTimer = 0f;
            }
        }
    }


    public void BeginCharge()
    {
        // Begin moving visual sphere into position.
        sphere.transform.DOLocalMoveZ(sphereActivePosition, 0.1f);

        // Make player temporarily invisible.
        GameManager.instance.forceInvincibility = true;

        // Make sure player does move up or down.
        player.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;

        // Allow player to pass through railings.
        Physics.IgnoreLayerCollision(16, 24, true);

        chargeTimer = 0f;
        isCharging = true;
    }


    public void EndCharge()
    {
        sphere.transform.DOLocalMoveZ(sphereInactivePosition, 0.1f);
        isCharging = false;

        Physics.IgnoreLayerCollision(16, 24, false);

        FireShockwave();
    }


    void FireShockwave()
    {
        GameManager.fallingSequenceManager.InstantiateShockwave(shockwavePrefab, 50f);
        capturedEnemies.Clear();
        isFiringShockwave = true;
    }


    public void OnTriggerEnter(Collider other)
    {
        if (isCharging && other.GetComponent<Enemy>() != null && !capturedEnemies.Contains(other.gameObject))
        {
            other.GetComponent<Enemy>().HP -= collideDamage;
            other.GetComponent<Enemy>().BecomePhysicsObject(2f);
            capturedEnemies.Add(other.gameObject);
        }
    }
}
