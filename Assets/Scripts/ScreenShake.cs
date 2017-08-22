using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour {

	float currentShake = 0f;   // How much the screen is currently shaking.
	float shakeAmount = 0.3f;   // How much the screen should shake at it's most shakey shaking.
    float vibrateAmount = 0.001f;    // How much the screen should vibrate to encourage zfighting.
	float decreaseFactor = 1.1f;    // How quickly the shake should decrease.
    float moveBackSpeed = 0.5f; // How quickly elements of the screen should move back to their original position after the shake ends.

    // Ambient drift is used to make the screen drift apart when the gun is out of tune.
    const float ambientDriftMax = 0f;
    Vector3 currentDriftPosition = Vector3.zero;
    Vector2 driftNoiseTime = Vector2.zero;
    Vector2 driftNoiseOffset;
    const float DRIFT_NOISE_SPEED = 1f;

	Vector3 originalPosition;

	void Start()
    {
		originalPosition = transform.position;
        currentDriftPosition = originalPosition;

        driftNoiseOffset = Random.insideUnitCircle * 100f;
    }

	void Update()
    {
        // Handle ambient drift.
        currentDriftPosition = new Vector3(
            originalPosition.x + MyMath.Map(Mathf.PerlinNoise(driftNoiseTime.x + driftNoiseOffset.x, 0f), 0f, 1f, -ambientDriftMax, ambientDriftMax),
            originalPosition.y + MyMath.Map(Mathf.PerlinNoise(driftNoiseTime.x + 18321231027373f, 0f), 0f, 1f, -ambientDriftMax, ambientDriftMax),
            originalPosition.z + MyMath.Map(Mathf.PerlinNoise(driftNoiseTime.y + driftNoiseOffset.y, 0f), 0f, 1f, -ambientDriftMax, ambientDriftMax)
            );

        driftNoiseTime.x += DRIFT_NOISE_SPEED;
        driftNoiseTime.y += DRIFT_NOISE_SPEED;

        // If the screen is currently shaking.
        if (currentShake > 0f)
        {
            // Move around all randomly.
			transform.position = currentDriftPosition + Random.insideUnitSphere * shakeAmount;

            // Decrease current shake.
			currentShake -= Time.deltaTime * decreaseFactor;
		}

        else
        {
            // Set current shake to exaclty 0.
			currentShake = 0.0f;

            // Move back towards original position.
            //if (Vector3.Distance(transform.position, currentDriftPosition) > 0.01f)
            //{
                Vector3 newPosition = Vector3.Lerp(transform.position, currentDriftPosition, moveBackSpeed * Time.deltaTime);
                transform.position = newPosition;
            //}

            // Vibrate to encourage z fighting.
            transform.position = transform.position + Random.insideUnitSphere * vibrateAmount;
        }
    }

	void IncreaseShake(float increaseAmount)
    {
		currentShake += increaseAmount;
	}
}
