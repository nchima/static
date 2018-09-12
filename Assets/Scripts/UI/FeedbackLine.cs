using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FeedbackLine : MonoBehaviour {

	Material m_Material { get { return GetComponent<Renderer>().material; } }

    public IEnumerator Sequence(float edge1, float edge2, float startingWidth, Vector3 upDirection) {
        float center = Mathf.Abs(Mathf.Lerp(edge1, edge2, 0.5f));
        transform.localPosition = new Vector3(center, 0f, -0.59f);
        float height = Mathf.Abs(edge1 - edge2);
        height = Mathf.Clamp(height, 0.05f, 100f);
        transform.localScale = new Vector3(height, startingWidth, 1);

        float duration = 0.5f;

        Vector2 endOffset = m_Material.mainTextureScale;
        endOffset.y = 200f;
        m_Material.DOTiling(endOffset, duration).SetEase(Ease.InExpo);

        Vector3 endWidth = transform.localScale;
        endWidth.y = 0.01f;
        transform.DOScale(endWidth, duration).SetEase(Ease.InExpo);

        yield return new WaitForSeconds(duration);

        GetComponent<PooledObject>().ReturnToPool();

        yield return null;
    }
}
