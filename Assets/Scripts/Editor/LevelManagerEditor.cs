using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        LevelManager m_Target = target as LevelManager;
        if (GUILayout.Button("Unlock all levels")) { m_Target.SetAllLevelsUnlockState(true); }
        if (GUILayout.Button("Lock all levels")) { m_Target.SetAllLevelsUnlockState(false); }
    }
}
