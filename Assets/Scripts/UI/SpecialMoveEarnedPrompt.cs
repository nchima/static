using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpecialMoveEarnedPrompt : MonoBehaviour {

    [SerializeField] Vector3 endingPosition;
    [SerializeField] Vector2 endingSize;

    Vector3 startingPosition;
    Vector3 startingSize;

    RectTransform m_RectTransform { get{ return GetComponent<RectTransform>(); } }


    private void Start() {
        startingPosition = transform.localPosition;
        startingSize = m_RectTransform.sizeDelta;
    }


    public void Activate() {
        if (moveCoroutine != null) { StopCoroutine(moveCoroutine); }
        StartCoroutine(MoveCoroutine());
    }


    Coroutine moveCoroutine;
    IEnumerator MoveCoroutine() {
        // Move back to original position/size
        transform.localPosition = startingPosition;
        m_RectTransform.sizeDelta = startingSize;

        // Hang in place for a sec
        float duration = 1f;
        yield return new WaitForSeconds(duration);

        // Move and shrink
        duration = 0.5f;
        transform.DOLocalMove(endingPosition, duration);
        m_RectTransform.DOSizeDelta(endingSize, duration);
        yield return new WaitForSeconds(duration);

        yield return null;
    }
}
