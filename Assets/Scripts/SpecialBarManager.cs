using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpecialBarManager : MonoBehaviour {

    // VISUAL STUFF
    [SerializeField] public GameObject barObject;    // A reference to the special bar GameObject.
    [SerializeField] GameObject barBorderObject;
    [SerializeField] public float barSizeMin = 0.03f;    // The special bar's smallest size.
    [SerializeField] private float barSizeMax = 7.15f;    // The special bar's largets size.
    [SerializeField] private float bottomPosition = -3f;   // The position of the special bar's lowest point.
    [SerializeField] private float tweenSpeed = 0.4f;    // How quickly the special bar changes size.

    // GAMEPLAY STUFF
    [SerializeField] private float startValue = 0.4f;    // How large of a special the player starts the game with.
    [SerializeField] private float baseDecay = 0.01f;  // How quickly the special bar shrinks (increases as the player's multiplier increases)

    [HideInInspector]
    float _currentValue;
    [HideInInspector] public float currentValue
    {
        get
        {
            return _currentValue;
        }

        set
        {
            value = Mathf.Clamp(value, 0f, 1f);
            if (value == 1f)
            {
                barObject.GetComponent<Material>().DOColor(readyColor1, 0.1f);
                barIsFull = true;
                stickToFullTimer = 0f;
            }

            else if (value == 0f)
            {
                barObject.GetComponent<Material>().DOColor(Color.black, 0.1f);
                barIsFull = false;
            }

            _currentValue = value;
        }
    }// The current size of the special bar (as percentage of it's max size).
    [HideInInspector] public bool freezeDecay = false;
    float currentDecay;     // How quickly the special bar currently shrinks.
    float currentStartValue;  // Where the special bar starts at the player's current multiplier (decreases as the multiplier increases).
    Color fullColor = Color.white;
    Color readyColor1 = new Color(1f, 1f, 0f);
    Color readyColor2 = new Color(1f, 0f, 0f);

    [HideInInspector] public bool barIsFull;
    [SerializeField] float stickToFullDuration = 2f;   // How long the bar sticks to it's maximum value.
    float stickToFullTimer = 0f;

    [SerializeField] private float enemyValue = 0.4f; // How much the player's special increases for killing an enemy.
    [SerializeField] private float bulletHitValue = 0.01f;    // How much the player's special increases for hitting an enemy with one bullet.
    [SerializeField] private float getHurtPenalty = 0.1f; // How much the special bar decreases when the player is hurt.


    private void Start()
    {
        currentValue = startValue;
        currentDecay = baseDecay;
        currentStartValue = startValue;
        barObject.transform.localScale = new Vector3(
            barObject.transform.localScale.x,
            MyMath.Map(currentValue, 0f, 1f, barSizeMin, barSizeMax),
            barObject.transform.localScale.z
        );
    }


    private void Update()
    {
        // If bar is at max, keep it there for a moment to make things easier for the player.
        if (barIsFull)
        {
            barBorderObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(readyColor1, readyColor2, Random.value);

            stickToFullTimer += Time.deltaTime;
            if (stickToFullTimer >= stickToFullDuration)
            {
                barObject.GetComponent<MeshRenderer>().material.color = Color.black;
                barIsFull = false;
            }
        }

        else
        {
            if (barBorderObject.GetComponent<MeshRenderer>().material.color != Color.black) barBorderObject.GetComponent<MeshRenderer>().material.color = Color.black;
            if (barObject.GetComponent<MeshRenderer>().material.color != Color.black) barObject.GetComponent<MeshRenderer>().material.color = Color.black;

            // Apply decay
            if (FindObjectOfType<GameManager>().gameStarted && !freezeDecay)
            {
                currentValue -= currentDecay * Time.deltaTime;
                currentValue = Mathf.Clamp(currentValue, 0f, 1f);
            }
        }

        // Update the bar's size and position
        float newYScale = MyMath.Map(currentValue, 0f, 1f, barSizeMin, barSizeMax);
        barObject.transform.DOScaleY(newYScale, tweenSpeed);

        float newYPos = bottomPosition + barObject.transform.localScale.y * 0.5f;
        barObject.transform.localPosition = new Vector3(
                barObject.transform.localPosition.x,
                newYPos,
                barObject.transform.localPosition.z
            );
    }


    public void FlashBar()
    {
        barObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(readyColor1, readyColor2, Random.Range(0f, 1f));
    }


    public void PlayerKilledEnemy()
    {
        // Increase the value of the multiplier bar.
        currentValue += enemyValue;
    }


    public void BulletHitEnemy()
    {
        currentValue += bulletHitValue;
    }


    public void PlayerWasHurt()
    {
        currentValue -= getHurtPenalty;
    }


    public void PlayerUsedSpecialMove()
    {
        currentValue = 0f;
    }
}
