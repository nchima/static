using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

public class LevelBranchNode_ScoreCondition : LevelBranchNode {

    [SerializeField] int scoreToEnterBranch2;

    public override LevelBranchNode DetermineNext() {
        if (Services.scoreManager.Score + Services.comboManager.GetMultipliedTotal() >= scoreToEnterBranch2) { return branch2; }
        else { return branch1; }
    }

#if UNITY_EDITOR
    public void TallyUpBaseScore() {
        int scoreValueTotal = 0;
        int bestMultiplier = 0;

        foreach(LevelDataListItem reference in levelSet.levelDataReferences) {

            Scene openedScene = EditorSceneManager.OpenScene(reference.levelData.Path, OpenSceneMode.Additive);

            scoreValueTotal += Services.comboManager.levelCompleteValue;

            Enemy[] enemies = FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in enemies) {
                scoreValueTotal += enemy.scoreKillValue;
                bestMultiplier++;
            }

            PointPickup[] pickups = FindObjectsOfType<PointPickup>();
            foreach(PointPickup pointPickup in pickups) {
                scoreValueTotal += pointPickup.scoreValue;
                bestMultiplier++;
            }

            EditorSceneManager.CloseScene(openedScene, true);
        }

        Debug.Log("A near-perfect run of this set would net the player: " + levelSet.name + ": " + scoreValueTotal * bestMultiplier);
    }
#endif
}
