using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Custom/Level Data")]
public class LevelData : ScriptableObject {
    public int buildIndex;
    public ColorPalette colorPalette;
    public string path {
        get {
            return SceneUtility.GetScenePathByBuildIndex(buildIndex);
        }
    }
}
