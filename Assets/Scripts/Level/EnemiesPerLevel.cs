using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemiesPerLevel
{
    public int basicEnemies;
    public int meleeEnemies;
    public int tankEnemies;
    public int snailEnemies;
    public int total
    {
        get
        {
            return basicEnemies + meleeEnemies + tankEnemies + snailEnemies;
        }
    }
}
