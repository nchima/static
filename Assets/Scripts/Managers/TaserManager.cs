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
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PickupObtained>(PickupObtainedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerUsedSpecialMove>(PlayerUsedSpecialMoveHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.Bullseye>(BullseyeHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameStarted>(GameStartedHandler);
    }

    private void Awake() {
        mainTimer = maxTime;
        timerBar.SetActive(false);
        forcePauseSpecialMove = true;
        forcePauseForEpisodeComplete = true;
    }

    private void Update() {
        // Run timer.
        if (!forcePauseSpecialMove && HangTimer <= 0f) {
            mainTimer -= Time.deltaTime;
            if (mainTimer <= 0) {
                TasePlayer();
                return;
            }
        }

        // Handle temporary puase.
        else if (HangTimer > 0) {
            HangTimer -= Time.deltaTime;
        }

        Vector3 newTimerBarScale = timerBar.transform.localScale;
        newTimerBarScale.x = MyMath.Map(mainTimer, 0f, maxTime, 0.001f, timerBarReference.localScale.x);
        timerBar.transform.localScale = newTimerBarScale;

        Vector3 newTimerBarPosition = timerBar.transform.localPosition;
        newTimerBarPosition.x = MyMath.Map(mainTimer, 0f, maxTime, timerBarReference.localPosition.x - timerBarReference.localScale.x * 0.5f, timerBarReference.localPosition.x);
        timerBar.transform.localPosition = newTimerBarPosition;
    }

    public void TasePlayer() {
        // Hurt player
        GameEventManager.instance.FireEvent(new GameEvents.PlayerWasHurt());
        StartCoroutine(ShowTasedPromptSequence());

        // Reset timer
        mainTimer = maxTime;

        // Add temporary pause
        HangTimer = 0.5f;
    }

    public void PlayerFellThroughFloor() {
        mainTimer = maxTime;
        HangTimer += fallThroughFloorHangTime;
    }

    public void EpisodeComplete() {
        mainTimer = maxTime;
        HangTimer += episodeCompleteHangTime;
    }

    public void PlayerKilledEnemyHandler(GameEvent gameEvent) {
        mainTimer = Mathf.Clamp(mainTimer + killEnemyBonusTime, 0f, maxTime);
        HangTimer += killEnemyHangTime;
    }

    public void LevelCompletedHandler(GameEvent gameEvent) {
        mainTimer = Mathf.Clamp(mainTimer + levelCompletedBonusTime, 0f, maxTime);
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

    private IEnumerator ShowTasedPromptSequence() {
        tasedPrompt.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        tasedPrompt.SetActive(false);
    }
}
