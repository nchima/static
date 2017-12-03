using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour {

    [SerializeField] Vector3 shakeScale = new Vector3(1f, 1f, 1f);  // Allows me to control how much this object shakes on each axis.

	float currentShake = 0f;   // How much the screen is currently shaking.
	float shakeAmount = 0.3f;   // How much the screen should shake at it's most shakey shaking.
    float shakeTimer = 0f;
    float vibrateAmount = 0.009f;    // How much the screen should vibrate to encourage zfighting.
	float decreaseFactor = 0.5f;    // How quickly the shake should decrease.
    float moveBackSpeed = 0.3f; // How quickly elements of the screen should move back to their original position after the shake ends.

    public bool useDrift = false;

    // Ambient drift is used to make the screen drift apart when the gun is out of tune.
    const float ambientDriftMax = 0f;
    Vector3 currentDriftPosition = Vector3.zero;
    Vector3 driftNoiseTime = Vector3.zero;
    Vector3 driftNoiseOffset;
    const float DRIFT_NOISE_SPEED = 1f;

	Vector3 originalPosition;

	void Start()
    {
		originalPosition = transform.position;
        currentDriftPosition = originalPosition;

        driftNoiseOffset = Random.insideUnitSphere * 100f;
    }

	void Update()
    {
        // Get a drift position.
        //currentDriftPosition = new Vector3(
        //    originalPosition.x + MyMath.Map(Mathf.PerlinNoise(driftNoiseTime.x + driftNoiseOffset.x, 0f), 0f, 1f, -ambientDriftMax, ambientDriftMax),
        //    originalPosition.y + MyMath.Map(Mathf.PerlinNoise(driftNoiseTime.y + driftNoiseOffset.y, 0f), 0f, 1f, -ambientDriftMax, ambientDriftMax),
        //    originalPosition.z + MyMath.Map(Mathf.PerlinNoise(driftNoiseTime.z + driftNoiseOffset.z, 0f), 0f, 1f, -ambientDriftMax, ambientDriftMax)
        //    );

        // Increment drift noise.
        //driftNoiseTime.x += DRIFT_NOISE_SPEED;
        //driftNoiseTime.y += DRIFT_NOISE_SPEED;
        //driftNoiseTime.z += DRIFT_NOISE_SPEED;

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
			currentShake -= Time.deltaTime * decreaseFactor;
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

            // Vibrate to encourage z fighting.
            //Vector3 vibratePosition = new Vector3(
            //        originalPosition.x + Random.Range(-vibrateAmount, vibrateAmount) * shakeScale.x,
            //        originalPosition.y + Random.Range(-vibrateAmount, vibrateAmount) * shakeScale.y,
            //        originalPosition.z + Random.Range(-vibrateAmount, vibrateAmount) * shakeScale.z
            //    );

            //vibratePosition = new Vector3(
            //       originalPosition.x + Mathf.Sin(Time.time*1.11f) *vibrateAmount * shakeScale.x*10f,
            //       originalPosition.y + Mathf.Sin(Time.time)  * vibrateAmount  * shakeScale.y * 10f,
            //       originalPosition.z + Mathf.Sin(Time.time*0.92f)  * vibrateAmount * shakeScale.z * 10f
            //   );

            // If this object is set to not move on certain axes, reset those axes to their current position.
            //if (!shakeOnXAxis) vibratePosition.x = transform.position.x;
            //if (!shakeOnYAxis) vibratePosition.y = transform.position.y;
            //if (!shakeOnZAxis) vibratePosition.z = transform.position.z;
            //vibratePosition = Vector3.Scale(vibratePosition, shakeScale);

            //transform.position = vibratePosition;
        }

        else {
            currentDriftPosition = originalPosition;
        }

        transform.position = currentDriftPosition;
    }


    // Increase shake magnitude by a certain ammount.
	void IncreaseShake(float increaseAmount) {
		currentShake += increaseAmount;
        shakeTimer = 0.5f;
	}


    // Set shake magnitude directly.
    public void SetShake(float magnitude, float time) {
        currentShake = magnitude;
        shakeTimer = time;
    }
}
