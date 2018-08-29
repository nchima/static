using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialBar : MonoBehaviour {

    [SerializeField] Text percentageText;
    [SerializeField] SpecialBarSizeController bar1SizeController;
    [SerializeField] SpecialBarSizeController bar2SizeController;

    float currentValue;
    public float CurrentValue {
        get {
            return currentValue;
        }

        set {
            if (value >= 1f && !Services.specialBarManager.BothFirstBarsFull) { value = Mathf.Clamp01(value); }
            else { value = Mathf.Clamp(value, 0f, 2f); }
            bar1SizeController.PercentageFilled = Mathf.Clamp01(value);
            bar2SizeController.PercentageFilled = Mathf.Clamp01(value - 1);
            currentValue = value;
        }
    }

    bool barIsFull;
    float stickToFullTimer = 0f;    // Used to make the bar stay at full for a certain amount of time.


    public void Initialize(SpecialBarManager manager) {
        CurrentValue = manager.startValue;
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
