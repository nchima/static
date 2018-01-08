using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class ScoreManager : MonoBehaviour
{
    // USED FOR DISPLAYING THE SCORE
    [SerializeField] private int _score = 0;
    public int score
    {
        get
        {
            return _score;
        }

        set
        {
            int inputValue = value - _score;
            inputValue = Mathf.RoundToInt(inputValue * multiplier);
            _score += inputValue;
            scoreDisplay.text = _score.ToString();
        }
    }// The player's current score. 
    [SerializeField] private TextMesh scoreDisplay;   // A reference to the TextMesh which displays the score.
    [SerializeField] private TextMesh highScoreDisplay; // A reference to the TextMesh which displays the current high score at the top of the screen.

    [SerializeField] private TextMesh multNumber; // The TextMesh which displays the player's current multiplier.
    float _multiplier = 1f;  // The multiplier that the player starts the game with.
    float multiplier
    {
        get { return _multiplier; }
        set
        {
            // Update the multiplier number display.
            multNumber.text = multiplier.ToString() + "X";

            _multiplier = value;
        }
    }

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


    void Start() {
        // Set up high score list.
        highScoreEntries = RetrieveHighScores();

        // Set up the score and multiplier number displays.
        scoreDisplay.text = score.ToString();
        highScoreDisplay.text = GetHighestScore().initials + ": " + GetHighestScore().score.ToString();
        multNumber.text = multiplier.ToString() + "X";
    }


    void Update()
    {
        // If the multiplier bar value has gone below zero, lower the player's multiplier.
        //if (multBarValueCurr <= 0f && multiplier > 1f)
        //{
        //    // Lower the multiplier.
        //    multiplier -= 0.1f;

        //    // Get the values for the new multiplier level.
        //    multBarStartValCurr = multBarStartVal / multiplier;
        //    multBarValueCurr = multBarStartValCurr;
        //    multBarDecayCurr = multBarBaseDecay * multiplier;
        //    multNumber.text = multiplier.ToString() + "X";
        //}

        //multiplierBar.transform.DOLocalMoveY(newYPos, multBarTweenSpeed);

        // Set multiplier bar value based on it's current size.
        //if (specialBar.transform.localScale.y > 6f) multiplier = 6;
        //else if (specialBar.transform.localScale.y > 4.4f) multiplier = 5;
        //else if (specialBar.transform.localScale.y > 2.6f) multiplier = 4;
        //else if (specialBar.transform.localScale.y > 1.35f) multiplier = 3;
        //else if (specialBar.transform.localScale.y > 0.5f) multiplier = 2;
        //else multiplier = 1;

        if (bonusTimerIsRunning)
        {
            bonusTimer += Time.deltaTime;
            //Debug.Log(currentTimeBonus);
        }
    }


    public void DetermineBonusTime()
    {
        maxBonusTime = 0;
        maxTimeBonus = 0;

        // Get all enemies and add their values to max values.
        EnemyOld[] allEnemies = FindObjectsOfType<EnemyOld>();
        for (int i = 0; i < allEnemies.Length; i++)
        {
            maxTimeBonus += allEnemies[i].killValue;
            maxBonusTime += allEnemies[i].bonusTimeAdded;
        }

        // Reset bonus timer.
        bonusTimer = 0f;
        bonusTimerIsRunning = true;
    }


    /// <summary>
    /// Should be called when the player kills an enemy.
    /// </summary>
    public void PlayerKilledEnemy(int enemyKillValue)
    {
        // If value of the multiplier bar has gotten to 1, raise the player's multiplier and set the multiplier bar values for the new multiplier level.
        //if (multBarValueCurr >= 1f)
        //{
        //    multiplier += 0.1f;
        //    multBarStartValCurr = multBarStartVal / multiplier;
        //    multBarValueCurr = multBarStartValCurr;
        //    multBarDecayCurr = multBarBaseDecay * multiplier;
        //}

        // Round the score to an integer and update the score display.
        score += enemyKillValue;
    }


    /// <summary>
    /// Is called when player beats the current level.
    /// </summary>
    public void LevelComplete()
    {
        // Give the player a score boost for beating the level.
        score += currentTimeBonus;
        bonusTimerIsRunning = false;

        ShowLevelCompleteScreen();
    }


    void ShowLevelCompleteScreen()
    {
        levelCompletedScreen.SetActive(true);
        levelCompletedDisplay.text = "LEVEL " + GameManager.levelManager.currentLevelNumber.ToString() + " COMPLETED";
        secondsDisplay.text = "IN " + (Mathf.Round(bonusTimer * 100f) / 100f).ToString() + " SECONDS!";
        bonusScoreDisplay.text = currentTimeBonus.ToString();
        nextLevelDisplay.text = "NOW ENTERING LEVEL " + (GameManager.levelManager.currentLevelNumber + 1).ToString();
    }


    public void HideLevelCompleteScreen()
    {
        levelCompletedScreen.SetActive(false);
    }
    

    /// <summary>
    /// Should be called when a bullet hits an enemy.
    /// </summary>
    public void BulletHitEnemy()
    {
        score += 1;
    }


    /// <summary>
    /// Should be called when the player gets hurt.
    /// </summary>
    public void GetHurt()
    {  
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
            newHighScoreList.Add(new ScoreEntry("AAA", 0, 0));
        }

        // Load saved high scores from PlayerPrefs.
        for (int i = 0; i < 10; i++) {
            // Check to see if this score entry has previously been saved.
            if (PlayerPrefs.GetString("HighScoreName" + i) != "") {
                newHighScoreList[i] = new ScoreEntry(PlayerPrefs.GetString("HighScoreName" + i), PlayerPrefs.GetInt("HighScoreNumber" + i), PlayerPrefs.GetInt("Newest" + i));
            }
        }

        return newHighScoreList;
    }


    // Saves high scores
    void SaveHighScores()
    {
        for (int i = 0; i < 10; i++) {
            PlayerPrefs.SetString("HighScoreName" + i, highScoreEntries[i].initials);
            PlayerPrefs.SetInt("HighScoreNumber" + i, highScoreEntries[i].score);
            PlayerPrefs.SetInt("Newest" + i, highScoreEntries[i].newest);
        }
    }


    public void RetrieveScoresForHighScoreScreen() {
        highScoreEntries = RetrieveHighScores();

        string scoreList = "";

        for (int i = 0; i < highScoreEntries.Count; i++) {
            if (highScoreEntries[i].newest == 1) { Debug.Log("adding newest score"); scoreList += ">"; }
            scoreList += highScoreEntries[i].initials + ": " + highScoreEntries[i].score;
            if (highScoreEntries[i].newest == 1) { scoreList += "<"; }
            scoreList += "\n";

            highScoreEntries[i].newest = 0;
        }

        highScoreListText.GetComponent<TextMesh>().text = scoreList;
        SaveHighScores();
    }


    // Insters a score into the list.
    public void InsertScore(string initials)
    {
        highScoreEntries = RetrieveHighScores();

        // Add score to list
        highScoreEntries.Add(new ScoreEntry(initials, score, 1));

        SortScores();

        // Stop showing game over screen and show high score list instead.
        SaveHighScores();
    }


    ScoreEntry GetHighestScore()
    {
        SortScores();
        return highScoreEntries[0];
    }


    void SortScores()
    {
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


    public void ResetScores()
    {
        highScoreEntries = RetrieveHighScores();

        for (int i = 0; i < 10; i++)
        {
            highScoreEntries[i].score = 0;
            highScoreEntries[i].initials = "AAA";
            highScoreEntries[i].newest = 0;
        }

        PlayerPrefs.DeleteAll();

        SaveHighScores();
    }


    public void PrintHighScores() {
        for (int i = 0; i < 10; i++) {
            Debug.Log(PlayerPrefs.GetString("HighScoreName" + i, highScoreEntries[i].initials) + ", "
            + PlayerPrefs.GetInt("HighScoreNumber" + i, highScoreEntries[i].score) + ", "
            + PlayerPrefs.GetInt("Newest" + i, highScoreEntries[i].newest));
        }
    }


    public class ScoreEntry
    {
        public string initials;
        public int score;
        public int newest; // Int as bool... 0 = false, 1 = true

        public ScoreEntry(string initials, int score, int newest)
        {
            this.initials = initials;
            this.score = score;
            this.newest = newest;
        }
    }
}
