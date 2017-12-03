using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Explosion : MonoBehaviour {

    enum ExplosionState { Expanding, Fading };
    ExplosionState explosionState;

	public float explosionRadius;
    [SerializeField] float pushForce;
    [SerializeField] float expandDuration;
    [SerializeField] float fadeDuration;
    public int damageMin;
    public int damageMax;
    [SerializeField] bool shouldDamagePlayer = true;    

    List<Collider> affectedObjects; // Contains references to all objects that have been collided with so that I don't hurt the same enemy multiple times.

    [SerializeField] GameObject explosionSphere;

    private GameManager gameManager;


    private void Start() {
        explosionState = ExplosionState.Expanding;

        explosionSphere.transform.DOScale(explosionRadius, expandDuration);
        affectedObjects = new List<Collider>();
    }


    private void Update()
    {
        if (explosionState == ExplosionState.Expanding)
        {
            if (explosionSphere.transform.localScale.x >= explosionRadius * 0.99f)
            {
                //Debug.Log("Explosion reached full size.");
                foreach(Renderer renderer in explosionSphere.GetComponentsInChildren<Renderer>()) renderer.material.DOFade(0f, fadeDuration);
                explosionState = ExplosionState.Fading;
            }
        }

        else if (explosionState == ExplosionState.Fading)
        {
            if (explosionSphere.GetComponentInChildren<Renderer>().material.color.a <= 0.01f)
            {
                //Debug.Log("Explosion deleting self.");
                Destroy(gameObject);
            }
        }
    }


    public void SetColor(Color _color) {
        explosionSphere.GetComponent<Renderer>().material.color = _color;
        explosionSphere.GetComponent<Renderer>().material.SetColor("_EmissionColor", _color);
    }


    public void OnTriggerStayChild(Collider collider)
    {
        //Debug.Log("Something affected by explosion.");

        if (explosionState != ExplosionState.Expanding) return;
        if (affectedObjects == null) { return; }
        if (affectedObjects.Contains(collider)) return;
        else affectedObjects.Add(collider);

        if (collider.tag == "Player") {
            if (!shouldDamagePlayer) return;

            // Raycast to see if player is behind cover.
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, collider.transform.position - transform.position, out hit, explosionRadius, 1<<8)) {
                if (hit.transform != collider.transform) return;
            }

            GameManager.instance.PlayerWasHurt();
        }

        else if (collider.tag == "Enemy") {
            //Debug.Log("Enemy affected by explosion.");
            //collider.GetComponent<Enemy>().BecomePhysicsObject(1f);
            collider.GetComponent<Rigidbody>().AddExplosionForce(pushForce, Vector3.Scale(transform.position, new Vector3(1f, 0f, 1f)), explosionRadius, pushForce*0.01f, ForceMode.Impulse);
            collider.GetComponent<Enemy>().HP -= damageMax;
        }

        else if (LayerMask.LayerToName(collider.gameObject.layer).Contains("ShootableBullet")) {
            collider.SendMessage("Detonate", SendMessageOptions.DontRequireReceiver);
        }
    }
}
