using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour {

    public Vector3 shakeScale = new Vector3(1f, 1f, 1f);  // Allows me to control how much this object shakes on each axis.

	float currentShake = 0f;   // How much the screen is currently shaking.
	float shakeAmount = 0.3f;   // How much the screen should shake at it's most shakey shaking.
    float shakeTimer = 0f;
    float vibrateAmount = 0.009f;    // How much the screen should vibrate to encourage zfighting.
	float decreaseFactor = 0.5f;    // How quickly the shake should decrease.
    float moveBackSpeed = 3f; // How quickly elements of the screen should move back to their original position after the shake ends.

    bool useDrift = false;

    // Ambient drift is used to make the screen drift apart when the gun is out of tune.
    const float ambientDriftMax = 0f;
    Vector3 currentDriftPosition = Vector3.zero;
    Vector3 driftNoiseTime = Vector3.zero;
    Vector3 driftNoiseOffset;
    const float DRIFT_NOISE_SPEED = 1f;

	Vector3 originalPosition;

	void Awake()
    {
		originalPosition = transform.localPosition;
        currentDriftPosition = originalPosition;

        driftNoiseOffset = Random.insideUnitSphere * 100f;
    }

	void Update()
    {
        // If the screen is currently shaking.
        if (shakeTimer > 0f)
        {
            // Get a random position relative to the drift position.
            Vector3 shakePosition = new Vector3(
                currentDriftPosition.x + Random.Range(-1f, 1f) * currentShake * shakeScale.x,
                currentDriftPosition.y + Random.Range(-1f, 1f) * currentShake * shakeScale.y,
                currentDriftPosition.z + Random.Range(-1f, 1f) * currentShake * shakeScale.z
            );

            // Update position.
            currentDriftPosition = shakePosition;

            // Decrease current shake.
            if (Time.timeScale == 0f) { currentShake = 0; }
            else { currentShake = Mathf.Clamp(currentShake - Time.deltaTime * decreaseFactor, 0f, 1000f); }
            //Debug.Log("current shake: " + currentShake);
            shakeTimer -= Time.deltaTime;
		}

        // If this object is not current shaking or has finished shaking.
        else if (useDrift)
        {
            // Set current shake to exaclty 0.
			currentShake = 0.0f;
            shakeTimer = 0f;

            // Move back towards original position.
            Vector3 newPosition = Vector3.Lerp(currentDriftPosition, originalPosition, moveBackSpeed * Time.deltaTime);
            currentDriftPosition = newPosition;
        }

        else {
            currentDriftPosition = originalPosition;
        }

        transform.localPosition = currentDriftPosition;
    }


    // Increase shake magnitude by a certain ammount.
	public void IncreaseShake(float increaseAmount) {
        currentShake += increaseAmount;
        shakeTimer = 0.5f;
	}


    // Set shake magnitude directly.
    public void SetShake(float magnitude, float time) {
        currentShake = magnitude;
        shakeTimer = time;
    }
}
