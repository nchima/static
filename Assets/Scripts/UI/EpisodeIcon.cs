using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class EpisodeIcon : MonoBehaviour {

    [SerializeField] bool allowMouseSelection = true;
    [SerializeField] Collider mouseOverCollider;
    [SerializeField] GameObject unlockedIconMesh;
    [SerializeField] GameObject lockedIconMesh;
    public LevelBranchNode correspondingNode;
    [SerializeField] GameObject branchStem;
    [SerializeField] GameObject upperBranch;
    [SerializeField] GameObject lowerBranch;

    public enum SelectionState { Highlighted, Unhighlighted }
    public SelectionState selectionState = SelectionState.Unhighlighted;

    GameObject ActiveIconMesh {
        get {
            if (correspondingNode.IsUnlocked) { return unlockedIconMesh; }
            else { return lockedIconMesh; }
        }
    }

    [HideInInspector] public bool recieveMouseInput = false;
    bool IsMouseOverlapping {
        get {
            if (!recieveMouseInput || !allowMouseSelection) { return false; }

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
    public Text m_Text { get { return GetComponentInChildren<Text>(); } }

    [HideInInspector] public bool forceHighlighting = false;

    float MaterialThickness {
        get { return ActiveIconMesh.GetComponent<MeshRenderer>().material.GetFloat("_Thickness"); }
        set { ActiveIconMesh.GetComponent<MeshRenderer>().material.SetFloat("_Thickness", value); }
    }
    float materialThicknessBase = 2f;

    float currentRotationSpeed = 10f;

    private void Start() {
        // Activate the proper 3D icon and text
        UpdateVisuals();
    }

    private void Update() {
        switch (selectionState) {

            case SelectionState.Unhighlighted:

                // Switch to selected state
                if ((correspondingNode.IsUnlocked && IsMouseOverlapping) || forceHighlighting) {
                    selectionState = SelectionState.Highlighted;
                    ClearActiveTweens();
                    ActiveIconMesh.transform.parent.localScale = Vector3.one * 1.1f;
                    if (selectionCoroutine != null) { StopCoroutine(selectionCoroutine); }
                    selectionCoroutine = StartCoroutine(SelectionSequence());
                }
                break;

            case SelectionState.Highlighted:

                // Get clicked on
                if (InputManager.fireButtonDown && recieveMouseInput) {
                    Services.levelManager.SetCurrentBranchNode(correspondingNode);
                    GameEventManager.instance.FireEvent(new GameEvents.GameStarted());
                }

                if (!IsMouseOverlapping && !forceHighlighting) {
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

    public void HighlightUnlockedBranches() {
        // Light up the correct branches
        // Actually if this thingy is locked then don't do anything just stop stop stop
        if (!correspondingNode.IsUnlocked) { return; }

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
            SetBranchHighlight(lowerBranch, 0);
        }

        else if (correspondingNode.branches.Length == 1) {
            if (upperBranch.activeInHierarchy) { SetBranchHighlight(upperBranch, MyMath.BoolToInt(correspondingNode.branches[0].IsUnlocked)); }
            else { SetBranchHighlight(lowerBranch, MyMath.BoolToInt(correspondingNode.branches[0].IsUnlocked)); }
        }

        else {
            SetBranchHighlight(upperBranch, MyMath.BoolToInt(correspondingNode.branches[0].IsUnlocked));
            SetBranchHighlight(lowerBranch, MyMath.BoolToInt(correspondingNode.branches[1].IsUnlocked));
        }
    }

    public void UnHighlightAllBranches() {
        SetBranchHighlight(branchStem, 0);
        SetBranchHighlight(upperBranch, 0);
        SetBranchHighlight(lowerBranch, 0);
    }

    public void UpdateVisuals() {
        unlockedIconMesh.SetActive(correspondingNode.IsUnlocked);
        lockedIconMesh.SetActive(!correspondingNode.IsUnlocked);

        // Set name text
        if (correspondingNode.IsUnlocked) { m_Text.text = correspondingNode.levelSet.Name.ToUpper(); }
        else { m_Text.text = "LOCKED"; }
    }

    public void SetBranchHighlight(int branchIndex, int value) {
        SetBranchHighlight(branchStem, value);

        if (correspondingNode.branches.Length == 0) {
            return;
        }

        else if (correspondingNode.branches.Length == 1) {
            if (upperBranch.activeInHierarchy) { SetBranchHighlight(upperBranch, value); }
            else { SetBranchHighlight(lowerBranch, value); }
        }

        else {
            if (branchIndex == 0) { SetBranchHighlight(upperBranch, value); }
            else if (branchIndex == 1) { SetBranchHighlight(lowerBranch, value); }
        }
    }

    public void SetBranchHighlight(GameObject branchParent, int value) {
        foreach (Slider slider in branchParent.GetComponentsInChildren<Slider>()) {
            slider.value = value;
        }
    }

    public int GetBranchIndexToNode(LevelBranchNode node) {
        for (int i = 0; i < correspondingNode.branches.Length; i++) {
            if (correspondingNode.branches[i] == node) {
                return i;
            }
        }

        Debug.LogError(node.gameObject.name + " does not seem to connect with " + correspondingNode.gameObject.name);
        return 49324;
    }
}
