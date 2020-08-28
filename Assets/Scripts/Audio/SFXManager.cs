using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour {

    [SerializeField] AudioMixer sfxMasterMixer;
    [SerializeField] FloatRange sfxMasterVolumeRange;

    [SerializeField] AudioSource bulletHitEnemyAudioSource;
    [SerializeField] AudioSource bulletHitWeakPointAudioSource;
    [SerializeField] AudioSource playerWasHurtAudioSource;
    [SerializeField] AudioSource taserShockAudioSource;

    private float volume = 1f;
    private float volumeSliderValue = 1f;

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasTased>(PlayerWasTasedHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasTased>(PlayerWasTasedHandler);
    }

    public void PlayBulletEnemyHitSoundOld(EnemyOld hitEnemy) {

        /* Come back later and make this compatible with new enemies. */
        if (hitEnemy == null) { return; }

        //Get sound effect pitch based on enemies current vs max health.
        float newPitch = MyMath.Map(hitEnemy.HP, 0f, hitEnemy.maxHP, 1f, 0.75f);
        bulletHitEnemyAudioSource.pitch = newPitch;
        bulletHitEnemyAudioSource.Play();
    }

    public void PlayBulletHitEnemySound(Enemy hitEnemy) {

        /* Come back later and make this compatible with new enemies. */
        if (hitEnemy == null) { return; }

        //Get sound effect pitch based on enemies current vs max health.
        float newPitch = MyMath.Map(hitEnemy.currentHealth, 0f, hitEnemy.maxHealth, 1f, 0.75f);
        bulletHitEnemyAudioSource.pitch = newPitch;
        bulletHitEnemyAudioSource.Play();
    }

    public void PlayBulletHitWeakPointSound() {
        bulletHitWeakPointAudioSource.Play();
    }

    public void SetVolumeSliderValue(float value) {
        value = Mathf.Clamp01(value);
        volumeSliderValue = value;
        SetVolume(volume);
    }

    public void SetVolume(float value) {
        volume = Mathf.Clamp01(value);
        sfxMasterMixer.SetFloat("Master Volume", sfxMasterVolumeRange.MapTo(volume * volumeSliderValue, 0f, 1f));
    }

    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        if (Services.healthManager.isInvincible) { return; }
        playerWasHurtAudioSource.Play();
    }

    public void PlayerWasTasedHandler(GameEvent gameEvent) {
        taserShockAudioSource.Play();
    }
}
