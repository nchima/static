using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationController : MonoBehaviour {

    [SerializeField] protected float idleMorphSpeed;
    [SerializeField] protected FloatRange idleMorphNoiseRange = new FloatRange(1f, 100f);

    protected SkinnedMeshRenderer[] blendRenderers;

    // We need to use this because of the simple enemies; right now they don't use blend shapes.
    private Renderer[] renderers;

    Coroutine[] getHurtCoroutines;


    protected virtual void Start() {
        blendRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (blendRenderers.Length == 0) {
            renderers = GetComponentsInChildren<MeshRenderer>();
        } else {
            renderers = blendRenderers;
        }

        getHurtCoroutines = new Coroutine[renderers.Length];
    }


    public void GetHurt() {
        // Flashes the enemy red by moving it between layers.
        for (int i = 0; i < renderers.Length; i++) {
            if (getHurtCoroutines.Length > 0 && getHurtCoroutines[i] != null) { StopCoroutine(getHurtCoroutines[i]); }
            getHurtCoroutines[i] = StartCoroutine(GetHurtCoroutine(renderers[i].gameObject));
        }
    }


    float getHurtTime = 0.1f;
    Coroutine getHurtCoroutine;
    private IEnumerator GetHurtCoroutine(GameObject objectToFlash) {
        objectToFlash.layer = LayerMask.NameToLayer("Enemy Hit");
        yield return new WaitForSeconds(getHurtTime);
        if (objectToFlash.name.ToLower().Contains("sheathe")) {
            objectToFlash.layer = LayerMask.NameToLayer("Enemy Sheathe");
        } else {
            objectToFlash.layer = LayerMask.NameToLayer("Enemies");
        }
        yield return null;
    }
}
