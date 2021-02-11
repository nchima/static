using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ComboManager : MonoBehaviour {

    // References
    [SerializeField] GameObject timerBar;
    [SerializeField] Transform timerBarReference;
    [SerializeField] Text textDisplay;
    [SerializeField] Text scoreDisplay;
    [SerializeField] Text multiplierDisplay;
    [SerializeField] float fontSizeIncreaseFactor = 2f;
    [SerializeField] GameObject comboFinisherPrefab;
    [SerializeField] GameObject comboFinisherParent;

    enum State { ComboActive, ComboIdle }
    State state = State.ComboIdle;

    // Timer
    const float MAX_COMBO_TIME = 3f;
    float comboTimer = 0f;

    const float KILL_BONUS_TIME = 1f;
    const float LEVEL_COMPLETED_BONUS_TIME = 2f;
    const float PICKUP_OBTAINED_BONUS_TIME = 1f;
    const float MISC_BONUS_TIME = 1f;

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

    [SerializeField] float bulletHitHangTime = 0.01f;
    float _bulletHangTimer;
    float BulletHangTimer {
        get { return _bulletHangTimer; }
        set {
            _bulletHangTimer = value;
            _bulletHangTimer = Mathf.Clamp(_bulletHangTimer, 0f, bulletHitHangTime);
        }
    }

    // Other values
    const float KILL_ENEMY_HANG_TIME = 0.1f;
    public const int PICKUP_SCORE_VALUE = 100;
    const float PICKUP_HANG_TIME = 0.1f;
    const int BULLSEYE_VALUE = 200;
    const int SPECIAL_MOVE_VALUE = 50;
    public int levelCompleteValue = 500;
    const float LEVEL_COMPLETE_HANG_TIME = 5f;
    const float FALL_THROUGH_FLOOR_HANG_TIME = 1f;

    // Current combo data.
    int simpleEnemiesKilled = 0;
    int meleeEnemiesKilled = 0;
    int laserEnemiesKilled = 0;
    int tankEnemiesKilled = 0;
    int hoverEnemiesKilled = 0;
    int bossEnemiesKilled = 0;
    int tougherEnemiesKilled = 0;
    int pickupsObtained = 0;
    int levelsCompleted = 0;
    int bullseyes = 0;
    int timesSpecialMoveUsed = 0;

    int currentEnemiesKilledScoreTotal = 0;

    int initialMultiplierFontSize;
    float modifiedFontSize;
    int CurrentMultiplierFontSize { get { return Mathf.FloorToInt(MyMath.Map(CurrentMultiplier, 0, 400, initialMultiplierFontSize * 1.3f, 2000)); } }
    float initialMultiplierScale;
    float CurrentMultiplierScale { get { return MyMath.Map(CurrentMultiplier, 0, 400, initialMultiplierScale * 1.3f, 0.027f); } }

    int CurrentMultiplier { get { return simpleEnemiesKilled + meleeEnemiesKilled + laserEnemiesKilled + tankEnemiesKilled + hoverEnemiesKilled + bossEnemiesKilled + bullseyes + pickupsObtained + timesSpecialMoveUsed + levelsCompleted + 1; } }

    Coroutine increaseComboValueCoroutine;

    bool comboFinishersPaused = false;
    bool comboFinishersVisible = true;

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PickupObtained>(PickupObtainedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerUsedSpecialMove>(PlayerUsedSpecialMoveHandler);
        GameEventManager.instance.Subscribe<GameEvents.Bullseye>(BullseyeHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameOver>(GameOverHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerUsedFallThroughFloorMove>(PlayerUsedFallThroughFloorMoveHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PickupObtained>(PickupObtainedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerUsedSpecialMove>(PlayerUsedSpecialMoveHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.Bullseye>(BullseyeHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameOver>(GameOverHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerUsedFallThroughFloorMove>(PlayerUsedFallThroughFloorMoveHandler);
    }

    private void Awake() {
        initialMultiplierFontSize = multiplierDisplay.fontSize;
        modifiedFontSize = initialMultiplierFontSize;
        initialMultiplierScale = multiplierDisplay.rectTransform.localScale.x;

        textDisplay.text = "";
        multiplierDisplay.text = "";
        scoreDisplay.text = "";
    }

    private void Update() {
        if (state == State.ComboActive) {
            if (HangTimer <= 0f && BulletHangTimer <= 0f) {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0) {
                    EndCombo();
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

            textDisplay.text = GenerateComboTextString();
            scoreDisplay.text = GetUnmultipliedTotal().ToString();
            multiplierDisplay.text = "X" + CurrentMultiplier.ToString();
        }

        // Update size of multiplier display
        multiplierDisplay.fontSize = Mathf.FloorToInt(modifiedFontSize);

        // Move the multiplier display the correct distance from the score display
        Vector3 newPosition = scoreDisplay.transform.localPosition;
        newPosition.y = multiplierDisplay.rectTransform.localPosition.y;
        newPosition.x += scoreDisplay.rectTransform.sizeDelta.x * scoreDisplay.rectTransform.lossyScale.x + 0.3f;
        multiplierDisplay.rectTransform.localPosition = newPosition;

        // Update timer bar
        Vector3 newTimerBarScale = timerBar.transform.localScale;
        newTimerBarScale.x = MyMath.Map(comboTimer, 0f, MAX_COMBO_TIME, 0.001f, timerBarReference.localScale.x);
        timerBar.transform.localScale = newTimerBarScale;

        Vector3 newTimerBarPosition = timerBar.transform.localPosition;
        newTimerBarPosition.x = MyMath.Map(comboTimer, 0f, MAX_COMBO_TIME, timerBarReference.localPosition.x - timerBarReference.localScale.x * 0.5f, timerBarReference.localPosition.x);
        timerBar.transform.localPosition = newTimerBarPosition;
    }

	private void StartCombo() {
        comboTimer = MAX_COMBO_TIME;
        state = State.ComboActive;
    }

    public void EndCombo() {
        ComboFinisher comboFinisher = Instantiate(comboFinisherPrefab, comboFinisherParent.transform).GetComponent<ComboFinisher>();
        comboFinisher.transform.position = scoreDisplay.transform.position;
        comboFinisher.Initialize(GetMultipliedTotal(), CurrentMultiplierFontSize);
        if (!comboFinishersVisible) { comboFinisher.SetVisible(false); }
        if (comboFinishersPaused) { comboFinisher.Pause(true); }

        simpleEnemiesKilled = 0;
        meleeEnemiesKilled = 0;
        laserEnemiesKilled = 0;
        tankEnemiesKilled = 0;
        hoverEnemiesKilled = 0;
        bossEnemiesKilled = 0;
        tougherEnemiesKilled = 0;
        levelsCompleted = 0;
        pickupsObtained = 0;
        bullseyes = 0;
        timesSpecialMoveUsed = 0;
        currentEnemiesKilledScoreTotal = 0;

        textDisplay.text = "";
        scoreDisplay.text = "";
        multiplierDisplay.text = "";

		//StartCombo ();

        state = State.ComboIdle;
    }

    private void StartIncreaseComboValueCoroutine() {
        if (increaseComboValueCoroutine != null) { StopCoroutine(increaseComboValueCoroutine); }
        increaseComboValueCoroutine = StartCoroutine(IncreaseComboValueCoroutine());
    }

    private int GetUnmultipliedTotal() {
        int totalScore = 0;
        totalScore += currentEnemiesKilledScoreTotal;
        totalScore += levelsCompleted * levelCompleteValue;
        totalScore += pickupsObtained * PICKUP_SCORE_VALUE;
        totalScore += timesSpecialMoveUsed * SPECIAL_MOVE_VALUE;
        totalScore += bullseyes * BULLSEYE_VALUE;
        return totalScore;
    }

    public int GetMultipliedTotal() {
        return GetUnmultipliedTotal() * CurrentMultiplier;
    }

    private string GenerateComboTextString() {
        string comboString = "";

        if (bullseyes > 0) {
            if (comboString != "") { comboString += " + "; }
            comboString += "Bullseye! x" + bullseyes.ToString();
        }

        if (simpleEnemiesKilled > 0) {
            if (comboString != "") { comboString += " + "; }
            comboString += "Basic Enemy Killed x" + simpleEnemiesKilled.ToString();
        }

        if (meleeEnemiesKilled > 0) {
            if (comboString != "") { comboString += " + "; }
            comboString += "Melee Enemy Killed x" + meleeEnemiesKilled.ToString();
        }

        if (laserEnemiesKilled > 0) {
            if (comboString != "") { comboString += " + "; }
            comboString += "Laser Enemy Killed x" + laserEnemiesKilled.ToString();
        }

        if (tankEnemiesKilled > 0) {
            if (comboString != "") { comboString += " + "; }
            comboString += "Tank Enemy Killed x" + tankEnemiesKilled.ToString();
        }

        if (hoverEnemiesKilled > 0) {
            if (comboString != "") { comboString += " + "; }
            comboString += "Hovering Enemy Killed x" + hoverEnemiesKilled.ToString();
        }

        if (bossEnemiesKilled > 0) {
            if (comboString != "") { comboString += " + "; }
            comboString += "Boss Enemy Killed x" + bossEnemiesKilled.ToString();
        }

        if (tougherEnemiesKilled > 0) {
            if (comboString != "") { comboString += " + "; }
            comboString += "Tough Enemies Killed x" + tougherEnemiesKilled.ToString();
        }

        if (levelsCompleted > 0) {
            if (comboString != "") { comboString += " + "; }
            comboString += "Level Completed x" + levelsCompleted.ToString();
        }

        if (pickupsObtained > 0) {
            if (comboString != "") { comboString += " + "; }
            comboString += "Multiplier Pickup x" + pickupsObtained.ToString();
        }

        if (timesSpecialMoveUsed > 0) {
            if (comboString != "") { comboString += " + "; }
            comboString += "Special Move Used x" + timesSpecialMoveUsed.ToString();
        }

        return comboString;
    }

    private string GetComboTotalAsString() {
        string total = GetUnmultipliedTotal().ToString();
        if (CurrentMultiplier > 1) { total += " X " + CurrentMultiplier.ToString(); }
        return total;
    }

    public void PauseAllFinishers(bool value) {
        foreach(ComboFinisher finisher in comboFinisherParent.GetComponentsInChildren<ComboFinisher>()) {
            finisher.Pause(value);
        }

        comboFinishersPaused = value;
    }

    public void SetAllFinishersVisible(bool value) {
        foreach(ComboFinisher finisher in comboFinisherParent.GetComponentsInChildren<ComboFinisher>()) {
            finisher.SetVisible(value);
        }

        comboFinishersVisible = value;
    }

    public void PlayerShotEnemy() {
        BulletHangTimer += bulletHitHangTime;
    }

    public void PlayerUsedFallThroughFloorMoveHandler(GameEvent gameEvent) {
        comboTimer = MAX_COMBO_TIME;
        HangTimer += FALL_THROUGH_FLOOR_HANG_TIME;
    }

    public void PlayerKilledEnemyHandler(GameEvent gameEvent) {
        GameEvents.PlayerKilledEnemy playerKilledEnemyEvent = gameEvent as GameEvents.PlayerKilledEnemy;

        if (state == State.ComboIdle) {
            StartCombo();
        }
        else {
            HangTimer += KILL_ENEMY_HANG_TIME;
            comboTimer = Mathf.Clamp(comboTimer + KILL_BONUS_TIME, 0f, MAX_COMBO_TIME);
        }

        if (playerKilledEnemyEvent.enemyKilled is SimpleEnemy) { simpleEnemiesKilled++; } 
        else if (playerKilledEnemyEvent.enemyKilled is MeleeEnemy) { meleeEnemiesKilled++; }
        else if (playerKilledEnemyEvent.enemyKilled is LaserEnemy) { laserEnemiesKilled++; }
        else if (playerKilledEnemyEvent.enemyKilled is TankEnemy) { tankEnemiesKilled++; }
        else if (playerKilledEnemyEvent.enemyKilled is HoveringEnemy) { hoverEnemiesKilled++; }
        else if (playerKilledEnemyEvent.enemyKilled is SnailEnemy) { bossEnemiesKilled++; }
        else if (playerKilledEnemyEvent.enemyKilled is TougherEnemy) { tougherEnemiesKilled++; }

        currentEnemiesKilledScoreTotal += playerKilledEnemyEvent.enemyKilled.scoreKillValue;

        StartIncreaseComboValueCoroutine();
    }

    public void LevelCompletedHandler(GameEvent gameEvent) {
        if (state == State.ComboIdle) {
            StartCombo();
        }
        else {
            comboTimer = Mathf.Clamp(comboTimer + LEVEL_COMPLETED_BONUS_TIME, 0f, MAX_COMBO_TIME);
            HangTimer += LEVEL_COMPLETE_HANG_TIME;
        }

        levelsCompleted++;

        StartIncreaseComboValueCoroutine();
    }

    public void PickupObtainedHandler(GameEvent gameEvent) {
        if (state == State.ComboIdle) {
            StartCombo();
        }
        else {
            comboTimer = Mathf.Clamp(comboTimer + PICKUP_OBTAINED_BONUS_TIME, 0f, MAX_COMBO_TIME);
            HangTimer += PICKUP_HANG_TIME;
        }

        pickupsObtained++;

        StartIncreaseComboValueCoroutine();
    }

    public void PlayerUsedSpecialMoveHandler(GameEvent gameEvent) {
        if (state == State.ComboIdle) {
            StartCombo();
        }
        else {
            comboTimer = Mathf.Clamp(comboTimer + MISC_BONUS_TIME, 0f, MAX_COMBO_TIME);
        }

        timesSpecialMoveUsed++;

        StartIncreaseComboValueCoroutine();
    }

    public void BullseyeHandler(GameEvent gameEvent) {
        if (state == State.ComboIdle) {
            StartCombo();
        }
        else {
            comboTimer = Mathf.Clamp(comboTimer + MISC_BONUS_TIME, 0f, MAX_COMBO_TIME);
        }

        bullseyes++;

        StartIncreaseComboValueCoroutine();
    }

    public void GameOverHandler(GameEvent gameEvent) {
        EndCombo();

        StartIncreaseComboValueCoroutine();
    }

    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        EndCombo();
    }

    IEnumerator FuckOff() {
        TextGenerationSettings settings = new TextGenerationSettings();
        settings.resizeTextForBestFit = true;
        settings.textAnchor = TextAnchor.MiddleCenter;
        settings.color = textDisplay.color;
        settings.generationExtents = new Vector2(1000f, 1000f);
        settings.pivot = Vector2.zero;
        settings.richText = true;
        settings.font = textDisplay.font;
        //settings.fontSize = comboTextDisplay.fontSize;
        settings.fontStyle = textDisplay.fontStyle;
        settings.verticalOverflow = VerticalWrapMode.Overflow;
        TextGenerator generator = new TextGenerator();
        generator.Populate(textDisplay.text, settings);

        yield return new WaitForEndOfFrame();

        textDisplay.fontSize = Mathf.FloorToInt(generator.fontSizeUsedForBestFit / textDisplay.canvas.scaleFactor);

        yield return null;
    }

    IEnumerator IncreaseComboValueCoroutine() {
        modifiedFontSize = Mathf.FloorToInt(CurrentMultiplierFontSize * fontSizeIncreaseFactor);
        multiplierDisplay.rectTransform.localScale = CurrentMultiplierScale * Vector2.one;
        yield return new WaitForSeconds(0.5f);

        float duration = 0.2f;
        DOTween.To(() => modifiedFontSize, x => modifiedFontSize = x, initialMultiplierFontSize, duration);
        multiplierDisplay.rectTransform.DOScale(initialMultiplierScale, duration);
        yield return new WaitForSeconds(duration);
             
        multiplierDisplay.fontSize = initialMultiplierFontSize;
        yield return null;
    }
}
