using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Custom/Level Data")]
public class LevelData : ScriptableObject {

    public int buildIndex;
    public ColorPalette colorPalette;

    public string Path {
        get {
            return SceneUtility.GetScenePathByBuildIndex(buildIndex);
        }
    }

    public string LevelName {
        get {
            string[] pathWords = Path.Split('/');
            string name = pathWords[pathWords.Length - 1];
            string[] nameWords = name.Split('.');
            return nameWords[nameWords.Length - 2];
        }
    }
}
