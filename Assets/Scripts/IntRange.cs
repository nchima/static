using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class IntRange {

    public int min;
    public int max;

    public IntRange(int _min, int _max)
    {
        min = _min;
        max = _max;
    }

    public float Random
    {
        get { return UnityEngine.Random.Range(min, max+1); }
    }
}
