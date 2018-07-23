using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(LevelData))]
[CanEditMultipleObjects]
public class LevelDataEditor : Editor {

	private LevelData levelData { get { return target as LevelData; } }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        EditorStyles.label.wordWrap = true;
        EditorGUILayout.LabelField(levelData.path);
        this.Repaint();

        if (GUILayout.Button("Load Scene")) {
            EditorSceneManager.OpenScene(levelData.path, OpenSceneMode.Additive);
        }
    }
}
