using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelBranchNode : MonoBehaviour {

    public LevelSet levelSet;
    [SerializeField] public LevelBranchNode[] branches;

    public abstract LevelBranchNode DetermineNext();

    public bool IsUnlocked {
        get { return MyMath.IntToBool(PlayerPrefs.GetInt(levelSet.name + "_unlocked")); }
        set { PlayerPrefs.SetInt(levelSet.name + "_unlocked", MyMath.BoolToInt(value)); }
    }
}
