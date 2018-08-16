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
    Text m_Text { get { return GetComponent<Text>(); } }


    private void Start() {
        if (!memorized) {
            startingPosition = transform.parent.localPosition;
            startingSize = m_RectTransform.sizeDelta;
            memorized = true;
            m_Text.enabled = false;
        }
    }


    public void Activate() {
        if (moveCoroutine != null) {
            StopCoroutine(moveCoroutine);
            transform.parent.localPosition = startingPosition;
            m_RectTransform.sizeDelta = startingSize;
            m_Text.enabled = false;
        }
        StartCoroutine(MoveCoroutine());
    }


    Coroutine moveCoroutine;
    IEnumerator MoveCoroutine() {
        m_Text.enabled = true;

        // Hang in place for a sec
        float duration = 2.5f;
        yield return new WaitForSeconds(duration);

        // Move and shrink
        duration = 0.5f;
        transform.parent.DOLocalMove(endingPosition, duration);
        m_RectTransform.DOSizeDelta(endingSize, duration);
        yield return new WaitForSeconds(duration + 0.1f);

        // Move back to original position/size and disable
        transform.parent.localPosition = startingPosition;
        m_RectTransform.sizeDelta = startingSize;
        m_Text.enabled = false;

        yield return null;
    }
}
