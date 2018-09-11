using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelBranchNode : MonoBehaviour {

    public LevelSet levelSet;
    [SerializeField] protected LevelBranchNode branch1;
    [SerializeField] protected LevelBranchNode branch2;

    public abstract LevelBranchNode DetermineNext();
}
