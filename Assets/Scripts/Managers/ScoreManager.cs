using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Steamworks;

public class ScoreManager : MonoBehaviour
{
    // STEAM V LOCAL LEADERBOARDS
    public enum LeaderboardType { Steam, Local }
    public LeaderboardType leaderboardType = LeaderboardType.Steam;

    // USED FOR DISPLAYING THE SCORE
    private int _score = 0;
    public int score
    {
        get {
            return _score;
        }

        set {
            int inputValue = value - _score;
            inputValue = Mathf.RoundToInt(inputValue * multiplier);
            Services.healthManager.ApplyPointsToBonus(inputValue);
            _score += inputValue;
            scoreDisplay.text = _score.ToString();
        }
    }// The player's current score. 
    [SerializeField] private TextMesh scoreDisplay;   // A reference to the TextMesh which displays the score.
    [SerializeField] private TextMesh highScoreDisplay; // A reference to the TextMesh which displays the current high score at the top of the screen.

    [SerializeField] private TextMesh multNumber; // The TextMesh which displays the player's current multiplier.
    float _multiplier = 1f;  // The multiplier that the player starts the game with.
    public float multiplier
    {
        get { return _multiplier; }
        set
        {
            // Update the multiplier number display.
            multNumber.text = multiplier.ToString() + "X";

            _multiplier = value;
        }
    }
    [SerializeField] float multiplierIncreaseValue = 0.1f;
    [SerializeField] float comboTimer = 0f;
    [SerializeField] float comboTime = 1f;

    // HIGH SCORE LIST
    [SerializeField] public List<ScoreEntry> highScoreEntries;
    [SerializeField] Transform highScoreListText;

    // TIME BONUS STUFF
    [SerializeField] GameObject levelCompletedScreen;
    [SerializeField] TextMesh levelCompletedDisplay;
    [SerializeField] TextMesh secondsDisplay;
    [SerializeField] TextMesh bonusScoreDisplay;
    [SerializeField] TextMesh nextLevelDisplay;

    bool bonusTimerIsRunning;
    int maxTimeBonus;
    float maxBonusTime;
    float bonusTimer = 0;
    public int currentTimeBonus
    {
        get {
            return Mathf.Clamp(Mathf.RoundToInt(MyMath.Map(bonusTimer, 0f, maxBonusTime, maxTimeBonus, 0f)), 0, maxTimeBonus);
        }
    }


    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }


    void Start() {
        // Set up high score list.
        highScoreEntries = RetrieveHighScores();

        // Set up the score and multiplier number displays.
        scoreDisplay.text = score.ToString();
        highScoreDisplay.text = GetHighestScore().name + ": " + GetHighestScore().score.ToString();
        multNumber.text = multiplier.ToString() + "X";
    }


    void Update() {
        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0f) {
            multiplier = 1f;
        }

        if (bonusTimerIsRunning)
        {
            bonusTimer += Time.deltaTime;
            //Debug.Log(currentTimeBonus);
        }

        if (Input.GetKeyDown(KeyCode.Minus)) {
            Debug.Log("deleting scores");
            ResetScores();
        }
    }


    public void DetermineBonusTime() {
        maxBonusTime = 0;
        maxTimeBonus = 0;

        // Get all enemies and add their values to max values.
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        for (int i = 0; i < allEnemies.Length; i++) {
            maxTimeBonus += allEnemies[i].scoreKillValue;
            maxBonusTime += allEnemies[i].bonusTimeAdded;
        }

        // Reset bonus timer.
        bonusTimer = 0f;
        bonusTimerIsRunning = true;
    }


    public void PlayerKilledEnemyHandler(GameEvent gameEvent) {
        GameEvents.PlayerKilledEnemy playerKilledEnemyEvent = gameEvent as GameEvents.PlayerKilledEnemy;

        // Round the score to an integer and update the score display.
        score += playerKilledEnemyEvent.scoreValue;

        Services.scorePopupManager.CreatePositionalPopup(playerKilledEnemyEvent.enemyKilled.transform.position, playerKilledEnemyEvent.scoreValue);

        IncreaseMultiplier();
    }


    public void IncreaseMultiplier() {
        multiplier += multiplierIncreaseValue;
        comboTimer = comboTime;
    }


    public void LevelCompletedHandler(GameEvent gameEvent) {
        // Give the player a score boost for beating the level.
        score += currentTimeBonus;
        bonusTimerIsRunning = false;

        ShowLevelCompleteScreen();
    }


    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        if (Services.healthManager.isInvincible) { return; }

        multiplier = 1f;
        comboTimer = 0f;
    }


    void ShowLevelCompleteScreen()
    {
        levelCompletedScreen.SetActive(true);
        levelCompletedDisplay.text = "LEVEL " + Services.levelManager.LevelNumber.ToString() + " COMPLETED";
        secondsDisplay.text = "IN " + (Mathf.Round(bonusTimer * 100f) / 100f).ToString() + " SECONDS!";
        bonusScoreDisplay.text = currentTimeBonus.ToString();
        nextLevelDisplay.text = "NOW ENTERING LEVEL " + (Services.levelManager.LevelNumber + 1).ToString();
    }


    public void HideLevelCompleteScreen()
    {
        levelCompletedScreen.SetActive(false);
    }
    

    /// <summary>
    /// Should be called when a bullet hits an enemy.
    /// </summary>
    public void BulletHitEnemy() {
        score += 1;
    }


    /// <summary>
    /// Loads saved high scores from PlayerPrefs.
    /// </summary>
    /// <returns>List<ScoreEntry></returns>
    List<ScoreEntry> RetrieveHighScores() {
        // Declare a new high score list.
        List<ScoreEntry> newHighScoreList = new List<ScoreEntry>();

        // Initialize 10 empty score entries in the highScore List.
        for (int i = 0; i < 10; i++) {
            newHighScoreList.Add(new ScoreEntry(i + 1, "AAA", 0, 0));
        }

        if (leaderboardType == LeaderboardType.Local) {
            // Load saved high scores from PlayerPrefs.
            for (int i = 0; i < 10; i++) {
                // Check to see if this score entry has previously been saved.
                if (PlayerPrefs.GetString("HighScoreName" + i) != "") {
                    newHighScoreList[i] = new ScoreEntry(i + 1, PlayerPrefs.GetString("HighScoreName" + i), PlayerPrefs.GetInt("HighScoreNumber" + i), PlayerPrefs.GetInt("Newest" + i));
                }
            }
        }

        return newHighScoreList;
    }


    public void UploadAndDownloadScores() {
        StartCoroutine(UploadAndDownloadScoresCoroutine());
    }


    IEnumerator UploadAndDownloadScoresCoroutine() {

        // Upload score
        Services.steamLeaderboardManager.UploadScore(score);

        // Wait until score has been uploaded.
        yield return new WaitUntil(() => {
            if (Services.steamLeaderboardManager.isScoreUploaded) { return true; } 
            else { return false; }
        });

        // Download scores
        Services.steamLeaderboardManager.DownloadLeaderboardEntries(-4, 5);

        // Wait until leaderboard has been downloaded
        yield return new WaitUntil(() => {
            if (Services.steamLeaderboardManager.isScoreEntriesDownloaded) { return true; }
            else { return false; }
        });

        Debug.Log("Uploaded and downloaded score. Displaying.");

        // Use downloaded scores to fill a new list of score entries.
        List<ScoreEntry> newHighScoreList = new List<ScoreEntry>();
        for (int i = 0; i < 10; i++) {
            LeaderboardEntry_t entry = Services.steamLeaderboardManager.GetDownloadedLeaderboardEntry(i);
            if (SteamFriends.GetFriendPersonaName(entry.m_steamIDUser) != "") {
                string username = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);
                bool isUsersScore = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser) == SteamFriends.GetPersonaName();
                int peePee = 0;
                if (isUsersScore) { peePee = 1; }
                ScoreEntry newScoreEntry = new ScoreEntry(entry.m_nGlobalRank, username, entry.m_nScore, peePee);
                newHighScoreList.Add(newScoreEntry);
            }

            // If the score entry was empty.
            else {
                ScoreEntry newScoreEntry = new ScoreEntry(0, "NO DATA", 0, 0);
                newHighScoreList.Add(newScoreEntry);
            }
        }

        // Update list
        highScoreEntries = newHighScoreList;
        highScoreListText.GetComponent<TextMesh>().text = GetScoreListAsString();

        yield return null;
    }


    // Saves high scores
    void SaveHighScores() {
        if (leaderboardType == LeaderboardType.Local) {
            for (int i = 0; i < 10; i++) {
                PlayerPrefs.SetString("HighScoreName" + i, highScoreEntries[i].name);
                PlayerPrefs.SetInt("HighScoreNumber" + i, highScoreEntries[i].score);
                PlayerPrefs.SetInt("Newest" + i, highScoreEntries[i].newest);
            }
        } 
        
        else if (leaderboardType == LeaderboardType.Steam && SteamManager.Initialized) {
            //Services.steamLeaderboardManager.UploadScore(score);
        }
    }


    public void RetrieveScoresForHighScoreScreen() {

        if (leaderboardType == LeaderboardType.Steam) { return; }
        
        highScoreEntries = RetrieveHighScores();

        highScoreListText.GetComponent<TextMesh>().text = GetScoreListAsString();
        SaveHighScores();
    }


    string GetScoreListAsString() {
        string scoreList = "";

        for (int i = 0; i < highScoreEntries.Count; i++) {
            if (highScoreEntries[i].newest == 1) { scoreList += ">"; }
            else { scoreList += " "; }
            scoreList += highScoreEntries[i].rank + ". ";
            scoreList += highScoreEntries[i].name + ": " + highScoreEntries[i].score;
            if (highScoreEntries[i].newest == 1) { scoreList += "<"; }
            scoreList += "\n";

            highScoreEntries[i].newest = 0;
        }

        return scoreList;
    }


    // Insters a score into the list.
    public void InsertScoreLocal(string initials) {
        highScoreEntries = RetrieveHighScores();

        // Add score to list
        highScoreEntries.Add(new ScoreEntry(highScoreEntries.Count+1, initials, score, 1));

        SortScores();

        // Stop showing game over screen and show high score list instead.
        SaveHighScores();
    }


    ScoreEntry GetHighestScore() {
        SortScores();
        return highScoreEntries[0];
    }


    void SortScores() {
        // Add and sort list
        highScoreEntries.Sort(delegate (ScoreEntry b, ScoreEntry a)
        {
            return (a.score.CompareTo(b.score));
        });

        // Remove excess entries
        for (int i = 10; i < highScoreEntries.Count; i++)
        {
            highScoreEntries.RemoveAt(i);
        }
    }


    public void ResetScores() {
        highScoreEntries = RetrieveHighScores();

        for (int i = 0; i < 10; i++)
        {
            highScoreEntries[i].score = 0;
            highScoreEntries[i].name = "AAA";
            highScoreEntries[i].newest = 0;
        }

        PlayerPrefs.DeleteAll();

        SaveHighScores();
    }


    public void PrintHighScores() {
        for (int i = 0; i < 10; i++) {
            if (PlayerPrefs.GetString("HighScoreName" + i) != null) { continue; }
            Debug.Log(PlayerPrefs.GetString("HighScoreName" + i, highScoreEntries[i].name) + ", "
            + PlayerPrefs.GetInt("HighScoreNumber" + i, highScoreEntries[i].score) + ", "
            + PlayerPrefs.GetInt("Newest" + i, highScoreEntries[i].newest));
        }
    }


    public class ScoreEntry
    {
        public string name;
        public int rank;
        public int score;
        public int newest; // Int as bool... 0 = false, 1 = true

        public ScoreEntry(int rank, string name, int score, int newest)
        {
            this.rank = rank;
            this.name = name;
            this.score = score;
            this.newest = newest;
        }
    }
}
