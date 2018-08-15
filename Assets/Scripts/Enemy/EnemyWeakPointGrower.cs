using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyWeakPointGrower : MonoBehaviour {

    [SerializeField] Vector3 minSizeScale;
    [SerializeField] float keepInFrontOffset = 2f;

    [HideInInspector] public Enemy myDad;
    Vector3 fullSizeScale;

    const int BULLETS_FOR_INSTANT_DEATH = 15;
    int hitsLastFrame;


    private void Awake() {
        fullSizeScale = transform.localScale;
        transform.localScale = minSizeScale;
    }

    private void Update() {

        if (myDad.isBeingKnockedBack) {
            SetColliderActive(false);
        }

        if (keepInFrontOffset != 0 && transform.localScale.magnitude > 0f) {
            // Billboard
            transform.LookAt(Services.playerTransform);
            transform.localRotation = Quaternion.Euler(90f, transform.localRotation.eulerAngles.y, transform.localRotation.eulerAngles.z);

            // Move in front.
            Vector3 directionToPlayer = Vector3.Normalize(transform.position - Services.playerTransform.position);
            transform.localPosition = directionToPlayer * keepInFrontOffset;
        }

        // See if I was struck by almost all of the player's bullets at once
        if (hitsLastFrame >= BULLETS_FOR_INSTANT_DEATH) {
            GameEventManager.instance.FireEvent(new GameEvents.Bullseye());
            myDad.currentHealth = 0;
        } else {
            hitsLastFrame = 0;
        }
    }

    private void SetColliderActive(bool value) {
        if (myDad.isBeingKnockedBack) { value = false; }
        GetComponent<Collider>().enabled = value;
    }

    public void SetScale(float scale) {
        transform.localScale = Vector3.Lerp(minSizeScale, fullSizeScale, scale);
    }

    public void Grow(float duration) {
        SetColliderActive(true);
        transform.DOScale(fullSizeScale, duration);
    }

    public void YouHurtMyDad(int howMuchYouHurtMyDad) {
        if (myDad == null) { Debug.Log("My Dad Equals Null"); return; }
        hitsLastFrame++;
        myDad.currentHealth -= howMuchYouHurtMyDad;
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
