using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ComboFinisher : MonoBehaviour {

    Text m_Text { get { return GetComponent<Text>(); } }
    RectTransform m_RectTransform { get { return GetComponent<RectTransform>(); } }

    public void Initialize(int amount, int fontSize) {
        StartCoroutine(Sequence(amount, fontSize));
    }

    private IEnumerator Sequence(int comboAmount, int fontSize) {
        m_Text.fontSize = fontSize;
        
        // Count up to full amount and move right.
        float duration = 1f;
        float countUpAmount = 0;
        DOTween.To(() => countUpAmount, x => countUpAmount = x, comboAmount, duration);
        Vector3 nextPosition = m_RectTransform.localPosition;
        nextPosition.x = 19.01f;
        m_RectTransform.DOLocalMove(nextPosition, duration * 0.7f);
        yield return new WaitUntil(() => {
            if (countUpAmount >= comboAmount) {
                m_Text.text = Mathf.CeilToInt(comboAmount).ToString();
                return true;
            }
            else {
                m_Text.text = Mathf.CeilToInt(countUpAmount).ToString();
                return false;
            }
        });

        // Wait a seccy.
        yield return new WaitForSeconds(0.5f);

        // Move up to the score area.
        duration = 0.4f;
        nextPosition = m_RectTransform.localPosition;
        nextPosition.y = 11.72f;
        m_RectTransform.DOLocalMove(nextPosition, duration);
        yield return new WaitForSeconds(duration);

        Services.scoreManager.Score += comboAmount;
        Destroy(gameObject);

        yield return null;
    }
}
