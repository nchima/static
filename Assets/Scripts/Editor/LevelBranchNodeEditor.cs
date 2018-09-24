using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelBranchNode))]
public class LevelBranchNodeEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        LevelBranchNode m_Target = target as LevelBranchNode;
        if (GUILayout.Button("Lock Node")) { m_Target.IsUnlocked = false; }
    }
}
