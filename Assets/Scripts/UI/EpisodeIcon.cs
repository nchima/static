using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class EpisodeIcon : MonoBehaviour {

    [SerializeField] Collider mouseOverCollider;
    [SerializeField] GameObject unlockedIconMesh;
    [SerializeField] GameObject lockedIconMesh;
    [SerializeField] LevelBranchNode correspondingNode;
    [SerializeField] GameObject branchStem;
    [SerializeField] GameObject upperBranch;
    [SerializeField] GameObject middleBranch;
    [SerializeField] GameObject lowerBranch;

    enum SelectionState { Highlighted, Unhighlighted }
    SelectionState selectionState = SelectionState.Unhighlighted;

    GameObject ActiveIconMesh { get {
            if (correspondingNode.IsUnlocked) { return unlockedIconMesh; }
            else { return lockedIconMesh; }
        } }

	bool IsMouseOverlapping {
        get {
            Ray mouseRay = Services.finalCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(mouseRay, out hit, 100f)) {
                if (hit.collider == mouseOverCollider) {
                    return true;
                }
            }

            return false;
        }
    }
    Text m_Text { get { return GetComponentInChildren<Text>(); } }

    float MaterialThickness {
        get { return ActiveIconMesh.GetComponent<MeshRenderer>().material.GetFloat("_Thickness"); }
        set { ActiveIconMesh.GetComponent<MeshRenderer>().material.SetFloat("_Thickness", value); }
    }
    float materialThicknessBase = 2f;

    float currentRotationSpeed = 10f;

    private void Start() {
        // Set name text
        if (correspondingNode.IsUnlocked) { m_Text.text = correspondingNode.levelSet.Name.ToUpper(); }
        else { m_Text.text = "???"; }

        // Activate the proper 3D icon
        unlockedIconMesh.SetActive(correspondingNode.IsUnlocked);
        lockedIconMesh.SetActive(!correspondingNode.IsUnlocked);

        // Light up the correct branches
        // If this node has no branches, just deactivate all sliders.
        bool anyBranchUnlocked = false;
        for (int i = 0; i < correspondingNode.branches.Length; i++) {
            if (correspondingNode.branches[i].IsUnlocked) {
                anyBranchUnlocked = true;
                break;
            }
        }

        SetBranchHighlight(branchStem, MyMath.BoolToInt(anyBranchUnlocked));

        if (correspondingNode.branches.Length == 0) {
            SetBranchHighlight(upperBranch, 0);
            SetBranchHighlight(middleBranch, 0);
            SetBranchHighlight(lowerBranch, 0);
        }

        else if (correspondingNode.branches.Length == 2) {
            if (upperBranch.activeInHierarchy) {
                SetBranchHighlight(upperBranch, MyMath.BoolToInt(correspondingNode.branches[0].IsUnlocked));
                SetBranchHighlight(middleBranch, MyMath.BoolToInt(correspondingNode.branches[1].IsUnlocked));
            }

            else {
                SetBranchHighlight(middleBranch, MyMath.BoolToInt(correspondingNode.branches[0].IsUnlocked));
                SetBranchHighlight(lowerBranch, MyMath.BoolToInt(correspondingNode.branches[1].IsUnlocked));
            }
        }

        else if (correspondingNode.branches.Length == 3) {
            SetBranchHighlight(upperBranch, MyMath.BoolToInt(correspondingNode.branches[0].IsUnlocked));
            SetBranchHighlight(middleBranch, MyMath.BoolToInt(correspondingNode.branches[1].IsUnlocked));
            SetBranchHighlight(lowerBranch, MyMath.BoolToInt(correspondingNode.branches[2].IsUnlocked));
        }
    }

    private void Update() {
        switch (selectionState) {

            case SelectionState.Unhighlighted:

                // Switch to selected state
                if (correspondingNode.IsUnlocked && IsMouseOverlapping) {
                    selectionState = SelectionState.Highlighted;
                    ClearActiveTweens();
                    ActiveIconMesh.transform.parent.localScale = Vector3.one * 1.1f;
                    if (selectionCoroutine != null) { StopCoroutine(selectionCoroutine); }
                    selectionCoroutine = StartCoroutine(SelectionSequence());
                }
                break;

            case SelectionState.Highlighted:

                // Get clicked on
                if (InputManager.fireButtonDown) {
                    Services.levelManager.SetStartingLevelSet(correspondingNode.levelSet);
                    GameEventManager.instance.FireEvent(new GameEvents.GameStarted());
                }

                if (!IsMouseOverlapping) {
                    selectionState = SelectionState.Unhighlighted;
                    ClearActiveTweens();
                    ActiveIconMesh.transform.parent.localScale = Vector3.one;
                    if (selectionCoroutine != null) { StopCoroutine(selectionCoroutine); }
                    selectionCoroutine = StartCoroutine(DeselectionSequence());
                }
                break;
        }

        // Update material thickness
        MaterialThickness = MyMath.ClampPositive(materialThicknessBase + Random.Range(-0.5f, 0.5f));

        // Rotate
        ActiveIconMesh.transform.parent.Rotate(transform.forward, currentRotationSpeed * Time.deltaTime);
    }

    Coroutine selectionCoroutine;
    List<Tween> activeTweens = new List<Tween>();
    IEnumerator SelectionSequence() {
        // Rise above
        float duration = 0.1f;
        activeTweens.Add(DOTween.To(() => materialThicknessBase, x => materialThicknessBase = x, 20f, duration));
        activeTweens.Add(DOTween.To(() => currentRotationSpeed, x => currentRotationSpeed = x, 60f, duration));
        yield return new WaitForSeconds(duration);

        // Return to base
        duration = 0.4f;
        activeTweens.Add(DOTween.To(() => materialThicknessBase, x => materialThicknessBase = x, 7f, duration).SetEase(Ease.OutExpo));
        yield return new WaitForSeconds(duration);

        yield return null;
    }
    IEnumerator DeselectionSequence() {
        // Lower
        float duration = 0.28f;
        activeTweens.Add(DOTween.To(() => materialThicknessBase, x => materialThicknessBase = x, 1.5f, duration));
        activeTweens.Add(DOTween.To(() => currentRotationSpeed, x => currentRotationSpeed = x, 10f, duration));
        yield return new WaitForSeconds(duration);

        yield return null;
    }

    void ClearActiveTweens() {
        foreach (Tween tween in activeTweens) { tween.Kill(); }
        activeTweens.Clear();
    }

    void SetBranchHighlight(GameObject branchParent, int value) {
        foreach (Slider slider in branchParent.GetComponentsInChildren<Slider>()) {
            slider.value = value;
        }
    }
}
