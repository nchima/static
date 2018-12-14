using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMap : MonoBehaviour {

    EpisodeIcon[] episodeIcons;

    private void Awake() {
        // Get references to all episode icons.
        episodeIcons = transform.GetChild(0).GetComponentsInChildren<EpisodeIcon>();
    }

    public void AllowSelection(bool value) {
        foreach (EpisodeIcon icon in episodeIcons) {
            icon.isSelectable = value;
        }
    }

    public void UnHighlightAll() {
        foreach(EpisodeIcon icon in episodeIcons) {
            icon.UnHighlightAllBranches();
        }
    }

    public void HighlightUnlockedPaths() {
        foreach(EpisodeIcon icon in episodeIcons) {
            icon.HighlightUnlockedBranches();
        }
    }

    public void HighlightPath() {
        foreach(LevelManager.ChosenPath chosenPath in Services.levelManager.chosenPaths) {
            EpisodeIcon icon = GetIconByCorrespondingNode(chosenPath.fromNode);
            icon.SetBranchHighlight(icon.GetBranchIndexToNode(chosenPath.toNode), 1);
            chosenPath.toNode.IsUnlocked = true;
            Services.uiManager.gameMap.GetComponent<GameMap>().GetIconByCorrespondingNode(chosenPath.toNode).UpdateVisuals();
        }
    }

    public EpisodeIcon GetIconByCorrespondingNode(LevelBranchNode node) {
        foreach (EpisodeIcon icon in episodeIcons) {
            if (icon.correspondingNode == node) {
                return icon;
            }
        }

        Debug.LogError("Tried to look up an icon with this node: " + node.name + " but could not find one.");
        return null;
    }

    // Prob not gonna use this after all:
    EpisodeIcon GetIconByNodeName(string nodeName) {
        foreach(EpisodeIcon icon in episodeIcons) {
            if (icon.correspondingNode.name == nodeName) {
                return icon;
            }
        }

        Debug.LogError("Tried to look up an icon with the name: " + nodeName + " but could not find one.");
        return null;
    }
}
