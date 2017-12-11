using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour {

    [SerializeField] AudioSource bulletHitEnemyAudioSource;

    public void PlayBulletHitEnemySound(EnemyOld hitEnemy) {

        /* Come back later and make this compatible with new enemies. */
        if (hitEnemy == null) { return; }

        //Get sound effect pitch based on enemies current vs max health.
        float newPitch = MyMath.Map(hitEnemy.HP, 0f, hitEnemy.maxHP, 1f, 0.75f);
        bulletHitEnemyAudioSource.pitch = newPitch;
        bulletHitEnemyAudioSource.Play();
    }
}
