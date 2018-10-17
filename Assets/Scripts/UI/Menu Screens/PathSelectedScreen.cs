using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathSelectedScreen : MonoBehaviour {

    [SerializeField] EpisodeIcon clearedEpisodeIcon;
    [SerializeField] EpisodeIcon leftBranchIcon;
    [SerializeField] EpisodeIcon rightBranchIcon;

    [HideInInspector] public bool pathSelectedTrigger = false;
    int selectedBranchIndex = 0;

    enum SelectionState { Inactive, Active }
    SelectionState selectionState;

    float selectTime = 5f;
    float selectTimer = 0f;

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasTased>(PlayerWasTasedHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasTased>(PlayerWasTasedHandler);
    }

    public void Initialize(LevelBranchNode clearedEpisodeNode) {

        SetUpIcon(clearedEpisodeIcon, clearedEpisodeNode);

        // I'll need to have more specialized code for this case eventually.
        if (clearedEpisodeNode.branches.Length == 1) {
            leftBranchIcon.gameObject.SetActive(false);
            SetUpIcon(leftBranchIcon, clearedEpisodeNode.branches[0]);
        }

        else if (clearedEpisodeNode.branches.Length == 2) {
            SetUpIcon(leftBranchIcon, clearedEpisodeNode.branches[0]);
            SetUpIcon(rightBranchIcon, clearedEpisodeNode.branches[1]);
        }

        pathSelectedTrigger = false;
        selectTimer = 0f;

        //selectedBranchIndex = Random.Range(0, 2);
        //HighlightPath(selectedBranchIndex, false);
        //HighlightPath(MyMath.Wrap01(selectedBranchIndex++), false);
        selectionState = SelectionState.Active;
    }

    void Update() {
        switch (selectionState) {
            case SelectionState.Inactive:
                // Don't do anything.
                break;
            case SelectionState.Active:
                WaitForSelection();
                break;
        }
    }

    void WaitForSelection() {
        // Increase selection timer.
        selectTimer += Time.deltaTime;
        // For now this does nothing, but it should pressure the player into choosing quickly.
        // Idea: player is tased, and a random path is chosen if they don't make a decision in time.

        // Recieve input from player to select path.
        if (clearedEpisodeIcon.correspondingNode.branches.Length == 1) {
            // In this case, keep the one branch constantly highlighted but I won't worry about this yet because yolo
        }

        // If there are two potential paths use directional input to select between them.
        else {
            if (InputManager.movementAxis.x < -0.5f) {
                SelectBranch(0);
            }
            else if (InputManager.movementAxis.x > 0.5f) {
                SelectBranch(1);
            }
        }

        // If the player presses the fire button then choose the currently selected branch.
        if (InputManager.fireButtonDown) {
            MakeSelection();
        }
    }

    void SelectBranch(int branchIndex) {
        if (clearedEpisodeIcon.correspondingNode.branches.Length == 1) {
            // In this case, keep the one branch constantly highlighted but I won't worry about this yet because yolo
        }

        // If there are two potential paths use directional input to select between them.
        else {
            if (branchIndex == 0) {
                selectedBranchIndex = 0;
                HighlightPath(0, true);
                HighlightPath(1, false);
            }
            else if (branchIndex == 1) {
                selectedBranchIndex = 1;
                HighlightPath(0, false);
                HighlightPath(1, true);
            }
        }
    }

    void MakeSelection() {
        if (selectionState != SelectionState.Active) { return; }
        if (GetSelectedNode() == null) { return; }

        pathSelectedTrigger = true;
        selectionState = SelectionState.Inactive;
    }

    void SetUpIcon(EpisodeIcon targetIcon, LevelBranchNode correspondingNode) {
        targetIcon.correspondingNode = correspondingNode;
        targetIcon.m_Text.text = correspondingNode.levelSet.Name;

        // Also give the icons the proper meshes (Once I've actually implemented them.)
    }

    // For path index, left = 0 and right = 1
    void HighlightPath(int pathIndex, bool isHighlighted) {
        clearedEpisodeIcon.SetBranchHighlight(pathIndex, MyMath.BoolToInt(isHighlighted));
        if (pathIndex == 0) { leftBranchIcon.forceHighlighting = isHighlighted; }
        else { rightBranchIcon.forceHighlighting = isHighlighted; }
    }

    public LevelBranchNode GetSelectedNode() {
        if (selectedBranchIndex == 0) { return leftBranchIcon.correspondingNode; }
        else if (selectedBranchIndex == 1) { return rightBranchIcon.correspondingNode; }
        else return null;
    }

    public void PlayerWasTasedHandler(GameEvent gameEvent) {
        if (selectionState == SelectionState.Active) {
            SelectBranch(Random.Range(0, 2));
            MakeSelection();
        }
    }
}
