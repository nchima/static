using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour {

	float currentShake = 0f;   // How much the screen is currently shaking.
	float shakeAmount = 0.3f;   // How much the screen should shake at it's most shakey shaking.
    float vibrateAount = 0.001f;    // How much the screen should vibrate to encourage zfighting.
	float decreaseFactor = 1.0f;    // How quickly the shake should decrease.
    float moveBackSpeed = 0.3f; // How quickly elements of the screen should move back to their original position after the shake ends.

	Vector3 originalPosition;

	void Start()
    {
		originalPosition = transform.position;
	}

	void Update()
    {
        // If the screen is currently shaking.
		if (currentShake > 0f)
        {
            // Move around all randomly.
			transform.position = originalPosition + new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0) * shakeAmount;

            // Decrease current shake.
			currentShake -= Time.deltaTime * decreaseFactor;

		}

        else
        {
            // Set current shake to exaclty 0.
			currentShake = 0.0f;

            // Move back towards original position.
            if (Vector3.Distance(transform.position, originalPosition) > 0.01f)
            {
                Vector3 newPosition = Vector3.Lerp(transform.position, originalPosition, moveBackSpeed * Time.deltaTime);
                transform.position = newPosition;
            }

            // Vibrate to encourage z fighting.
            transform.position = transform.position + new Vector3(Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0) * vibrateAount;
        }
    }

	void IncreaseShake(float increaseAmount)
    {
		currentShake += increaseAmount;
	}
}
