using UnityEngine;
using System.Collections;

public class MonitorSliderScript : MonoBehaviour {

	public float minXPos;
	public float maxXPos;

    GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update ()
    {
		float newXPos = MyMath.Map (gameManager.currentSine, -1f, 1f, minXPos, maxXPos);
		transform.localPosition = new Vector3 (newXPos, transform.localPosition.y, transform.localPosition.z);
	}
}
