using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LevelGenerator myTarget = target as LevelGenerator;

        if (GUILayout.Button("Resize level"))
        {
            myTarget.SetupWallsAndFloor();
        }
    }
}
