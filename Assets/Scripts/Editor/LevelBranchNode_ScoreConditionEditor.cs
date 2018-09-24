using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelBranchNode_ScoreCondition))]
public class LevelBranchNode_ScoreConditionEditor : LevelBranchNodeEditor {

    private LevelBranchNode_ScoreCondition levelBranchNode { get { return target as LevelBranchNode_ScoreCondition; } }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Tally perfect score value")) {
            levelBranchNode.TallyUpBaseScore();
        }
    }
}
