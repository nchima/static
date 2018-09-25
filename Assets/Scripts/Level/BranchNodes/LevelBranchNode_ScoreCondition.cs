using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

public class LevelBranchNode_ScoreCondition : LevelBranchNode {

    [SerializeField] int scoreToEnter;

    public override LevelBranchNode DetermineNext() {
        // This code only works if all adjoining nodes are of type ScoreCondition (This may be how the game actually is in the end.)
        LevelBranchNode nextBranch = branches[0];
        foreach (LevelBranchNode branch in branches) {
            if (!(branch is LevelBranchNode_ScoreCondition)) {
                Debug.LogError("Failed to determine next level set. One of the next level sets was not of ScoreCondition type.");
                return null;
            }

            else if (branch == nextBranch) {
                continue;
            }

            else {
                LevelBranchNode_ScoreCondition branchScoreCondition = branch as LevelBranchNode_ScoreCondition;
                LevelBranchNode_ScoreCondition nextBranchScoreCondition = nextBranch as LevelBranchNode_ScoreCondition;
                bool thisBranchIsHigher = branchScoreCondition.scoreToEnter > nextBranchScoreCondition.scoreToEnter;
                bool playerQualifies = Services.scoreManager.Score + Services.comboManager.GetMultipliedTotal() >= branchScoreCondition.scoreToEnter;
                if (thisBranchIsHigher && playerQualifies) {
                    nextBranch = branch;
                }
            }
        }

        return nextBranch;

        // Determine which of the potential paths are 

        // Deprecated
        //if (Services.scoreManager.Score + Services.comboManager.GetMultipliedTotal() >= scoreToEnterBranch2) { return branch2; }
        //else { return branch1; }
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
