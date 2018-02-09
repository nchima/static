using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;

public class MeleeEnemy : Enemy {

    [SerializeField] Rotator meshRotator;   // The game object that rotates.
    public GameObject geometryParent;
    [SerializeField] SkinnedMeshRenderer sheatheMeshRenderer;
    public AudioSource humAudioSource;
    public AudioSource attackAudioSource;
    public MeleeEnemyAnimationController m_AnimationController;

    [HideInInspector] public float rotationSpeedMax = 2000f; // How quickly we rotate just when we're about to charge.
    float rotationSpeedCurrent; // The current rotation speed.

    Color attackingColorCurrent;
    Color originalColor;
    [HideInInspector] public Color attackingColor = new Color(0.5f, 1f, 1f, 0.9f);   // The color we turn into when we're charging.

    // AUDIO
    [HideInInspector] public float originalHumVolume;    // The original hum volume.
    [HideInInspector] public float originalHumPitch; // The original pitch of the hum.

    [HideInInspector] public NavMeshAgent m_NavMeshAgent;


    private void Awake() {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        originalColor = sheatheMeshRenderer.material.GetColor("_FlashColor1");
        originalHumVolume = humAudioSource.volume;
        originalHumPitch = humAudioSource.pitch;
    }
    

    new void Start() {
        base.Start();
    }


    new void Update() {
        base.Update();

        // Handle animation and color switching.
        meshRotator.speedScale = rotationSpeedCurrent;
        sheatheMeshRenderer.material.color = attackingColorCurrent;
    }


    private void OnCollisionEnter(Collision collision)
    {
        // See if we hit the player.
        if (collision.collider.tag == "Player" && (currentState is MeleeEnemyState_Attacking)) {
            GameManager.instance.PlayerWasHurt();
        }

        // See if we hit an obstacle.
        else if ((collision.collider.tag == "Obstacle" || collision.collider.tag == "Wall") && currentState is MeleeEnemyState_Attacking) {
            Debug.Log("Melee enemy hit obstacle.");
            GetComponent<Rigidbody>().MovePosition(transform.position);
        }
    }


    public void TweenRotationSpeed(float value, float duration) {
        DOTween.To(() => rotationSpeedCurrent, x => rotationSpeedCurrent = x, value, duration).SetEase(Ease.InQuad).SetUpdate(true);
    }


    public void TweenAttackColor(Color value, float duration) {
        DOTween.To(() => attackingColorCurrent, x => attackingColorCurrent = x, value, duration).SetEase(Ease.InCubic).SetUpdate(true);
    }


    public void ReturnToOriginalAttackColor(float duration) {
        DOTween.To(() => attackingColorCurrent, x => attackingColorCurrent = x, originalColor, duration).SetEase(Ease.InCubic).SetUpdate(true);
    }
}
