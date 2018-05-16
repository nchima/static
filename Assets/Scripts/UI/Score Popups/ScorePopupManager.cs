using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePopupManager : MonoBehaviour {

    [SerializeField] GameObject scorePopupPrefab;
    [SerializeField] Camera screenCamera;
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform popupContainer;
    [SerializeField] FloatRange sizeRange = new FloatRange(0.1f, 0.5f);
    [SerializeField] FloatRange scoreRange = new FloatRange(100, 1000);

    public void CreatePositionalPopup(Vector3 position, int score) {
        GameObject newPopup = Instantiate(scorePopupPrefab, popupContainer);

        Vector3 newPosition = playerCamera.WorldToViewportPoint(position);
        newPosition = screenCamera.ViewportToWorldPoint(newPosition);
        newPosition.x = Mathf.Clamp(newPosition.x, -7f, 7f);
        newPosition.y += Random.Range(1f, 3.5f) * MyMath.Either1orNegative1;
        //newPosition.y = Mathf.Clamp(newPosition.y, -4.5f, 4.5f);
        newPosition.z = 10f;
        newPopup.transform.position = newPosition;

        int multipliedScore = Mathf.RoundToInt(score * Services.scoreManager.multiplier);
        newPopup.GetComponent<TextMesh>().text = multipliedScore.ToString();
        newPopup.GetComponent<TextMesh>().characterSize = Mathf.Clamp(MyMath.Map((float)multipliedScore, scoreRange.min, scoreRange.max, sizeRange.min, sizeRange.max), sizeRange.min, sizeRange.max);

        newPopup.GetComponent<ScorePopup>().BeginSequence();
    }
}
