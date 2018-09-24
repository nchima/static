using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelBranchNode), true)]
public class LevelBranchNodeEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        LevelBranchNode m_Target = target as LevelBranchNode;
        if (GUILayout.Button("Unlock Node")) { m_Target.IsUnlocked = true; }
        if (GUILayout.Button("Lock Node")) { m_Target.IsUnlocked = false; }
    }
}
