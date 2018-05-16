using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePopupManager : MonoBehaviour {

    [SerializeField] GameObject scorePopupPrefab;
    [SerializeField] Camera screenCamera;
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform popupContainer;

    public void CreateEnemyPopup(Vector3 enemyPosition, int score) {
        GameObject newPopup = Instantiate(scorePopupPrefab, popupContainer);

        Vector3 newPosition = playerCamera.WorldToViewportPoint(enemyPosition);
        newPosition = screenCamera.ViewportToWorldPoint(newPosition);
        newPosition.x = Mathf.Clamp(newPosition.x, -7.5f, 7.5f);
        newPosition.y += Random.Range(-4f, 4f);
        newPosition.z = 10f;
        newPopup.transform.position = newPosition;

        newPopup.GetComponent<TextMesh>().text = score.ToString();

        newPopup.GetComponent<ScorePopup>().BeginSequence();
    }
}
