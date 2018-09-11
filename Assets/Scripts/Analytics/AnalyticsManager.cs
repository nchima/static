using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class AnalyticsManager : MonoBehaviour {

    float averageTimer = 0f;
    float AVERAGE_SAMPLE_INTERVAL = 1f;   // How often we sample tuning value for the average.

    // Per level stats
    float timeInLevel; //
    int damageTakenInLevel; //
    List<float> tuningValuesLevel = new List<float>();
    int timesFiredGunInLevel; // Used to calculate average tuning value.
    [HideInInspector] int timesSpecialMoveUsedInLevel;
    int pickupsObtainedInLevel;
    int scoreEarnedInLevel;

    // Total game stats
    float timeAlive;
    int levelDiedOn = 0;
    List<float> tuningValuesTotal = new List<float>();
    int timesFiredGunTotal;   // Used to calculate average tuning value;
    [HideInInspector] int timesSpecialMoveUsedTotal;
    int pickupsObtainedTotal;
    int scoreEarned;
    [HideInInspector] string startingLevelSet;


    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerUsedSpecialMove>(PlayerUsedSpecialMoveHandler);
        GameEventManager.instance.Subscribe<GameEvents.PickupObtained>(PickupObtainedHandler);
        GameEventManager.instance.Subscribe<GameEvents.GameOver>(GameOverHandler);
    }


    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerUsedSpecialMove>(PlayerUsedSpecialMoveHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PickupObtained>(PickupObtainedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.GameOver>(GameOverHandler);
    }


    private void Update() {
        timeAlive += Time.deltaTime;
        timeInLevel += Time.deltaTime;

        if (Services.gameManager.isGameStarted) {
            averageTimer += Time.deltaTime;
            if (averageTimer >= AVERAGE_SAMPLE_INTERVAL) {
                timesFiredGunTotal++;
                tuningValuesLevel.Add(GunValueManager.currentValue);
                tuningValuesTotal.Add(GunValueManager.currentValue);
                averageTimer = 0f;
            }
        }
    }


    void LevelCompletedHandler(GameEvent gameEvent) {
        Analytics.CustomEvent("Level Complete", new Dictionary<string, object> {
            { "Level name", Services.levelManager.currentlyLoadedLevelData.LevelName },
            { "Level number", Services.levelManager.CurrentLevelNumber },
            { "Time spent in level", timeInLevel },
            { "Damage taken in level", damageTakenInLevel },
            { "Average tuning value in level", MyMath.Average(tuningValuesLevel) },
            { "Times special move used in level", timesSpecialMoveUsedInLevel },
            { "Pickups obtained in level", pickupsObtainedInLevel }
        });

        timeInLevel = 0f;
        damageTakenInLevel = 0;
        timesFiredGunInLevel = 0;
        timesSpecialMoveUsedInLevel = 0;
        tuningValuesLevel = new List<float>();
        pickupsObtainedInLevel = 0;
    }

    void PlayerWasHurtHandler(GameEvent gameEvent) {
        damageTakenInLevel++;
    }

    void PlayerUsedSpecialMoveHandler(GameEvent gameEvent) {
        timesSpecialMoveUsedInLevel++;
        timesSpecialMoveUsedTotal++;
    }

    void PickupObtainedHandler(GameEvent gameEvent) {
        pickupsObtainedInLevel++;
        pickupsObtainedTotal++;
    }

    void GameOverHandler(GameEvent gameEvent) {
        Analytics.CustomEvent("Player Died", new Dictionary<string, object> {
            { "Level died on", Services.levelManager.currentlyLoadedLevelData.name },
            { "Starting level set", startingLevelSet },
            { "Levels completed", Services.levelManager.levelsCompleted },
            { "Play time", timeAlive },
            { "Average tuning value", MyMath.Average(tuningValuesTotal) },
            { "Times special move used in level", timesSpecialMoveUsedTotal },
            { "Pickups obtained", pickupsObtainedTotal }
        });
    }
}
