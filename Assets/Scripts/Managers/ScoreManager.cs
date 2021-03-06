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
    // STEAM OR LOCAL LEADERBOARDS
    public enum LeaderboardType { Steam, Local }
    public LeaderboardType leaderboardType = LeaderboardType.Steam;

    // USED FOR DISPLAYING THE SCORE
    private int score = 0;
    public int Score
    {
        get {
            return score;
        }

        set {
            int inputValue = value - score;
            inputValue = Mathf.RoundToInt(inputValue * Multiplier);
            Services.healthManager.ApplyPointsToBonus(inputValue);
            score += inputValue;
            scoreDisplay.text = TextUtil.AddZerosToBeginningOfNumber(Score, 9);
        }
    }// The player's current score. 
    [SerializeField] private Text scoreDisplay;   // A reference to the TextMesh which displays the score.
    [SerializeField] private Text highScoreDisplay; // A reference to the TextMesh which displays the current high score at the top of the screen.

    [SerializeField] private TextMesh multNumber; // The TextMesh which displays the player's current multiplier.
    float multiplier = 1f;  // The multiplier that the player starts the game with.
    public float Multiplier {
        get { return multiplier; }
        set
        {
            // Update the multiplier number display.
            multNumber.text = Multiplier.ToString() + "X";

            multiplier = value;
        }
    }
    [SerializeField] float multiplierIncreaseValue = 0.1f;
    [SerializeField] float comboTimer = 0f;
    [SerializeField] float comboTime = 1f;

    // HIGH SCORE LIST
    [SerializeField] public List<ScoreEntry> highScoreEntries;
    [SerializeField] Transform highScoreListText;

    // TIME BONUS STUFF
    bool bonusTimerIsRunning;
    int maxTimeBonus;
    float maxBonusTime;
    float bonusTimer = 0;
    public int CurrentTimeBonus
    {
        get {
            return Mathf.Clamp(Mathf.RoundToInt(MyMath.Map(bonusTimer, 0f, maxBonusTime, maxTimeBonus, 0f)), 0, maxTimeBonus);
        }
    }


    private void OnEnable() {
        //GameEventManager.instance.Subscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }

    private void OnDisable() {
        //GameEventManager.instance.Unsubscribe<GameEvents.PlayerKilledEnemy>(PlayerKilledEnemyHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }

    void Start() {
        // Set up high score list.
        highScoreEntries = RetrieveHighScores();

        // Set up the score and multiplier number displays.
        Score = 0;
        multNumber.text = Multiplier.ToString() + "X";
    }

    void Update() {
        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0f) {
            Multiplier = 1f;
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

    public void UpdateHighScoreDisplay() {
        if (leaderboardType == LeaderboardType.Local) {
            highScoreDisplay.text = GetHighestScore().name + ": " + TextUtil.AddZerosToBeginningOfNumber(GetHighestScore().score, 9);
        } else if (leaderboardType == LeaderboardType.Steam) {
            StartCoroutine(UpdateHighScoreDisplayCoroutine());
        }
    }

    IEnumerator UpdateHighScoreDisplayCoroutine() {
        Services.steamLeaderboardManager.DownloadLeaderboardEntries(0, 0);

        // Wait until leaderboard has been downloaded
        yield return new WaitUntil(() => {
            if (Services.steamLeaderboardManager.isScoreEntriesDownloaded) { return true; } else { return false; }
        });

        highScoreDisplay.text = TextUtil.AddZerosToBeginningOfNumber(Services.steamLeaderboardManager.GetDownloadedLeaderboardEntry(0).m_nScore, 9);

        yield return null;
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

    //public void PlayerKilledEnemyHandler(GameEvent gameEvent) {
    //    GameEvents.PlayerKilledEnemy playerKilledEnemyEvent = gameEvent as GameEvents.PlayerKilledEnemy;

    //    // Round the score to an integer and update the score display.
    //    Score += playerKilledEnemyEvent.scoreValue;

    //    Services.scorePopupManager.CreatePositionalPopup(playerKilledEnemyEvent.enemyKilled.transform.position, playerKilledEnemyEvent.scoreValue);

    //    IncreaseMultiplier();
    //}

    public void IncreaseMultiplier() {
        Multiplier += multiplierIncreaseValue;
        comboTimer = comboTime;
    }

    public void LevelCompletedHandler(GameEvent gameEvent) {
        bonusTimerIsRunning = false;
    }

    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        if (Services.healthManager.isInvincible) { return; }

        Multiplier = 1f;
        comboTimer = 0f;
    }

    /// <summary>
    /// Should be called when a bullet hits an enemy.
    /// </summary>
    public void BulletHitEnemy() {
        Score += 1;
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

        yield return new WaitUntil(() => {
            if (Services.steamLeaderboardManager.isLeaderboardInitialized) { return true; }
            else { return false; }
        });

        // Upload score
        Services.steamLeaderboardManager.UploadScore(Score);

        // Wait until score has been uploaded.
        yield return new WaitUntil(() => {
            if (Services.steamLeaderboardManager.isScoreUploaded) { return true; } 
            else { return false; }
        });

        StartCoroutine(DownloadScores());

        yield return null;
    }

    IEnumerator DownloadScores() {
        // Download scores
        Services.steamLeaderboardManager.DownloadLeaderboardEntries(-4, 5);

        // Wait until leaderboard has been downloaded
        yield return new WaitUntil(() => {
            if (Services.steamLeaderboardManager.isScoreEntriesDownloaded) { return true; } else { return false; }
        });

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
        highScoreListText.GetComponent<Text>().text = GetScoreListAsString();
        
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
        if (leaderboardType == LeaderboardType.Steam) {
            StartCoroutine(DownloadScores());
            return;
        }

        Debug.Log("retrieving local scores");
        highScoreEntries = RetrieveHighScores();

        highScoreListText.GetComponent<Text>().text = GetScoreListAsString();
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
        highScoreEntries.Add(new ScoreEntry(highScoreEntries.Count+1, initials, Score, 1));

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

    public class ScoreEntry {
        public string name;
        public int rank;
        public int score;
        public int newest; // Int as bool... 0 = false, 1 = true

        public ScoreEntry(int rank, string name, int score, int newest) {
            this.rank = rank;
            this.name = name;
            this.score = score;
            this.newest = newest;
        }
    }
}
