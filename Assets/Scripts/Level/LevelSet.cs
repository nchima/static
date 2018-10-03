using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Level Set")]
public class LevelSet : ScriptableObject {

    public ColorPalette colorPalette;

    [HideInInspector] public List<LevelDataListItem> levelDataReferences;    // Drawn in inspector via editor script
    [HideInInspector] public int levelsCompleted = 0;
    public bool AllLevelsCompleted { get { return levelsCompleted >= levelDataReferences.Count && levelDataReferences.Count != 0; } }
    public LevelData NextLevel { get { return levelDataReferences[levelsCompleted].levelData; } }
    public string Name {
        get {
            string firstWordOfName = "";
            for (int i = 0; i < this.name.Length; i++) {
                if (this.name[i] != ' ') { firstWordOfName += this.name[i]; }
                else { break; }
            }
            return firstWordOfName;
        }
    }

    private void Awake() {
        if (levelDataReferences == null) { levelDataReferences = new List<LevelDataListItem>(); }
    }

    public LevelData GetLevelData(int index) {
        return levelDataReferences[index].levelData;
    }
}
