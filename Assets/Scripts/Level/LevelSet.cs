using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Level Set")]
public class LevelSet : ScriptableObject {
    [SerializeField] [HideInInspector] public List<LevelDataListItem> levelDataReferences;    // Drawn in inspector via editor script
    [HideInInspector] public int levelsCompleted = 0;

    private void Awake() {
        if (levelDataReferences == null) { levelDataReferences = new List<LevelDataListItem>(); }
    }

    public bool AllLevelsCompleted { get { return levelsCompleted >= levelDataReferences.Count && levelDataReferences.Count != 0; } }
    public LevelData NextLevel { get { return levelDataReferences[levelsCompleted].levelData; } }

    public LevelData GetLevelData(int index) {
        return levelDataReferences[index].levelData;
    }
}
