using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class FloatRange {

    public float min;
    public float max;

    public FloatRange(float _min, float _max)
    {
        min = _min;
        max = _max;
    }

    public float Random
    {
        get { return UnityEngine.Random.Range(min, max); }
    }
}
