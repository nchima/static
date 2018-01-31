using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyWeakPointGrower : MonoBehaviour {

    [SerializeField] Vector3 minSizeScale;
    Vector3 fullSizeScale;

    private void Awake() {
        fullSizeScale = transform.localScale;
        transform.localScale = minSizeScale;
    }

    public void Grow(float duration) {
        GetComponent<Collider>().enabled = true;
        transform.DOScale(fullSizeScale, duration);
    }

    public void Shrink(float duration) {
        StartCoroutine(ShrinkCoroutine(duration));
    }

    IEnumerator ShrinkCoroutine(float duration) {
        transform.DOScale(minSizeScale, duration);
        yield return new WaitForSeconds(duration);
        GetComponent<Collider>().enabled = false;
        yield return null;
    }
}
