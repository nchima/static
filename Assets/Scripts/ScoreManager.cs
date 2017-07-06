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

    // USED FOR THE MULTIPLIER BAR
    [SerializeField] public GameObject multiplierBar;    // A reference to the multiplier bar GameObject.
    [SerializeField] private float multBarStartVal = 0.4f;    // How large of a multiplier the player starts the game with.
    [SerializeField] private float multBarBaseDecay = 0.01f;  // How quickly the multiplier bar shrinks (increases as the player's multiplier increases)

    [SerializeField] public float multBarSizeMin = 0.03f;    // The multiplier bar's smallest size.
    [SerializeField] private float multBarSizeMax = 7.15f;    // The multiplier bar's largets size.
    [SerializeField] private float multBarBottomPos = -2.92f;   // The position of the multiplier bar's lowest point.
    [SerializeField] private float multBarTweenSpeed = 0.4f;    // How quickly the multiplier bar changes size.

    [HideInInspector] public float multBarValueCurr;   // The current size of the multiplier bar (as percentage of it's max size).
    float multBarDecayCurr;     // How quickly the multiplier bar currently shrinks.
    float multBarStartValCurr;  // Where the multiplier bar starts at the player's current multiplier (decreases as the multiplier increases).

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

    // SCORE/MULTIPLIER VALUE OF VARIOUS THINGS
    [SerializeField] private int enemyScoreValue = 1000;  // How many points the player gets for killing an enemy (without multiplier applied)
    [SerializeField] private int levelWinScoreValue = 3000;
    [SerializeField] private float enemyMultValue = 0.5f; // How much the player's multiplier increases for killing an enemy.
    [SerializeField] private float bulletHitValue = 0.01f;    // How much the player's multiplier increases for hitting an enemy with one bullet.
    [SerializeField] private float getHurtPenalty = 0.1f; // How much the multiplier bar decreases when the player is hurt.


    void Start()
    {
        // Set up the multiplier bar.
        multBarValueCurr = multBarStartVal;
        multBarDecayCurr = multBarBaseDecay;
        multBarStartValCurr = multBarStartVal;
        multiplierBar.transform.localScale = new Vector3(
            multiplierBar.transform.localScale.x,
            MyMath.Map(multBarValueCurr, 0f, 1f, multBarSizeMin, multBarSizeMax),
            multiplierBar.transform.localScale.z
        );

        // Set up high score list.
        highScoreEntries = LoadHighScores();

        // Set up the score and multiplier number displays.
        scoreDisplay.text = score.ToString();
        highScoreDisplay.text = GetHighestScore().initials + ": " + GetHighestScore().score.ToString();
        multNumber.text = multiplier.ToString() + "X";
    }


    void Update()
    {
        // Apply decay to the multiplier bar
        if (FindObjectOfType<GameManager>().gameStarted)
        {
            multBarValueCurr -= multBarDecayCurr * Time.deltaTime;
            multBarValueCurr = Mathf.Clamp(multBarValueCurr, 0f, 1f);
        }

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

        // Update the size and position of the multiplier bar.
        float newYScale = MyMath.Map(multBarValueCurr, 0f, 1f, multBarSizeMin, multBarSizeMax);
        multiplierBar.transform.DOScaleY(newYScale, multBarTweenSpeed);

        float newYPos = multBarBottomPos + multiplierBar.transform.localScale.y * 0.5f;
        multiplierBar.transform.localPosition = new Vector3(
                multiplierBar.transform.localPosition.x,
                newYPos,
                multiplierBar.transform.localPosition.z
            );
        //multiplierBar.transform.DOLocalMoveY(newYPos, multBarTweenSpeed);

        // Set multiplier bar value based on it's current size.
        if (multiplierBar.transform.localScale.y > 6f) multiplier = 6;
        else if (multiplierBar.transform.localScale.y > 4.4f) multiplier = 5;
        else if (multiplierBar.transform.localScale.y > 2.6f) multiplier = 4;
        else if (multiplierBar.transform.localScale.y > 1.35f) multiplier = 3;
        else if (multiplierBar.transform.localScale.y > 0.5f) multiplier = 2;
        else multiplier = 1;
    }


    /// <summary>
    /// Should be called when the player kills an enemy.
    /// </summary>
    public void KilledEnemy()
    {
        // Increase the multiplier.
        float multBarIncreaseAmt = enemyMultValue / multiplier;

        // Increase the value of the multiplier bar.
        multBarValueCurr += multBarIncreaseAmt;

        // If value of the multiplier bar has gotten to 1, raise the player's multiplier and set the multiplier bar values for the new multiplier level.
        //if (multBarValueCurr >= 1f)
        //{
        //    multiplier += 0.1f;
        //    multBarStartValCurr = multBarStartVal / multiplier;
        //    multBarValueCurr = multBarStartValCurr;
        //    multBarDecayCurr = multBarBaseDecay * multiplier;
        //}

        // Round the score to an integer and update the score display.
        score += enemyScoreValue;
    }


    /// <summary>
    /// Is called when player beats the current level.
    /// </summary>
    public void LevelBeaten()
    {
        // Give the player a score boost for beating the level.
        score += levelWinScoreValue;
    }
    

    /// <summary>
    /// Should be called when a bullet hits an enemy.
    /// </summary>
    public void BulletHit()
    {
        score += 1;
        multBarValueCurr += bulletHitValue;
    }


    /// <summary>
    /// Should be called when the player gets hurt.
    /// </summary>
    public void GetHurt()
    {  
        multBarValueCurr -= getHurtPenalty;
    }


    /// <summary>
    /// Loads saved high scores from PlayerPrefs.
    /// </summary>
    /// <returns>List<ScoreEntry></returns>
    List<ScoreEntry> LoadHighScores()
    {
        // Declare a new high score list.
        List<ScoreEntry> newHighScoreList = new List<ScoreEntry>();

        // Initialize 10 empty score entries in the highScore List.
        for (int i = 0; i < 10; i++)
        {
            newHighScoreList.Add(new ScoreEntry("AAA", 0));
        }

        // Load saved high scores from PlayerPrefs.
        for (int i = 0; i < 10; i++)
        {
            // Check to see if this score entry has previously been saved.
            if (PlayerPrefs.GetString("HighScoreName" + i) != "")
            {
                newHighScoreList[i] = new ScoreEntry(PlayerPrefs.GetString("HighScoreName" + i), PlayerPrefs.GetInt("HighScoreNumber" + i));
            }
        }

        return newHighScoreList;
    }


    // Saves high scores
    void SaveHighScores()
    {
        for (int i = 0; i < 10; i++)
        {
            PlayerPrefs.SetString("HighScoreName" + i, highScoreEntries[i].initials);
            PlayerPrefs.SetInt("HighScoreNumber" + i, highScoreEntries[i].score);
        }
    }


    public void LoadScoresForHighScoreScreen()
    {
        highScoreEntries = LoadHighScores();

        string scoreList = "";

        for (int i = 0; i < highScoreEntries.Count; i++)
        {
            scoreList += highScoreEntries[i].initials + ": " + highScoreEntries[i].score + "\n";
        }

        highScoreListText.GetComponent<TextMesh>().text = scoreList;
    }


    // Insters a score into the list.
    public void InsertScore(string initials)
    {
        highScoreEntries = LoadHighScores();

        // Add score to list
        highScoreEntries.Add(new ScoreEntry(initials, score));

        SortScores();

        // Stop showing game over screen and show high score list instead.
        SaveHighScores();
        LoadScoresForHighScoreScreen();
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
        highScoreEntries = LoadHighScores();

        for (int i = 0; i < 10; i++)
        {
            highScoreEntries[i].score = 0;
            highScoreEntries[i].initials = "AAA";
        }

        PlayerPrefs.DeleteAll();

        SaveHighScores();
    }


    public class ScoreEntry
    {
        public string initials;
        public int score;

        public ScoreEntry(string _initials, int _score)
        {
            initials = _initials;
            score = _score;
        }
    }
}
