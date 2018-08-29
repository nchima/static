using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ComboManager : MonoBehaviour {

    // References
    [SerializeField] Text textDisplay;
    [SerializeField] Text scoreDisplay;
    [SerializeField] Text multiplierDisplay;
    [SerializeField] GameObject timerBar;
    [SerializeField] Transform timerBarReference;
    [SerializeField] float fontSizeIncreaseFactor = 2f;
    [SerializeField] GameObject comboFinisherPrefab;
    
    //[SerializeField] FloatRange timerBarLengthRange = new FloatRange(0f, 5f);

    enum State { ComboActive, ComboIdle }
    State state = State.ComboIdle;

    // Timer
    const float MAX_COMBO_TIME = 5f;
    float comboTimer = 0f;

    const float KILL_BONUS_TIME = 1f;
    const float LEVEL_COMPLETED_BONUS_TIME = 1f;
    const float PICKUP_OBTAINED_BONUS_TIME = 2f;
    const float MISC_BONUS_TIME = 1f;

    // Enemy kill values.
    const int SIMPLE_ENEMY_SCORE_VALUE = 100;
    const int MELEE_ENEMY_SCORE_VALUE = 150;
    const int LASER_ENEMY_SCORE_VALUE = 200;
    const int TANK_ENEMY_KILL_VALUE = 200;
    const int HOVER_ENEMY_KILL_VALUE = 175;
    const int BOSS_ENEMY_KILL_VALUE = 500;
    const int LEVEL_COMPLETE_VALUE = 500;

    // Other values
    const int PICKUP_SCORE_VALUE = 100;
    const int BULLSEYE_VALUE = 200;
    const int SPECIAL_MOVE_VALUE = 50;

    // Current combo data.
    int simpleEnemiesKilled = 0;
    int meleeEnemiesKilled = 0;
    int laserEnemiesKilled = 0;
    int tankEnemiesKilled = 0;
    int hoverEnemiesKilled = 0;
    int bossEnemiesKilled = 0;
    int pickupsObtained = 0;
    int levelsCompleted = 0;
    int bullseyes = 0;
    int timesSpecialMoveUsed = 0;

    int initialMultiplierFontSize;
    float modifiedFontSize;
    int CurrentMultiplierFontSize { get { return Mathf.FloorToInt(MyMath.Map(CurrentMultiplier, 0, 400, initialMultiplierFontSize * 1.2f, 2000)); } }
    float initialMultiplierScale;
    float CurrentMultiplierScale { get { return MyMath.Map(CurrentMultiplier, 0, 400, initialMultiplierScale * 1.2f, 0.027f); } }

    int CurrentMultiplier { get { return simpleEnemiesKilled + meleeEnemiesKilled + laserEnemiesKilled + tankEnemiesKilled + hoverEnemiesKilled + bossEnemiesKilled + bullseyes + pickupsObtained + timesSpecialMoveUsed + levelsCompleted + 1; } }

    Coroutine increaseComboValueCoroutine;

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PickupObtained>(PickupObtainedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerUsedSpecialMove>(PlayerUsedSpecialMoveHandler);
        GameEventManager.instance.Subscribe<GameEvents.Bullseye>(BullseyeHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameOver>(GameOverHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PickupObtained>(PickupObtainedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerUsedSpecialMove>(PlayerUsedSpecialMoveHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.Bullseye>(BullseyeHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameOver>(GameOverHandler);
    }

    private void Awake() {
        initialMultiplierFontSize = multiplierDisplay.fontSize;
        modifiedFontSize = initialMultiplierFontSize;
        initialMultiplierScale = multiplierDisplay.rectTransform.localScale.x;

        textDisplay.text = "";
        multiplierDisplay.text = "";
        scoreDisplay.text = "";
        timerBar.SetActive(false);
    }

    private void Update() {
        if (state == State.ComboActive) {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0) {
                EndCombo();
                return;
            }

            Vector3 newTimerBarScale = timerBar.transform.localScale;
            newTimerBarScale.x = MyMath.Map(comboTimer, 0f, MAX_COMBO_TIME, 0.001f, timerBarReference.localScale.x);
            timerBar.transform.localScale = newTimerBarScale;

            Vector3 newTimerBarPosition = timerBar.transform.localPosition;
            newTimerBarPosition.x = MyMath.Map(comboTimer, 0f, MAX_COMBO_TIME, timerBarReference.localPosition.x - timerBarReference.localScale.x * 0.5f, timerBarReference.localPosition.x);
            timerBar.transform.localPosition = newTimerBarPosition;

            textDisplay.text = GenerateComboTextString();
            scoreDisplay.text = GetUnmultipliedTotal().ToString();
            multiplierDisplay.text = "X" + CurrentMultiplier.ToString();
        }
        
        // Update size of multiplier display
        multiplierDisplay.fontSize = Mathf.FloorToInt(modifiedFontSize);

        // Move the multiplier display the correct distance from the score display
        Vector3 newPosition = scoreDisplay.transform.localPosition;
        newPosition.y = multiplierDisplay.rectTransform.localPosition.y;
        newPosition.x += scoreDisplay.rectTransform.sizeDelta.x * scoreDisplay.rectTransform.lossyScale.x + 0.05f;
        multiplierDisplay.rectTransform.localPosition = newPosition;
    }

    private void StartCombo() {
        comboTimer = MAX_COMBO_TIME;
        timerBar.SetActive(true);
        state = State.ComboActive;
    }

    private void EndCombo() {
        Debug.Log("ending combo");
        ComboFinisher comboFinisher = Instantiate(comboFinisherPrefab, scoreDisplay.transform.parent).GetComponent<ComboFinisher>();
        comboFinisher.transform.position = scoreDisplay.transform.position;
        comboFinisher.Initialize(GetMultipliedTotal(), CurrentMultiplierFontSize);

        simpleEnemiesKilled = 0;
        meleeEnemiesKilled = 0;
        laserEnemiesKilled = 0;
        tankEnemiesKilled = 0;
        hoverEnemiesKilled = 0;
        bossEnemiesKilled = 0;
        levelsCompleted = 0;
        pickupsObtained = 0;
        bullseyes = 0;
        timesSpecialMoveUsed = 0;

        textDisplay.text = "";
        scoreDisplay.text = "";
        multiplierDisplay.text = "";

        timerBar.SetActive(false);

        state = State.ComboIdle;
    }

    private void StartIncreaseComboValueCoroutine() {
        if (increaseComboValueCoroutine != null) { StopCoroutine(increaseComboValueCoroutine); }
        increaseComboValueCoroutine = StartCoroutine(IncreaseComboValueCoroutine());
    }

    private int GetUnmultipliedTotal() {
        int totalScore = 0;
        totalScore += simpleEnemiesKilled * SIMPLE_ENEMY_SCORE_VALUE;
        totalScore += meleeEnemiesKilled * MELEE_ENEMY_SCORE_VALUE;
        totalScore += laserEnemiesKilled * LASER_ENEMY_SCORE_VALUE;
        totalScore += tankEnemiesKilled * TANK_ENEMY_KILL_VALUE;
        totalScore += hoverEnemiesKilled * HOVER_ENEMY_KILL_VALUE;
        totalScore += bossEnemiesKilled * BOSS_ENEMY_KILL_VALUE;
        totalScore += levelsCompleted * LEVEL_COMPLETE_VALUE;
        totalScore += pickupsObtained * PICKUP_SCORE_VALUE;
        totalScore += timesSpecialMoveUsed * SPECIAL_MOVE_VALUE;
        totalScore += bullseyes * BULLSEYE_VALUE;
        return totalScore;
    }

    private int GetMultipliedTotal() {
        return GetUnmultipliedTotal() * CurrentMultiplier;
    }

    private string GenerateComboTextString() {
        string comboString = "";

        if (bullseyes > 0) {
            if (comboString != "") { comboString += " + "; }
            comboString += "Bullseye! x" + bullseyes.ToString();
        }

        if (simpleEnemiesKilled > 0) {
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

    public void PlayerKilledEnemyHandler(GameEvent gameEvent) {
        GameEvents.PlayerKilledEnemy playerKilledEnemyEvent = gameEvent as GameEvents.PlayerKilledEnemy;

        if (state == State.ComboIdle) { StartCombo(); }
        else { comboTimer = Mathf.Clamp(comboTimer + KILL_BONUS_TIME, 0f, MAX_COMBO_TIME); }

        if (playerKilledEnemyEvent.enemyKilled is SimpleEnemy) { simpleEnemiesKilled++; } 
        else if (playerKilledEnemyEvent.enemyKilled is MeleeEnemy) { meleeEnemiesKilled++; }
        else if (playerKilledEnemyEvent.enemyKilled is LaserEnemy) { laserEnemiesKilled++; }
        else if (playerKilledEnemyEvent.enemyKilled is TankEnemy) { tankEnemiesKilled++; }
        else if (playerKilledEnemyEvent.enemyKilled is HoveringEnemy) { hoverEnemiesKilled++; }
        else if (playerKilledEnemyEvent.enemyKilled is SnailEnemy) { bossEnemiesKilled++; }

        StartIncreaseComboValueCoroutine();
    }

    public void LevelCompletedHandler(GameEvent gameEvent) {
        if (state == State.ComboIdle) { StartCombo(); } else { comboTimer = Mathf.Clamp(comboTimer + LEVEL_COMPLETED_BONUS_TIME, 0f, MAX_COMBO_TIME); }

        levelsCompleted++;

        StartIncreaseComboValueCoroutine();
    }

    public void PickupObtainedHandler(GameEvent gameEvent) {
        if (state == State.ComboIdle) { StartCombo(); } else { comboTimer = Mathf.Clamp(comboTimer + PICKUP_OBTAINED_BONUS_TIME, 0f, MAX_COMBO_TIME); }

        pickupsObtained++;

        StartIncreaseComboValueCoroutine();
    }

    public void PlayerUsedSpecialMoveHandler(GameEvent gameEvent) {
        if (state == State.ComboIdle) { StartCombo(); } else { comboTimer = Mathf.Clamp(comboTimer + MISC_BONUS_TIME, 0f, MAX_COMBO_TIME); }

        timesSpecialMoveUsed++;

        StartIncreaseComboValueCoroutine();
    }

    public void BullseyeHandler(GameEvent gameEvent) {
        if (state == State.ComboIdle) { StartCombo(); } else { comboTimer = Mathf.Clamp(comboTimer + MISC_BONUS_TIME, 0f, MAX_COMBO_TIME); }

        bullseyes++;

        StartIncreaseComboValueCoroutine();
    }

    public void GameOverHandler(GameEvent gameEvent) {
        EndCombo();

        StartIncreaseComboValueCoroutine();
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
