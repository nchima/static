using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScoreManager))]
public class ScoreManagerEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();  

        ScoreManager myTarget = target as ScoreManager;

        if (GUILayout.Button("Reset High Scores"))
        {
            myTarget.ResetScores();
        }

        if (GUILayout.Button("Print High Scores to Console")) {
            myTarget.PrintHighScores();
        }
    }
}
