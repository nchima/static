using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScorePopup : MonoBehaviour {

    float Alpha {
        get {
            return GetComponent<TextMesh>().color.a;
        }
        set {
            Color newColor = GetComponent<TextMesh>().color;
            newColor.a = value;
            GetComponent<TextMesh>().color = newColor;
        }
    }

    public void BeginSequence() {
        StartCoroutine(MySequence());
    }

    IEnumerator MySequence() {
        Alpha = 0f;

        float duration = 0.3f;
        DOTween.To(() => Alpha, x => Alpha = x, 1, duration);
        yield return new WaitForSeconds(duration);

        yield return new WaitForSeconds(0.5f);

        duration = 1f;
        DOTween.To(() => Alpha, x => Alpha = x, 0, duration);
        yield return new WaitForSeconds(duration);

        GetDestroyed();

        yield return null;
    }

    void GetDestroyed() {
        Destroy(gameObject);
    }
}
