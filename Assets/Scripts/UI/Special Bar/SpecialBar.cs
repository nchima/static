using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialBar : MonoBehaviour {

    [SerializeField] TextMesh percentageText;
    public GameObject fullBar;

    float currentValue;
    public float CurrentValue {
        get {
            return currentValue;
        }

        set {
            value = Mathf.Clamp01(value);
            sizeController.percentageFilled = value;
            currentValue = value;
        }
    }

    [HideInInspector] public bool barIsFull;
    float stickToFullTimer = 0f;    // Used to make the bar stay at full for a certain amount of time.

    [HideInInspector] public bool freezeDecay = false;

    // Colors
    Color fullColor = Color.white;
    Color readyColor1 = new Color(1f, 1f, 0f);
    Color readyColor2 = new Color(1f, 0f, 0f);

    [HideInInspector] public SpecialBarSizeController sizeController;


    private void Awake() {
        sizeController = GetComponentInChildren<SpecialBarSizeController>();
    }


    public void Initialize(SpecialBarManager manager) {
        currentValue = manager.startValue;
    }


    public void Run(SpecialBarManager manager) {

        // If bar is at max, keep it there for a moment to make things easier for the player.
        if (barIsFull) {
            //barBorderObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(readyColor1, readyColor2, Random.value);

            stickToFullTimer += Time.deltaTime;
            if (stickToFullTimer >= manager.stickToFullDuration) {
                //barObject.GetComponent<MeshRenderer>().material.color = Color.black;
                barIsFull = false;
            }
        }
        
        else {
            //if (barBorderObject.GetComponent<MeshRenderer>().material.color != Color.black) barBorderObject.GetComponent<MeshRenderer>().material.color = Color.black;
            //if (barObject.GetComponent<MeshRenderer>().material.color != Color.black) barObject.GetComponent<MeshRenderer>().material.color = Color.black;

            // Apply decay [Disabled to see if it's really necessary]
            //if (Services.gameManager.gameStarted && !freezeDecay) {
            //    currentValue -= manager.decayRate * Time.deltaTime;
            //}
        }

        string newPercentageText = Mathf.CeilToInt(CurrentValue * 100f).ToString() + "%";
        percentageText.text = newPercentageText;
    }
}
