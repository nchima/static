using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Level Set")]
public class LevelSet : ScriptableObject {
    [HideInInspector] public List<LevelDataListItem> levelDataReferences = new List<LevelDataListItem>();    // Drawn in inspector via editor script
    [HideInInspector] public int levelsCompleted = 0;

    public bool AllLevelsCompleted { get { return levelsCompleted >= levelDataReferences.Count; } }

    public LevelData GetLevelData(int index) {
        return levelDataReferences[index].levelData;
    }
}
