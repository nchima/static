using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TaserManager : MonoBehaviour {

    // References
    [SerializeField] GameObject timerBar;
    [SerializeField] Transform timerBarReference;
    [SerializeField] GameObject tasedPrompt;
    
    // Timer
    [SerializeField] float maxTime = 6f;
    float mainTimer = 0f;

    [SerializeField] float bulletHitHangTime = 0.01f;
    [SerializeField] float bulletHitBonus = 0.01f;
    float _bulletHangTimer;
    float BulletHangTimer {
        get { return _bulletHangTimer; }
        set {
            _bulletHangTimer = value;
            _bulletHangTimer = Mathf.Clamp(_bulletHangTimer, 0f, bulletHitHangTime);
        }
    }

    // Pausing
    [HideInInspector] public bool forcePauseSpecialMove = false;
    [HideInInspector] public bool forcePauseForEpisodeComplete = false;

    // Hang time
    [SerializeField] float maxHangTime = 2f;
    float _hangTimer = 0f;
    float HangTimer {
        get { return _hangTimer; }
        set {
            _hangTimer = value;
            _hangTimer = Mathf.Clamp(_hangTimer, 0f, maxHangTime);
        }
    }

    // Various you know things
    [SerializeField] float killEnemyBonusTime = 3f;
    [SerializeField] float killEnemyHangTime = 0.1f;
    [SerializeField] float levelCompletedBonusTime = 6f;
    [SerializeField] float levelCompleteHangTime = 5f;
    [SerializeField] float pickupObtainedBonusTime = 3f;
    [SerializeField] float pickupObtainedHangTime = 0.1f;
    [SerializeField] float miscBonusTime = 1f;
    [SerializeField] float fallThroughFloorHangTime = 1f;
    [SerializeField] float episodeCompleteHangTime = 2f;

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PickupObtained>(PickupObtainedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerUsedSpecialMove>(PlayerUsedSpecialMoveHandler);
        GameEventManager.instance.Subscribe<GameEvents.Bullseye>(BullseyeHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameStarted>(GameStartedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerUsedFallThroughFloorMove>(PlayerUsedFallThroughFloorMoveHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PickupObtained>(PickupObtainedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerUsedSpecialMove>(PlayerUsedSpecialMoveHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.Bullseye>(BullseyeHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameStarted>(GameStartedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerUsedFallThroughFloorMove>(PlayerUsedFallThroughFloorMoveHandler);
    }

    private void Awake() {
        mainTimer = maxTime;
        timerBar.SetActive(false);
        forcePauseSpecialMove = true;
        forcePauseForEpisodeComplete = true;
    }

    private void Update() {
        return;
        // Run timer.
        if (!forcePauseSpecialMove && HangTimer <= 0f && BulletHangTimer <= 0f) {
            mainTimer -= Time.deltaTime;
            if (mainTimer <= 0) {
                TasePlayer();
                return;
            }
        }

        // Handle temporary puase.
        else {
            if (HangTimer > 0) {
                HangTimer -= Time.deltaTime;
            }

            if (BulletHangTimer > 0) {
                BulletHangTimer -= Time.deltaTime;
            }
        }

        Vector3 newTimerBarScale = timerBar.transform.localScale;
        newTimerBarScale.x = MyMath.Map(mainTimer, 0f, maxTime, 0.001f, timerBarReference.localScale.x);
        timerBar.transform.localScale = newTimerBarScale;

        Vector3 newTimerBarPosition = timerBar.transform.localPosition;
        newTimerBarPosition.x = MyMath.Map(mainTimer, 0f, maxTime, timerBarReference.localPosition.x - timerBarReference.localScale.x * 0.5f, timerBarReference.localPosition.x);
        timerBar.transform.localPosition = newTimerBarPosition;
    }

    public void TasePlayer() {
        StartCoroutine(TasingSequence());
    }

    IEnumerator TasingSequence() {
        // Hurt player
        //Services.healthManager.currentMaxHealth--;
        //Services.healthManager.CurrentHealth--;
        GameEventManager.instance.FireEvent(new GameEvents.PlayerWasTased());
        StartCoroutine(ShowTasedPromptSequence());

        // Freeze player
        PlayerController.State savedPlayerState = Services.playerController.state;
        Services.playerController.state = PlayerController.State.GettingTased;

        // Add temporary pause
        HangTimer = 1.6f;

        float savedTimeScale = Time.timeScale;

        MusicManager.State savedMusicState = Services.musicManager.state;
        Services.musicManager.state = MusicManager.State.GettingTased;

        // Cycle through random color palettes
        int colorChanges = 10;
        float frequency = 1.6f / colorChanges;
        for (int i = 0; i < colorChanges; i++) {
            Services.screenShakeManager.IncreaseShake(0.7f);
            Services.timeScaleManager.TweenTimeScale(UnityEngine.Random.Range(0.1f, 0.5f), frequency * 0.5f);
            Services.colorPaletteManager.ChangeToRandomPalette(frequency * 0.5f);
            yield return new WaitForSecondsRealtime(frequency);
        }

        Services.timeScaleManager.TweenTimeScale(savedTimeScale, 0.5f);
        Services.playerController.state = savedPlayerState;
        Services.screenShakeManager.SetShake(0f, 0f);
        Services.musicManager.SetMusicVolume(1f);
        Services.musicManager.state = savedMusicState;

        GameEventManager.instance.FireEvent(new GameEvents.PlayerWasHurt());

        yield return null;
    }

    public void EpisodeComplete() {
        mainTimer = maxTime;
        HangTimer += episodeCompleteHangTime;
    }

    public void PlayerShotEnemy() {
        BulletHangTimer += bulletHitHangTime;
        mainTimer += bulletHitBonus;
    }

    public void PlayerUsedFallThroughFloorMoveHandler(GameEvent gameEvent) {
        mainTimer = maxTime;
        HangTimer += fallThroughFloorHangTime;
    }

    public void PlayerKilledEnemyHandler(GameEvent gameEvent) {
        mainTimer = Mathf.Clamp(mainTimer + killEnemyBonusTime, 0f, maxTime);
        HangTimer += killEnemyHangTime;
    }

    public void LevelCompletedHandler(GameEvent gameEvent) {
        mainTimer = Mathf.Clamp(mainTimer + levelCompletedBonusTime, 0f, maxTime);
        HangTimer += levelCompleteHangTime;
    }

    public void PickupObtainedHandler(GameEvent gameEvent) {
        mainTimer = Mathf.Clamp(mainTimer + pickupObtainedBonusTime, 0f, maxTime);
        HangTimer += pickupObtainedHangTime;
    }

    public void PlayerUsedSpecialMoveHandler(GameEvent gameEvent) {
        forcePauseSpecialMove = true;
    }

    public void BullseyeHandler(GameEvent gameEvent) {
        mainTimer = Mathf.Clamp(mainTimer + miscBonusTime, 0f, maxTime);
    }

    public void GameStartedHandler(GameEvent gameEvent) {
        timerBar.SetActive(true);
        mainTimer = maxTime;
        forcePauseSpecialMove = false;
        forcePauseForEpisodeComplete = false;
    }

    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        // Reset timer
        mainTimer = maxTime;
    }

    private IEnumerator ShowTasedPromptSequence() {
        tasedPrompt.SetActive(true);
        yield return new WaitForSeconds(3f);
        tasedPrompt.SetActive(false);
    }
}
