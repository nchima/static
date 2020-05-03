using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class WadImporter {

    public static void ImportWad() {
        // Allow the player to select a wad file to import.
        string wadPath = EditorUtility.OpenFilePanel("Select a wad file to import", "", "wad");
        if (wadPath.Length == 0) { return; }

        // Make sure that the selected file was actually a wad.
        string fileExtension = wadPath.Substring(wadPath.Length - 3);
        if (fileExtension != "wad") {
            EditorUtility.DisplayDialog("Import wad", "You selected a file that wasn't a wad.", "Damn, I fucked up.");
            return;
        }

        // Get the name of the wad.
        string[] wadPathSplit = wadPath.Split('/');
        string wadName = wadPathSplit[wadPathSplit.Length - 1];
        wadName = wadName.Split('.')[0];

        // Get the path to where we would save the new scene.
        string saveScenePath = "Assets/Scenes/Levels/Doom Rips/" + wadName + ".unity";

        // See if a scene with the name of this wad has already been imported.
        bool sceneNameExistsInBuildSettings = false;
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++) {
            if (EditorBuildSettings.scenes[i].path == saveScenePath) {
                sceneNameExistsInBuildSettings = true;
                break;
            }
        }

        // If a scene with the name of this wad has already been imported, ask the player if they want to overwrite it.
        if (sceneNameExistsInBuildSettings) {
            if (!EditorUtility.DisplayDialog("Overwrite previously imported wad?", "A scene with the same name as the wad you are trying to import already exists. If you continue you will overwrite the previously imported scene. Are you sure you want to do this?", "Yes", "No")) {
                EditorUtility.DisplayDialog("Import cancelled.", "Importing has been cancelled.", "OK");
                return;
            }
        }

        // If a scene with this name is already open, get a reference to it.
        Scene importScene = new Scene();
        bool isSceneAlreadyOpen = false;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++) {
            if (EditorSceneManager.GetSceneAt(i).name == wadName) {
                isSceneAlreadyOpen = true;
                // Make sure scene is loaded.
                importScene = EditorSceneManager.OpenScene(saveScenePath, OpenSceneMode.Additive);
                break;
            }
        }

        // If the scene exists in build settings but is not open, open it.
        if (sceneNameExistsInBuildSettings && !isSceneAlreadyOpen) {
            EditorSceneManager.OpenScene(saveScenePath, OpenSceneMode.Additive);
            importScene = EditorSceneManager.GetSceneByPath(saveScenePath);
        }

        // If it's not alrady open, create a new scene with that name.
        else if (!isSceneAlreadyOpen) {
            importScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        }

        EditorSceneManager.SetActiveScene(importScene);

        // If the scene already contains a Doom wad loader, destroy it.
        GameObject[] sceneObjects = importScene.GetRootGameObjects();
        for (int i = 0; i < sceneObjects.Length; i++) {
            if (sceneObjects[i].GetComponent<WadLoader>()) {
                GameObject.DestroyImmediate(sceneObjects[i]);
                continue;
            }
            if (sceneObjects[i].scene == importScene && sceneObjects[i].name.ToUpper() == "LEVEL") {
                GameObject.DestroyImmediate(sceneObjects[i]);
            }
        }

        // Instantiate the Doom wad loader gameobject into the new scene and use it to import the wad.
        Object wadLoaderPrefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Doom Loading/Doom Loader.prefab", typeof(Object));
        WadLoader wadLoader = (PrefabUtility.InstantiatePrefab(wadLoaderPrefab as GameObject, importScene) as GameObject).GetComponent<WadLoader>();
        wadLoader.filePath = wadPath;
        wadLoader.wadName = wadName;
        wadLoader.autoloadMapName = "MAP01";
        wadLoader.scene = importScene;
        wadLoader.ConvertMap();

        // Destroy the doom loader game object.
        GameObject.DestroyImmediate(wadLoader.gameObject);

        // Save the scene.
        EditorSceneManager.SaveScene(importScene, saveScenePath);

        // If a scene of this name has not already been added to build settings, add it now.
        if (!sceneNameExistsInBuildSettings) {
            List<EditorBuildSettingsScene> buildSettingsScenes = EditorBuildSettings.scenes.ToList();
            buildSettingsScenes.Add(new EditorBuildSettingsScene(saveScenePath, true));
            EditorBuildSettings.scenes = buildSettingsScenes.ToArray();
        }

        // Check to see whether a level data already exists for this wad.
        bool levelDataExists = false;
        string[] levelDataFilePaths = Directory.GetFiles("Assets/Level Data/", "*.asset", SearchOption.AllDirectories);
        for (int i = 0; i < levelDataFilePaths.Length; i++) {
            if (levelDataFilePaths[i].Contains(wadName)) {
                levelDataExists = true;
                return;
            }
        }

        // Create a level data asset.
        if (!levelDataExists) {
            LevelData newLevelData = ScriptableObject.CreateInstance(typeof(LevelData)) as LevelData;
            newLevelData.buildIndex = EditorBuildSettings.scenes.Length - 1;
            AssetDatabase.CreateAsset(newLevelData, "Assets/Level Data/Newly Imported/" + wadName + ".asset");
            EditorGUIUtility.PingObject(newLevelData);
        }

        EditorUtility.DisplayDialog("Success", "Import successful. Please note: from this point on, if you rename any scene files or move them around in the build order everything will get fucked up. So please don't do that", "I promise I won't!");
    }
}
