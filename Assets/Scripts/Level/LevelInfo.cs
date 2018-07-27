using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Level Info", menuName = "Custom/Level Info", order = 1)]
public class LevelInfo : ScriptableObject {

    public float levelSize;

    public int simpleEnemyAmount;
    public int laserEnemyAmount;
    public int meleeEnemyAmount;
    public int hoverEnemyAmount;
    public int tankEnemyAmount;
    public int bossEnemyAmount;

    public int TotalEnemyAmount {
        get {
            return simpleEnemyAmount + laserEnemyAmount + meleeEnemyAmount + hoverEnemyAmount + tankEnemyAmount + bossEnemyAmount;
        }
    }
}
