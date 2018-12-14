using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelBranchNode : MonoBehaviour {

    public LevelSet levelSet;
    [SerializeField] public LevelBranchNode[] branches;

    [HideInInspector] public LevelBranchNode previousNode;

    public abstract LevelBranchNode DetermineNext();
    public EpisodeIcon correspondingIcon { get { return Services.uiManager.gameMap.GetComponent<GameMap>().GetIconByCorrespondingNode(this); } }

    public bool IsUnlocked {
        get { return MyMath.IntToBool(PlayerPrefs.GetInt(levelSet.name + "_unlocked")); }
        set { PlayerPrefs.SetInt(levelSet.name + "_unlocked", MyMath.BoolToInt(value)); }
    }

    public void Awake() {
        foreach(LevelBranchNode branch in branches) {
            branch.previousNode = this;
        }
    }
}
