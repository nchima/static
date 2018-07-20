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

    bool memorized = false;

    RectTransform m_RectTransform { get{ return GetComponent<RectTransform>(); } }


    private void Start() {
        if (!memorized) {
            startingPosition = transform.parent.parent.localPosition;
            startingSize = m_RectTransform.sizeDelta;
            memorized = true;
        }
    }


    public void Activate() {
        if (moveCoroutine != null) {
            transform.parent.parent.localPosition = startingPosition;
            m_RectTransform.sizeDelta = startingSize;
            gameObject.SetActive(false);
            StopCoroutine(moveCoroutine);
        }
        StartCoroutine(MoveCoroutine());
    }


    Coroutine moveCoroutine;
    IEnumerator MoveCoroutine() {
        // Hang in place for a sec
        float duration = 2.5f;
        yield return new WaitForSeconds(duration);

        // Move and shrink
        duration = 0.5f;
        transform.parent.parent.DOLocalMove(endingPosition, duration);
        m_RectTransform.DOSizeDelta(endingSize, duration);
        yield return new WaitForSeconds(duration + 0.1f);

        // Move back to original position/size and disable
        transform.parent.parent.localPosition = startingPosition;
        m_RectTransform.sizeDelta = startingSize;
        gameObject.SetActive(false);

        yield return null;
    }
}
