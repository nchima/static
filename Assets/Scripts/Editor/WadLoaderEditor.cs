using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WadLoader))]
public class WadLoaderEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        WadLoader myTarget = target as WadLoader;

        if (GUILayout.Button("Convert Map")) {
            myTarget.ConvertMap();
        }
    }

}
