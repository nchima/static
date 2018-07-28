using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(LevelSet))]
public class LevelSetEditor : Editor {

    ReorderableList levelDataList;
    private LevelSet levelSet {
        get {
            return target as LevelSet;
        }
    }

    private void OnEnable() {
        levelDataList = new ReorderableList(levelSet.levelDataReferences, typeof(LevelData), true, true, true, true);

        levelDataList.drawHeaderCallback += DrawHeader;
        levelDataList.drawElementCallback += DrawElement;
        levelDataList.onAddCallback += AddItem;
        levelDataList.onRemoveCallback += RemoveItem;
    }

    private void OnDisable() {
        levelDataList.drawHeaderCallback -= DrawHeader;
        levelDataList.drawElementCallback -= DrawElement;
        levelDataList.onAddCallback -= AddItem;
        levelDataList.onRemoveCallback -= RemoveItem;
    }

    void DrawHeader(Rect rect) {
        GUI.Label(rect, "Level Data");
    }

    void DrawElement(Rect rect, int index, bool isActive, bool isFocused) {
        LevelDataListItem item = levelSet.levelDataReferences[index];
        EditorGUI.BeginChangeCheck();
        item.levelData = (LevelData)EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, rect.height), "Level " + (index + 1).ToString(), item.levelData, typeof(LevelData), false);
        if (EditorGUI.EndChangeCheck()) { EditorUtility.SetDirty(target); }
    }

    void AddItem(ReorderableList list) {
        levelSet.levelDataReferences.Add(new LevelDataListItem());
        EditorUtility.SetDirty(target);
    }

    void RemoveItem(ReorderableList list) {
        levelSet.levelDataReferences.RemoveAt(list.index);
        EditorUtility.SetDirty(target);
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (levelDataList != null) {
            levelDataList.DoLayoutList();
        }
    }
}
