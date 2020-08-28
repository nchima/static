using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathSelectedScreen : MonoBehaviour {

    [SerializeField] EpisodeIcon clearedEpisodeIcon;
    [SerializeField] EpisodeIcon upperBranchIcon;
    [SerializeField] EpisodeIcon lowerBranchIcon;

    [HideInInspector] public bool pathSelectedTrigger = false;
    int selectedBranchIndex = 0;

    enum SelectionState { Inactive, Active }
    SelectionState selectionState;

    enum BonusType { /*Health, */MachineGun, SniperRifle, Shotgun, Pulse, Saw, Rocket }
    BonusType upperBonus;
    BonusType lowerBonus;

    float selectTime = 5f;
    float selectTimer = 0f;

    bool isUsingRightAnalogStickForControllerSelection = false;

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
            upperBranchIcon.gameObject.SetActive(false);
            SetUpIcon(upperBranchIcon, clearedEpisodeNode.branches[0]);
        }

        else if (clearedEpisodeNode.branches.Length == 2) {
            SetUpIcon(upperBranchIcon, clearedEpisodeNode.branches[0]);
            SetUpIcon(lowerBranchIcon, clearedEpisodeNode.branches[1]);
        }

        pathSelectedTrigger = false;
        selectTimer = 0f;

        // Determine bonuses for each path
        int enumCount = System.Enum.GetValues(typeof(BonusType)).Length;
        upperBonus = (BonusType)Random.Range(0, enumCount);
        for (int i = 0; i < 100; i++) {
            if(!IsBonusValid(upperBonus, true)) {
                upperBonus = (BonusType)Random.Range(0, enumCount); Debug.Log("Upper bonus: " + upperBonus.ToString());
            }
            else { break; }
        }

        lowerBonus = upperBonus;
        for (int i = 0; i < 100; i++) {
            if(!IsBonusValid(lowerBonus, false)) {
                lowerBonus = (BonusType)MyMath.Wrap((int)upperBonus + Random.Range(0, enumCount), 0, enumCount);
            }
            else { break; }
        }

        Debug.Log("Upper Bonus: " + upperBonus.ToString() + ". Lower Bonus: " + lowerBonus.ToString());

        //upperBranchIcon.transform.Find("Bonus Description").GetComponent<Text>().text = GetBonusTypeDescription(upperBonus, true);
        //lowerBranchIcon.transform.Find("Bonus Description").GetComponent<Text>().text = GetBonusTypeDescription(lowerBonus, false);

        selectedBranchIndex = Random.Range(0, 2);
        HighlightPath(selectedBranchIndex, false);
        HighlightPath(MyMath.Wrap01(selectedBranchIndex++), false);
        selectionState = SelectionState.Active;
    }

    bool IsBonusValid(BonusType bonusType, bool isUpper) {

        switch (bonusType) {
            case BonusType.MachineGun:
                if (Services.gun.upperWeapon == Services.gun.machineGun || Services.gun.lowerWeapon == Services.gun.machineGun) {
                    return false;
                }
                break;
            case BonusType.Shotgun:
                if (Services.gun.upperWeapon == Services.gun.shotgunWeapon || Services.gun.lowerWeapon == Services.gun.shotgunWeapon) {
                    return false;
                }
                break;
            case BonusType.SniperRifle:
                if (Services.gun.upperWeapon == Services.gun.sniperRifle || Services.gun.lowerWeapon == Services.gun.sniperRifle) {
                    return false;
                }
                break;
            case BonusType.Pulse:
                if (Services.gun.upperWeapon == Services.gun.pulseWeapon || Services.gun.lowerWeapon == Services.gun.pulseWeapon) {
                    return false;
                }
                break;
            case BonusType.Saw:
                if (Services.gun.upperWeapon == Services.gun.sawWeapon || Services.gun.lowerWeapon == Services.gun.sawWeapon) {
                    return false;
                }
                break;
            case BonusType.Rocket:
                if (Services.gun.upperWeapon == Services.gun.rocketWeapon || Services.gun.lowerWeapon == Services.gun.rocketWeapon) {
                    return false;
                }
                break;
            default:
                break;
        }

        if (!isUpper) {
            if (upperBonus == lowerBonus) {
                return false;
            }
        }

        return true;
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
            if (InputManager.inputMode == InputManager.InputMode.MouseAndKeyboard) {
                if (GunValueManager.currentValue >= 0f) {
                    HighlightBranch(0);
                }
                else {
                    HighlightBranch(1);
                }
            }

            else if (InputManager.inputMode == InputManager.InputMode.Controller) {

                // See which analog stick we should use for selection.
                if (isUsingRightAnalogStickForControllerSelection) {
                    if (Mathf.Abs(InputManager.movementAxis.y) > 0.5f) {
                        isUsingRightAnalogStickForControllerSelection = false;
                    }
                }
                else {
                    if (Mathf.Abs(InputManager.gunTuningValue) > 0.5f) {
                        isUsingRightAnalogStickForControllerSelection = true;
                    }
                }

                if (isUsingRightAnalogStickForControllerSelection) {
                    if (GunValueManager.currentValue >= 0f) {
                        HighlightBranch(0);
                    }
                    else {
                        HighlightBranch(1);
                    }
                }

                else {
                    if (InputManager.movementAxis.y > 0.5f) {
                        HighlightBranch(0);
                    }
                    else if (InputManager.movementAxis.y < -0.5f) {
                        HighlightBranch(1);
                    }
                }
            }
        }

        // If the player presses the fire button then choose the currently selected branch.
        if (InputManager.fireButtonDown || InputManager.submitButtonDown) {
            MakeSelection();
        }
    }

    void HighlightBranch(int branchIndex) {
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

    // For path index, up = 0 and down = 1
    void HighlightPath(int pathIndex, bool isHighlighted) {
        clearedEpisodeIcon.SetBranchHighlight(pathIndex, MyMath.BoolToInt(isHighlighted));
        if (pathIndex == 0 && isHighlighted) {
            upperBranchIcon.forceHighlighting = true;
            lowerBranchIcon.forceHighlighting = false;
        }
        else if (pathIndex == 1 && isHighlighted) {
            upperBranchIcon.forceHighlighting = false;
            lowerBranchIcon.forceHighlighting = true;
        }
    }


    void MakeSelection() {
        if (selectionState != SelectionState.Active) { return; }
        if (GetSelectedNode() == null) { return; }

        else if (GetSelectedNode() == upperBranchIcon.correspondingNode) { ApplyBonus(upperBonus, true); }
        else if (GetSelectedNode() == lowerBranchIcon.correspondingNode) { ApplyBonus(lowerBonus, false); }

        pathSelectedTrigger = true;
        selectionState = SelectionState.Inactive;
    }

    void ApplyBonus(BonusType bonusType, bool isUpper) {

        Gun.WeaponPosition weaponPosition = Gun.WeaponPosition.Upper;
        if (!isUpper) { weaponPosition = Gun.WeaponPosition.Lower; }

        switch (bonusType) {
            //case BonusType.Health:
            //    Services.healthManager.AddMaxHealth();
            //    break;
            case BonusType.MachineGun:
                Services.gun.SwitchWeapon(weaponPosition, Services.gun.machineGun);
                break;
            case BonusType.Shotgun:
                Services.gun.SwitchWeapon(weaponPosition, Services.gun.shotgunWeapon);
                break;
            case BonusType.SniperRifle:
                Services.gun.SwitchWeapon(weaponPosition, Services.gun.sniperRifle);
                break;
            case BonusType.Pulse:
                Services.gun.SwitchWeapon(weaponPosition, Services.gun.pulseWeapon);
                break;
            case BonusType.Saw:
                Services.gun.SwitchWeapon(weaponPosition, Services.gun.sawWeapon);
                break;
            case BonusType.Rocket:
                Services.gun.SwitchWeapon(weaponPosition, Services.gun.rocketWeapon);
                break;
        }

    }

    void SetUpIcon(EpisodeIcon targetIcon, LevelBranchNode correspondingNode) {
        targetIcon.correspondingNode = correspondingNode;
        targetIcon.m_Text.text = correspondingNode.levelSet.Name;

        // Also give the icons the proper meshes (Once I've actually implemented them.)
    }

    public LevelBranchNode GetSelectedNode() {
        if (selectedBranchIndex == 0) { return upperBranchIcon.correspondingNode; }
        else if (selectedBranchIndex == 1) { return lowerBranchIcon.correspondingNode; }
        else return null;
    }

    public void PlayerWasTasedHandler(GameEvent gameEvent) {
        if (selectionState == SelectionState.Active) {
            HighlightBranch(Random.Range(0, 2));
            MakeSelection();
        }
    }

    string GetBonusTypeDescription(BonusType bonusType, bool isUpper) {

        string weaponPosition = "UPPER";
        if (!isUpper) { weaponPosition = "LOWER"; }

        switch(bonusType) {
            //case BonusType.Health:
            //    return "+1 HEALTH";
            case BonusType.MachineGun:
                return weaponPosition + " WEAPON BECOMES " + Services.gun.machineGun.displayName + ". " /*+ Services.gun.machineGun.description + ". " */;
            case BonusType.Shotgun:
                return weaponPosition + " WEAPON BECOMES " + Services.gun.shotgunWeapon.displayName + ". " /*+ Services.gun.shotgunWeapon.description + ". " */;
            case BonusType.SniperRifle:
                return weaponPosition + " WEAPON BECOMES " + Services.gun.sniperRifle.displayName + ". " /*+ Services.gun.sniperRifle.description + ". " */;
            case BonusType.Pulse:
                return weaponPosition + " WEAPON BECOMES " + Services.gun.pulseWeapon.displayName + ". " /*+ Services.gun.pulseWeapon.description + ". " */;
            case BonusType.Saw:
                return weaponPosition + " WEAPON BECOMES " + Services.gun.sawWeapon.displayName + ". " /*+ Services.gun.sawWeapon.description + ". " */;
            case BonusType.Rocket:
                return weaponPosition + " WEAPON BECOMES " + Services.gun.rocketWeapon.displayName + ". " /*+ Services.gun.rocketWeapon.description + ". " */;
            default:
                return "MEOW MEOW, TIRED COW";
        }
    }
}
