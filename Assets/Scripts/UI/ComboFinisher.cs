using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ComboFinisher : MonoBehaviour {

    Text m_Text { get { return GetComponent<Text>(); } }
    RectTransform m_RectTransform { get { return GetComponent<RectTransform>(); } }
    bool isPaused = false;
    bool visibleBeforePause = true;

    public void Initialize(int amount, int fontSize) {
        StartCoroutine(Sequence(amount, fontSize));
    }

    public void Pause(bool value) {
        isPaused = value;

        if (value == true) {
            visibleBeforePause = m_Text.enabled;
            SetVisible(false);
        }

        else {
            SetVisible(visibleBeforePause);
        }
    }

    public void SetVisible(bool value) {
        m_Text.enabled = value;
    }

    private IEnumerator Sequence(int comboAmount, int fontSize) {
        m_Text.fontSize = fontSize;

        // Count up to full amount and move right.
        float duration = 1f;
        float countUpAmount = 0;
        DOTween.To(() => countUpAmount, x => countUpAmount = x, comboAmount, duration);
        Vector3 nextPosition = m_RectTransform.anchoredPosition;
        nextPosition.x = 1.12f;
        //nextPosition.x = 20f;
        m_RectTransform.DOAnchorPos(nextPosition, duration * 0.7f);
        yield return new WaitUntil(() => {
            if (isPaused) { return false; }

            if (countUpAmount >= comboAmount) {
                m_Text.text = Mathf.CeilToInt(comboAmount).ToString();
                return true;
            }
            else {
                m_Text.text = Mathf.CeilToInt(countUpAmount).ToString();
                return false;
            }
        });

        m_RectTransform.anchoredPosition = nextPosition;

        // Wait a seccy.
        yield return new WaitForSeconds(0.5f);

        // Move up to the score area.
        duration = 0.4f;
        nextPosition = m_RectTransform.anchoredPosition;
        nextPosition.y = 55.91f;
        m_RectTransform.DOAnchorPos(nextPosition, duration);
        yield return new WaitForSeconds(duration);

        Services.scoreManager.Score += comboAmount;
        Destroy(gameObject);

        yield return null;
    }
}
