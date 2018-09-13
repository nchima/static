using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NowEnteringScreen : MonoBehaviour {

    [SerializeField] Text nodeNameText;
    [SerializeField] Text levelNumberText;

    public void UpdateText() {
        nodeNameText.text = "\"" + Services.levelManager.currentLevelSet.Name.ToUpper() + "\"";
        levelNumberText.text = "LEVEL " + (Services.levelManager.currentLevelSet.levelsCompleted + 1).ToString();
    }
}
